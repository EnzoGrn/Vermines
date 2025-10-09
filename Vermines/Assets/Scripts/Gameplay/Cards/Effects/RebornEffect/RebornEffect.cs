using System.Collections.Generic;
using OMGG.DesignPattern;
using OMGG.Chronicle;
using Newtonsoft.Json;
using UnityEngine;
using System;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Gameplay.Commands;
    using Vermines.Player;
    using Vermines.UI.Card;

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

        public override void Play(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;
            PlayerData playerData = GameDataStorage.Instance.PlayerData[player];
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[player];

            if (playerDeck.PlayedCards.Count >= playerData.NumberOfSlotInTable || playerDeck.Graveyard.Count == 0)
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
                UIContextManager.Instance.PopContext();
            if (card.Data.Type != CardType.Partisan)
                return;
            PlayerController.Local.NetworkEventCardEffect(Card.ID, card.ID.ToString());
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            PlayerData pData = GameDataStorage.Instance.PlayerData[player];

            ICard    card          = CardSetDatabase.Instance.GetCardByID(data);
            ICommand rebornCommand = new RebornCommand(player, card);

            CommandInvoker.ExecuteCommand(rebornCommand);

            if (player == PlayerController.Local.PlayerRef) {
                // TODO: Update the UI of the table by adding the reborned card to it and removing it from the graveyard.
            }

            ChronicleEntry entry = new() {
                Id = $"reborned-{Card.ID}-{card.ID}",
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.RebornEffect),
                TitleKey = $"T_CardReborned",
                MessageKey = $"D_CardReborned",
                IconKey = $"Partisan_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_CardReborned",
                    pData.Nickname,
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

            PlayerController.Local.AddChronicle(entry);
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
