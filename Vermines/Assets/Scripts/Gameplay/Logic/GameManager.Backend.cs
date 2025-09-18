using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines {

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
    ///           The severity of the error (Warning, Error, Critical, etc.)
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
    /// Errors can be returned either locally to the player via:
    /// <c>PlayerController.Local.RPC_LocalError(...)</c>,
    /// <c>PlayerController.Local.RPC_GlobalError(...)</c>
    /// </para>
    /// </remarks>
    public partial class GameManager : NetworkBehaviour {

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_BuyCard(ShopType shopType, int slot, int playerId)
        {
            BuyParameters parameters = new()
            {
                Decks = GameDataStorage.Instance.PlayerDeck,
                Player = PlayerRef.FromEncoded(playerId),
                Shop = GameDataStorage.Instance.Shop,
                ShopType = shopType,
                Slot = slot
            };

            ICommand buyCommand = new CheckBuyCommand(parameters);

            CommandResponse response = CommandInvoker.ExecuteCommand(buyCommand);

            if (response.Status == CommandStatus.Success)
                Player.PlayerController.Local.RPC_BuyCard(playerId, shopType, slot);
            else
                Debug.LogWarning($"[SERVER]: {response.Message}");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            Player.PlayerController.Local.RPC_CardPlayed(playerId, cardId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_DiscardCard(int playerId, int cardID)
        {
            Player.PlayerController.Local.RPC_DiscardCard(playerId, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_DiscardCardNoEffect(int playerId, int cardID)
        {
            Player.PlayerController.Local.RPC_DiscardCardNoEffect(playerId, cardID);
        }

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
        public void RPC_ReplaceCardInShop(int playerId, ShopType shopType, int slot)
        {
            Player.PlayerController.Local.RPC_ReplaceCardInShop(playerId, shopType, slot);
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
