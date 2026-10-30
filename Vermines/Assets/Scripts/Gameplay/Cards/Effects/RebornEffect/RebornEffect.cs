using Fusion;
using Newtonsoft.Json;
using OMGG.Chronicle;
using OMGG.DesignPattern;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core.Player;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Gameplay.Commands;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Reborn/Reborn a partisan effect.")]
    public class RebornEffect : AEffect {

        #region Constants

        private static readonly string template = "Reborn a sacrificed partisan.";

        #endregion

        #region Properties

        [SerializeField]
        private string _Description;

        public override string Description
        {
            get => _Description;
            set
            {
                _Description = value;
            }
        }

        #endregion

        #region UI Elements

        public Sprite SacrifiedPartisanIcon = null;
        public Sprite PlayPartisanIcon = null;

        #endregion

        public override bool CanBePlayed(PlayerRef player, out string reasonKey)
        {
            PlayerController controller = Context.NetworkGame.GetPlayer(player);
            if (controller.PlayedCards.Count >= controller.Statistics.NumberOfSlotInTable)
            {
                reasonKey = "skipped.table_full";
                return false;
            }
            if (EffectCandidates.ForReborn(controller, CardType.Partisan).Count == 0)
            {
                reasonKey = "skipped.reborn_no_target";
                return false;
            }
            reasonKey = null;
            return true;
        }
        public override void Play(PlayerRef playerRef)
        {
            if (playerRef != Context.Runner.LocalPlayer)
                return;
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            PlayerStatistics stat = player.Statistics;

            if (!CanBePlayed(playerRef, out string reasonKey))
            {
                GameEvents.OnEffectSkipped.Invoke(Card, reasonKey);

                return;
            }
            GameEvents.OnEffectPromptRequested.Invoke(new EffectPrompt(EffectPromptKind.Reborn, CardType.Partisan, Card));
            GameEvents.OnTurnTimerExpired.AddListener(CancelPrompt);
            GameEvents.OnEffectSelectCard.AddListener(Reborn);
        }

        private void Reborn(ICard card)
        {
            GameEvents.OnEffectSelectCard.RemoveListener(Reborn);
            GameEvents.OnEffectPromptClosed.Invoke(EffectPromptKind.Reborn);
            GameEvents.OnTurnTimerExpired.RemoveListener(CancelPrompt);

            if (card.Data.Type != CardType.Partisan)
                return;
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            player.NetworkEventCardEffect(Card == null ? -1 : Card.ID, card.ID.ToString());
        }

        private void CancelPrompt()
        {
            GameEvents.OnEffectSelectCard.RemoveListener(Reborn);
            GameEvents.OnTurnTimerExpired.RemoveListener(CancelPrompt);
        }

        public override void NetworkEventFunction(PlayerRef playerRef, string data)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            ICard    card          = CardSetDatabase.Instance.GetCardByID(data);
            ICommand rebornCommand = new RebornCommand(player, card);

            CommandInvoker.ExecuteCommand(rebornCommand);

            if (playerRef == Context.Runner.LocalPlayer)
                GameEvents.OnCardReborned.Invoke(card);
            ChronicleEntry entry = new() {
                Id = $"reborned-{Card.ID}-{card.ID}",
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.RebornEffect),
                TitleKey = $"T_CardReborned",
                MessageKey = $"D_CardReborned",
                IconKey = $"Partisan_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_CardReborned",
                    player.Nickname,
                    card.Data.Name,
                    Card.Data.Name
                }
            };

            var payloadObject = new {
                DescriptionArgs = entry.DescriptionArgs,
                CardId          = Card.ID,
                rebornedCardId  = card.ID,
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            entry.PayloadJson = payloadJson;

            player.AddChronicle(entry);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { ("1" , null) },
                { (null, SacrifiedPartisanIcon) },
                { ("->", null) },
                { (null, PlayPartisanIcon) }
            };

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = template;
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (SacrifiedPartisanIcon == null)
                SacrifiedPartisanIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificed_This_Card");
            if (PlayPartisanIcon == null)
                PlayPartisanIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Played");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
