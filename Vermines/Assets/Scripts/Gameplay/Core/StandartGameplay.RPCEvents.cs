using UnityEngine.Localization.Settings;
using UnityEngine;
using Fusion;
using OMGG.Chronicle;
using OMGG.DesignPattern;
using Newtonsoft.Json;
using System;

namespace Vermines.Gameplay.Core {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.Core;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Gameplay.Errors;
    using Vermines.Player;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.Gameplay.Commands;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;

    public partial class StandartGameplay : GameplayMode {

        #region Notifiers

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ReceiveError(GameActionError error)
        {
            if (error.Scope == ErrorScope.Local && Context.Runner.LocalPlayer != error.Target)
                return;
            HandleError(error);
        }

        private void HandleError(GameActionError error)
        {
            string localizedMessage = LocalizationSettings.StringDatabase.GetLocalizedString("Back-end Error", error.MessageKey.ToString(), error.MessageArgs.ToArray());

            Debug.LogWarning($"[Error-{error.Scope}] {localizedMessage} (Loc: {error.Location}, Sev: {error.Severity})");

            // TODO: Link to UI notification system

            switch (error.Location) {
                case ErrorLocation.Discard:
                    ICard card = CardSetDatabase.Instance.GetCardByID(error.MessageArgs.Arg0.ToString());

                    GameEvents.OnCardDiscardedRefused.Invoke(card);

                    break;
                default:
                    break;
            }
        }

        private void SendError(GameActionError error)
        {
            RPC_ReceiveError(error);
        }

        #endregion

        #region Shop

