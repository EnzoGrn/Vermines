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
    using Vermines.UI;
    using Vermines.UI.Card;
    using Vermines.UI.Screen;

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

        public override void Play(PlayerRef playerRef)
        {
            if (playerRef != Context.Runner.LocalPlayer)
                return;
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            PlayerStatistics stat = player.Statistics;
            PlayerDeck       deck = player.Deck;

            if (deck.PlayedCards.Count >= stat.NumberOfSlotInTable || deck.Graveyard.Count == 0)
                return;
            if (UIContextManager.Instance) {
                CardSelectedEffectContext args = new(CardType.Partisan, Card);

                CardRebornContext ctx = new(args);

                UIContextManager.Instance.PushContext(ctx);
            }

            GameEvents.OnEffectSelectCard.AddListener(Reborn);
        }

        private void Reborn(ICard card)
        {
            GameEvents.OnEffectSelectCard.RemoveListener(Reborn);

            if (UIContextManager.Instance)
                UIContextManager.Instance.PopContextOfType<CardRebornContext>();
            if (card.Data.Type != CardType.Partisan)
                return;
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            player.NetworkEventCardEffect(Card.ID, card.ID.ToString());
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
