using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem;

    using Vermines.Gameplay.Errors;
    using Vermines.Player;

    /// <summary>
    /// Back-end part of <see cref="GameManager" /> containing all RPC methods responsible for validating players actions on the server side.
    /// </summary>
    /// <remarks>
    /// 
    /// <para>
    /// This file (<c>GameManager.Backend.cs</c>) only contains network calls (RPC) to the state authority (server).
    /// Its role is to pre-simulate and verify the actions sent by players before they are actually executed in the game.
    /// </para>
    /// 
    /// <para>
    /// The main responsibilities are:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     Check whether a player is allowed to perform an action (e.g., is it their turn?, do they have enough resources?, does the targeted card still exist?).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Execute server-side commands to ensure the consistency and integrity of the game rules.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Transmit the result of the validated action to other players via client-side RPCs (PlayerController.Local).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Handle errors by returning a network return structure containing:
    ///       <list type="number">
    ///         <item>
    ///           <description>
    ///           The severity of the error (Minor, Major, Critical)
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///           The location of the error (e.g. Shop, Main, Book, Table, etc.)
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///           The explanatory message for the UI
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///           Possibly additional data (e.g. ID of the card concerned)
    ///           </description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// Errors can be returned to players using the following method:
    /// <c>PlayerController.Local.RPC_ReceiveError(GameActionError)</c>,
    /// </para>
    /// </remarks>
    public partial class GameManager : NetworkBehaviour {

        #region Notifiers

        private void SendError(GameActionError error)
        {
            PlayerController.Local.RPC_ReceiveError(error);
        }

        #endregion

        #region Shop

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_BuyCard(ShopType shopType, int slot, int playerId)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError {
                    Scope      = ErrorScope.Local,
                    Target     = playerSource,
                    Severity   = ErrorSeverity.Minor,
                    Location   = ErrorLocation.Shop,
                    MessageKey = "Shop_Buy_NotYourTurn"
                });

                return;
            }

            ShopArgs parameters = new(GameDataStorage.Instance.Shop, shopType, slot);

            ICommand                checker = new ADMIN_CheckBuyCommand(playerSource, GetCurrentPhase(), parameters);
            CommandResponse checkerResponse = CommandInvoker.ExecuteCommand(checker);

            if (checkerResponse.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope  = ErrorScope.Local,
                    Target = playerSource,

                    Severity = checkerResponse.Status == CommandStatus.Invalid ? ErrorSeverity.Minor :
                               checkerResponse.Status == CommandStatus.Failure ? ErrorSeverity.Minor :
                               ErrorSeverity.Critical,

                    Location    = ErrorLocation.Shop,
                    MessageKey  = checkerResponse.Message,
                    MessageArgs = new GameActionErrorArgs(checkerResponse.Args)
                });

                return;
            }

            ICommand buyCommand = new ADMIN_BuyCommand(playerSource, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            Debug.Log($"[SERVER]: {CommandInvoker.State.Message}");

            PlayerController.Local.RPC_BuyCard(playerId, shopType, slot);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ReplaceCardInShop(int playerId, ShopType shopType, int slot)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope      = ErrorScope.Local,
                    Target     = playerSource,
                    Severity   = ErrorSeverity.Major,
                    Location   = ErrorLocation.Shop,
                    MessageKey = "Shop_Replace_NotYourTurn"
                });

                return;
            }

            ICommand        checker  = new ADMIN_CheckChangeCardCommand(new ShopArgs(GameDataStorage.Instance.Shop, shopType, slot));
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) { // Can only failed if the shop or the slot selected is empty. So if it's a cheat, bug or a desync.
                SendError(new GameActionError {
                    Scope       = ErrorScope.Local,
                    Target      = playerSource,
                    Severity    = ErrorSeverity.Critical,
                    Location    = ErrorLocation.Shop,
                    MessageKey  = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            PlayerController.Local.RPC_ReplaceCardInShop(playerId, shopType, slot);
        }

        #endregion

        #region Table

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Table,
                    MessageKey = $"You cannot played a card when it's not your turn." // TODO: Localize the message. (fr, en...).
                });

                return;
            }

            Player.PlayerController.Local.RPC_CardPlayed(playerId, cardId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_DiscardCard(int playerId, int cardID)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError {
                    Scope    = ErrorScope.Local,
                    Target   = playerSource,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Table,
                    MessageKey  = $"You cannot discarded a card when it's not your turn." // TODO: Localize the message. (fr, en...).
                });

                return;
            }

            Player.PlayerController.Local.RPC_DiscardCard(playerId, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_DiscardCardNoEffect(int playerId, int cardID)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError {
                    Scope    = ErrorScope.Local,
                    Target   = playerSource,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Table,
                    MessageKey  = $"You cannot discarded a card when it's not your turn." // TODO: Localize the message. (fr, en...).
                });

                return;
            }

            Player.PlayerController.Local.RPC_DiscardCardNoEffect(playerId, cardID);
        }

        #endregion

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardSacrified(int playerId, int cardId)
        {
            Player.PlayerController.Local.RPC_CardSacrified(playerId, cardId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ActivateEffect(int playerID, int cardID)
        {
            Player.PlayerController.Local.RPC_ActivateEffect(playerID, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ReducedInSilenced(int playerId, int cardToBeSilenced)
        {
            Player.PlayerController.Local.RPC_ReducedInSilenced(playerId, cardToBeSilenced);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemoveReducedInSilenced(int playerId, int cardID, int originalSouls)
        {
            Player.PlayerController.Local.RPC_RemoveReducedInSilenced(playerId, cardID, originalSouls);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CopiedEffect(int playerId, int cardID, int cardToCopiedID)
        {
            Player.PlayerController.Local.RPC_CopiedEffect(playerId, cardID, cardToCopiedID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemoveCopiedEffect(int playerId, int cardID)
        {
            Player.PlayerController.Local.RPC_RemoveCopiedEffect(playerId, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_NetworkEventCardEffect(int playerID, int cardID, string data)
        {
            Player.PlayerController.Local.RPC_NetworkEventCardEffect(playerID, cardID, data);
        }
    }
}
