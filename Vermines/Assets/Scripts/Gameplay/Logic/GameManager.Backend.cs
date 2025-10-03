using Fusion;
using OMGG.DesignPattern;

namespace Vermines {
    using Vermines.Gameplay.Commands;
    using Vermines.Gameplay.Errors;
    using Vermines.Player;
    using Vermines.ShopSystem;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;

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

        #region Table (Play, Discard)

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope      = ErrorScope.Local,
                    Target     = playerSource,
                    Severity   = ErrorSeverity.Major,
                    Location   = ErrorLocation.Table,
                    MessageKey = "Table_Play_NotYourTurn",
                    MessageArgs = new GameActionErrorArgs(cardId.ToString())
                });

                return;
            }

            ICommand        checker         = new ADMIN_CheckPlayCommand(playerSource, GetCurrentPhase(), cardId);
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,

                    Severity = response.Status == CommandStatus.Invalid ? ErrorSeverity.Minor :
                               response.Status == CommandStatus.Failure ? ErrorSeverity.Minor :
                               ErrorSeverity.Critical,

                    Location = ErrorLocation.Table,
                    MessageKey = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }
            PlayerController.Local.RPC_CardPlayed(playerId, cardId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_DiscardCard(int playerId, int cardID, bool hasEffect = true)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope    = ErrorScope.Local,
                    Target   = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Discard,
                    MessageKey  = "Table_Discard_NotYourTurn",
                    MessageArgs = new GameActionErrorArgs(cardID.ToString())
                });

                return;
            }

            ICommand        checker  = new ADMIN_CheckDiscardCommand(playerSource, GetCurrentPhase(), cardID);
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,

                    Severity = response.Status == CommandStatus.Invalid ? ErrorSeverity.Minor :
                               response.Status == CommandStatus.Failure ? ErrorSeverity.Minor :
                               ErrorSeverity.Critical,

                    Location = ErrorLocation.Discard,
                    MessageKey = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            PlayerController.Local.RPC_DiscardCard(playerId, cardID, hasEffect);
        }

        #endregion

        #region Sacrifice

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardSacrified(int playerId, int cardId)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Sacrifice,
                    MessageKey = "Sacrifice_NotYourTurn"
                });
                return;
            }

            ICommand        checker  = new ADMIN_SacrificeCommand(playerSource, cardId);
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,

                    Severity = response.Status == CommandStatus.Invalid ? ErrorSeverity.Minor :
                               response.Status == CommandStatus.Failure ? ErrorSeverity.Minor :
                               ErrorSeverity.Critical,

                    Location = ErrorLocation.Sacrifice,
                    MessageKey = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            PlayerController.Local.RPC_CardSacrified(playerId, cardId);
        }

        #endregion

        #region Effects

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ActivateEffect(int playerId, int cardID)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            ICommand        checker  = new ADMIN_CheckEffectCommand(playerSource, cardID);
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            PlayerController.Local.RPC_ActivateEffect(playerId, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ReducedInSilenced(int playerId, int cardToBeSilenced)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            PlayerController.Local.RPC_ReducedInSilenced(playerId, cardToBeSilenced);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemoveReducedInSilenced(int playerId, int cardID, int originalSouls)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope      = ErrorScope.Local,
                    Target     = playerSource,
                    Severity   = ErrorSeverity.Major,
                    Location   = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            PlayerController.Local.RPC_RemoveReducedInSilenced(playerId, cardID, originalSouls);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CopiedEffect(int playerId, int cardID, int cardToCopiedID)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope      = ErrorScope.Local,
                    Target     = playerSource,
                    Severity   = ErrorSeverity.Major,
                    Location   = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            ICommand        checker  = new ADMIN_CheckEffectCommand(playerSource, cardID);
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope       = ErrorScope.Local,
                    Target      = playerSource,
                    Severity    = ErrorSeverity.Major,
                    Location    = ErrorLocation.Effect,
                    MessageKey  = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            PlayerController.Local.RPC_CopiedEffect(playerId, cardID, cardToCopiedID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemoveCopiedEffect(int playerId, int cardID)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope      = ErrorScope.Local,
                    Target     = playerSource,
                    Severity   = ErrorSeverity.Major,
                    Location   = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            ICommand        checker  = new ADMIN_CheckEffectCommand(playerSource, cardID);
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope       = ErrorScope.Local,
                    Target      = playerSource,
                    Severity    = ErrorSeverity.Major,
                    Location    = ErrorLocation.Effect,
                    MessageKey  = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            PlayerController.Local.RPC_RemoveCopiedEffect(playerId, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_NetworkEventCardEffect(int playerId, int cardID, string data)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            ICommand        checker  = new ADMIN_CheckEffectCommand(playerSource, cardID);
            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope       = ErrorScope.Local,
                    Target      = playerSource,
                    Severity    = ErrorSeverity.Major,
                    Location    = ErrorLocation.Effect,
                    MessageKey  = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });


                return;
            }

            PlayerController.Local.RPC_NetworkEventCardEffect(playerId, cardID, data);
        }

        #endregion
    }
}
