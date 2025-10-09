using Fusion;
using OMGG.DesignPattern;
using OMGG.Chronicle;
using Newtonsoft.Json;
using System;

namespace Vermines {

    using Vermines.Gameplay.Commands;
    using Vermines.Gameplay.Errors;
    using Vermines.Player;
    using Vermines.Gameplay.Chronicle;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem;

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

        #region UUID

        /// <summary>
        /// Generate a 16-character unique identifier.
        /// </summary>
        static string GenerateUUID()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 16);
        }

        #endregion

        #region Notifiers

        private void SendError(GameActionError error)
        {
            PlayerController.Local.RPC_ReceiveError(error);
        }

        #endregion

        #region Payload

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_AskPayload(string id)
        {
            if (ChroniclePayloadStorage.TryGet(id, out string payloadJson))
                PlayerController.Local.RPC_ReceivePayload(id, payloadJson);
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

            ICard cardBought = parameters.Shop.Sections[shopType].GetCardAtSlot(slot);

            ICommand buyCommand = new ADMIN_BuyCommand(playerSource, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            PlayerData player = GameDataStorage.Instance.PlayerData[playerSource];

            ChronicleEntry entry = new() {
                Id           = GenerateUUID(),
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType    = new VerminesLogEventType(VerminesLogsType.BuyCard),
                TitleKey     = $"T_CardPurchase",
                MessageKey   = $"D_{shopType}_CardPurchase",
                IconKey      = $"{shopType}"
            };

            var payloadObject = new {
                DescriptionArgs = new string[] {
                    $"ST_{shopType}_CardPurchase",
                    player.Nickname,
                    cardBought.Data.Name
                },
                CardId = cardBought.ID,
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            PlayerController.Local.RPC_BuyCard(playerId, NetworkChronicleEntry.FromChronicleEntry(entry), shopType, slot);
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

            ShopArgs parameters = new(GameDataStorage.Instance.Shop, shopType, slot);

            ICommand        checker  = new ADMIN_CheckChangeCardCommand(parameters);
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
            PlayerData   player = GameDataStorage.Instance.PlayerData[playerSource];
            ICard cardToReplace = GameDataStorage.Instance.Shop.Sections[shopType].GetCardAtSlot(slot);
            
            CommandInvoker.ExecuteCommand(new CLIENT_ChangeCardCommand(parameters)); // Simulate the change to get the new card

            ICard newCard = GameDataStorage.Instance.Shop.Sections[shopType].GetCardAtSlot(slot);

            CommandInvoker.UndoCommand(); // Undo the change in the simulation to keep the real state intact until the RPC is sent to all clients.

            ChronicleEntry entry = new() {
                Id           = GenerateUUID(),
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType    = new VerminesLogEventType(VerminesLogsType.ChangeCard),
                TitleKey     = $"T_CardReplaced",
                MessageKey   = $"D_CardReplaced",
                IconKey      = $"{shopType}"
            };

            var payloadObject = new {
                DescriptionArgs = new string[] {
                    $"ST_{shopType}_CardPurchase",
                    player.Nickname,
                    cardToReplace.Data.Name,
                    newCard.Data.Name
                },
                oldCardId = cardToReplace.ID,
                newCardId = newCard.ID
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            PlayerController.Local.RPC_ReplaceCardInShop(playerId, NetworkChronicleEntry.FromChronicleEntry(entry), shopType, slot);
        }

        #endregion

        #region Table (Play, Discard)

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerId); // The player who initiated the buy request

            if (playerSource != GetCurrentPlayer()) {
                SendError(new GameActionError {
                    Scope      = ErrorScope.Local,
                    Target     = playerSource,
                    Severity   = ErrorSeverity.Minor,
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
                SendError(new GameActionError {
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

        private int __ObservedSouls = 0;
        private PlayerRef __ObservedPlayer = PlayerRef.None;

        private void ObserveSoulChange(PlayerRef player, int oldValue, int newValue)
        {
            if (player == __ObservedPlayer)
                __ObservedSouls += newValue - oldValue;
        }

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

            ICommand         checker = new ADMIN_SacrificeCommand(playerSource, cardId);
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

            ICommand cardSacrifiedCommand = new CLIENT_CardSacrifiedCommand(playerSource, cardId);

            CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

            PlayerData     player = GameDataStorage.Instance.PlayerData[playerSource];
            ICard cardToSacrifice = CardSetDatabase.Instance.GetCardByID(cardId);

            __ObservedSouls  = cardToSacrifice.Data.CurrentSouls;
            __ObservedPlayer = playerSource;

            if (player.Family == cardToSacrifice.Data.Family)
                __ObservedSouls += SettingsData.BonusSoulInFamilySacrifice;
            ICommand earnCommand = new EarnCommand(playerSource, __ObservedSouls, DataType.Soul);

            CommandInvoker.ExecuteCommand(earnCommand);

            GameDataStorage.Instance.OnSoulsChanged += ObserveSoulChange;

            foreach (AEffect effect in cardToSacrifice.Data.Effects) {
                if ((effect.Type & EffectType.Sacrifice) != 0)
                    effect.Play(playerSource);
                else if ((effect.Type & EffectType.Passive) != 0)
                    effect.Stop(playerSource);
            }

            foreach (ICard playedCard in GameDataStorage.Instance.PlayerDeck[playerSource].PlayedCards) {
                if (playedCard.Data.Effects != null) {
                    foreach (AEffect effect in playedCard.Data.Effects) {
                        if ((effect.Type & EffectType.OnOtherSacrifice) != 0)
                            effect.Play(playerSource);
                    }
                }
            }

            GameDataStorage.Instance.OnSoulsChanged -= ObserveSoulChange;

            ChronicleEntry entry = new() {
                Id           = GenerateUUID(),
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType    = new VerminesLogEventType(VerminesLogsType.SacrificeCard),
                TitleKey     = $"T_CardSacrified",
                MessageKey   = $"D_CardSacrified",
                IconKey      = $"Sacrified_Other_Partisan_Card"
            };

            var payloadObject = new {
                DescriptionArgs = new string[] {
                    $"ST_CardSacrified",
                    player.Nickname,
                    cardToSacrifice.Data.Name,
                    __ObservedSouls.ToString()
                },
                CardId = cardToSacrifice.ID,
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            PlayerController.Local.RPC_CardSacrified(playerId, NetworkChronicleEntry.FromChronicleEntry(entry), cardId);

            __ObservedSouls = 0;
            __ObservedPlayer = PlayerRef.None;
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