        public override void OnBuyCard(PlayerRef playerSource, ShopType shopType, int cardID)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Shop,
                    MessageKey = "Shop_Buy_NotYourTurn"
                });

                return;
            }

            ShopArgs parameters = new(Shop, shopType, cardID);

            ICommand checker = new ADMIN_CheckBuyCommand(player, PhaseManager.CurrentPhase, parameters);

            CommandResponse checkerResponse = CommandInvoker.ExecuteCommand(checker);

            if (checkerResponse.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,

                    Severity = checkerResponse.Status == CommandStatus.Invalid ? ErrorSeverity.Minor :
                               checkerResponse.Status == CommandStatus.Failure ? ErrorSeverity.Minor :
                               ErrorSeverity.Critical,

                    Location = ErrorLocation.Shop,
                    MessageKey = checkerResponse.Message,
                    MessageArgs = new GameActionErrorArgs(checkerResponse.Args)
                });

                return;
            }

            ICard cardBought = CardSetDatabase.Instance.GetCardByID(cardID);

            ICommand buyCommand = new ADMIN_BuyCommand(player, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            ChronicleEntry entry = new() {
                Id = KeyGen.UUID(),
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.BuyCard),
                TitleKey = $"T_CardPurchase",
                MessageKey = $"D_{shopType}_CardPurchase",
                IconKey = $"{shopType}"
            };

            var payloadObject = new {
                DescriptionArgs = new string[] {
                    $"ST_{shopType}_CardPurchase",
                    player.NetworkedNickname.Value,
                    cardBought.Data.Name
                },
                CardId = cardBought.ID,
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            player.RPC_BuyCard(NetworkChronicleEntry.FromChronicleEntry(entry), shopType, cardID);
        }

        public override void OnReplaceCardInShop(PlayerRef playerSource, ShopType shopType, int cardID)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Shop,
                    MessageKey = "Shop_Replace_NotYourTurn"
                });

                return;
            }

            ShopArgs parameters = new(Context.GameplayMode.Shop, shopType, cardID);

            ICommand checker = new ADMIN_CheckChangeCardCommand(parameters);

            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) { // Can only failed if the shop or the card selected doesn't exist. So if it's a cheat, bug or a desync.
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Critical,
                    Location = ErrorLocation.Shop,
                    MessageKey = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            ICard cardToReplace = CardSetDatabase.Instance.GetCardByID(cardID);

            response = CommandInvoker.ExecuteCommand(new CLIENT_ChangeCardCommand(parameters)); // Simulate the change to get the new card

            ICard newCard = CardSetDatabase.Instance.GetCardByID(response.Args[^1]); // CLIENT_ChangeCardCommand return when success: Shoptype, oldCard id and newCard id. So we take the last args to know the newCard.

            CommandInvoker.UndoCommand(); // Undo the change in the simulation to keep the real state intact until the RPC is sent to all clients.

            ChronicleEntry entry = new() {
                Id = KeyGen.UUID(),
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.ChangeCard),
                TitleKey = $"T_CardReplaced",
                MessageKey = $"D_CardReplaced",
                IconKey = $"{shopType}"
            };

            var payloadObject = new {
                DescriptionArgs = new string[] {
                    $"ST_{shopType}_CardPurchase",
                    player.NetworkedNickname.Value,
                    cardToReplace.Data.Name,
                    newCard.Data.Name
                },
                oldCardId = cardToReplace.ID,
                newCardId = newCard.ID
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            player.RPC_ReplaceCardInShop(NetworkChronicleEntry.FromChronicleEntry(entry), shopType, cardID);
        }

        #endregion

        #region Table (Play, Discard)

        public override void OnCardPlayed(PlayerRef playerSource, int cardID)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Table,
                    MessageKey = "Table_Play_NotYourTurn",
                    MessageArgs = new GameActionErrorArgs(cardID.ToString())
                });

                return;
            }

            ICommand checker = new ADMIN_CheckPlayCommand(player, PhaseManager.CurrentPhase, cardID);

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

            player.RPC_CardPlayed(cardID);
        }

        public override void OnDiscardCard(PlayerRef playerSource, int cardID, bool hasEffect = true)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Discard,
                    MessageKey = "Table_Discard_NotYourTurn",
                    MessageArgs = new GameActionErrorArgs(cardID.ToString())
                });

                return;
            }

            ICommand checker = new ADMIN_CheckDiscardCommand(player, PhaseManager.CurrentPhase, cardID);

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

            player.RPC_DiscardCard(cardID, hasEffect);
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

        public override void OnCardSacrified(PlayerRef playerSource, int cardID)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Sacrifice,
                    MessageKey = "Sacrifice_NotYourTurn"
                });

                return;
            }

            ICommand checker = new ADMIN_SacrificeCommand(player, cardID);

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

            ICommand cardSacrifiedCommand = new CLIENT_CardSacrifiedCommand(player, cardID);

            CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

            ICard cardToSacrifice = CardSetDatabase.Instance.GetCardByID(cardID);

            __ObservedSouls = cardToSacrifice.Data.CurrentSouls;
            __ObservedPlayer = playerSource;

            if (player.Statistics.Family == cardToSacrifice.Data.Family)
                __ObservedSouls += BonusSoulsOnFamily;
            ICommand earnCommand = new EarnCommand(player, __ObservedSouls, DataType.Soul);

            CommandInvoker.ExecuteCommand(earnCommand);

            player.OnSoulsChanged += ObserveSoulChange;

            foreach (AEffect effect in cardToSacrifice.Data.Effects) {
                if ((effect.Type & EffectType.Sacrifice) != 0)
                    effect.Play(playerSource);
                else if ((effect.Type & EffectType.Passive) != 0)
                    effect.Stop(playerSource);
            }

            foreach (ICard playedCard in player.Deck.PlayedCards) {
                if (playedCard.Data.Effects != null) {
                    foreach (AEffect effect in playedCard.Data.Effects) {
                        if ((effect.Type & EffectType.OnOtherSacrifice) != 0)
                            effect.Play(playerSource);
                    }
                }
            }

            player.OnSoulsChanged -= ObserveSoulChange;

            ChronicleEntry entry = new() {
                Id = KeyGen.UUID(),
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.SacrificeCard),
                TitleKey = $"T_CardSacrified",
                MessageKey = $"D_CardSacrified",
                IconKey = $"Sacrified_Other_Partisan_Card"
            };

            var payloadObject = new {
                DescriptionArgs = new string[] {
                    $"ST_CardSacrified",
                    player.NetworkedNickname.Value,
                    cardToSacrifice.Data.Name,
                    __ObservedSouls.ToString()
                },
                CardId = cardToSacrifice.ID,
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            player.RPC_CardSacrified(NetworkChronicleEntry.FromChronicleEntry(entry), cardID);

            __ObservedSouls = 0;
            __ObservedPlayer = PlayerRef.None;
        }

        #endregion

        #region Recycled

        public override void OnCardRecycled(PlayerRef playerSource, int cardID)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Recycled,
                    MessageKey = "Recycle_NotYourTurn"
                });

                return;
            }

            ICommand checker = new ADMIN_CheckRecycleCommand(player, PhaseManager.CurrentPhase, cardID);

            CommandResponse response = CommandInvoker.ExecuteCommand(checker);

            if (response.Status != CommandStatus.Success) {
                SendError(new GameActionError {
                    Scope = ErrorScope.Local,
                    Target = playerSource,

                    Severity = response.Status == CommandStatus.Invalid ? ErrorSeverity.Minor :
                               response.Status == CommandStatus.Failure ? ErrorSeverity.Minor :
                               ErrorSeverity.Critical,

                    Location = ErrorLocation.Recycled,
                    MessageKey = response.Message,
                    MessageArgs = new GameActionErrorArgs(response.Args)
                });

                return;
            }

            ICommand cardSacrifiedCommand = new CLIENT_CardRecycleCommand(player, cardID, Shop.Sections[ShopType.Market]);

            CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

            ICard cardToRecycle = CardSetDatabase.Instance.GetCardByID(cardID);

            player.SetEloquence(player.Statistics.Eloquence + cardToRecycle.Data.RecycleEloquence);

            ChronicleEntry entry = new() {
                Id = KeyGen.UUID(),
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.SacrificeCard),
                TitleKey = $"T_CardRecycled",
                MessageKey = $"D_CardRecycled",
                IconKey = $"Replace"
            };

            var payloadObject = new {
                DescriptionArgs = new string[] {
                    $"ST_CardRecycled",
                    player.NetworkedNickname.Value,
                    cardToRecycle.Data.Name,
                    cardToRecycle.Data.RecycleEloquence.ToString()
                },
                CardId = cardToRecycle.ID,
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            player.RPC_CardRecycled(NetworkChronicleEntry.FromChronicleEntry(entry), cardID);
        }

        #endregion

        #region Effects

        public override void OnActivateEffect(PlayerRef playerSource, int cardID)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            ICommand checker = new ADMIN_CheckEffectCommand(playerSource, cardID);

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

            player.RPC_ActivateEffect(cardID);
        }

        public override void OnReducedInSilenced(PlayerRef playerSource, int cardToBeSilenced)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            player.RPC_ReducedInSilenced(cardToBeSilenced);
        }

        public override void OnRemoveReducedInSilenced(PlayerRef playerSource, int cardID, int originalSouls)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            player.RPC_RemoveReducedInSilenced(cardID, originalSouls);
        }

        public override void OnNetworkEventCardEffect(PlayerRef playerSource, int cardID, string data)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // This is a major error because it should never happen unless someone tries to cheat.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn"
                });

                return;
            }

            ICommand checker = new ADMIN_CheckEffectCommand(playerSource, cardID);

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

            player.RPC_NetworkEventCardEffect(cardID, data);
        }

        public override void OnEffectChosen(PlayerRef playerSource, int cardID, int effectIndex)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerSource);

            if (playerSource != CurrentPlayer) {
                SendError(new GameActionError { // You are not supposed to chose an effect when it's not your turn.
                    Scope = ErrorScope.Local,
                    Target = playerSource,
                    Severity = ErrorSeverity.Major,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_NotYourTurn",
                    MessageArgs = new GameActionErrorArgs(cardID.ToString(), effectIndex.ToString())
                });
            }

            ICommand checker = new ADMIN_CheckChosenEffectCommand(playerSource, cardID, effectIndex);

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

            player.RPC_EffectChosen(cardID, effectIndex);

        }

        #endregion
    }
}
