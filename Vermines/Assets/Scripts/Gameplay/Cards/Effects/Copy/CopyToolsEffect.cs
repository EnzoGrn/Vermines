using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {
    using Newtonsoft.Json;
    using OMGG.Chronicle;
    using System;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Player;
    using Vermines.UI.Screen;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Copy/Copy a tools effect.")]
    public class CopyToolEffect : AEffect {

        #region Constants

        private static readonly string copyTemplate = "Plays the effect of a tool in the market.";

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

        public Sprite ToolEffectIcon = null;
        public Sprite MarketIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;

            if (UIContextManager.Instance)
            {
                CardCopyEffectContext cardCopyEffectContext = new CardCopyEffectContext(CardType.Tools, Card);
                CopyContext copyContext = new CopyContext(cardCopyEffectContext);
                UIContextManager.Instance.PushContext(copyContext);
            }
            GameEvents.OnCardCopiedEffect.AddListener(OnCardCopied);
        }

        private void OnCardCopied(ICard card)
        {
            GameEvents.OnCardCopiedEffect.RemoveListener(OnCardCopied);
            UIContextManager.Instance.PopContext();

            if (card.Data.Type != CardType.Tools)
                return;
            PlayerController.Local.NetworkEventCardEffect(Card.ID, card.ID.ToString());
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(data);

            foreach (var effect in card.Data.Effects)
                effect.Play(player);
            PlayerData pData = GameDataStorage.Instance.PlayerData[player];

            ChronicleEntry entry = new() {
                Id           = $"copied-{Card.ID}-{card.ID}",
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType    = new VerminesLogEventType(VerminesLogsType.CopiedEffect),
                TitleKey     = $"T_CardReplaced",
                MessageKey   = $"D_CardReplaced",
                IconKey      = $"Tool_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_CardReplaced",
                    pData.Nickname,
                    Card.Data.Name,
                    card.Data.Name
                }
            };

            var payloadObject = new {
                DescriptionArgs = entry.DescriptionArgs,
                CardId          = Card.ID,
                copiedCardId    = card.ID,
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
                { (null, ToolEffectIcon) },
                { (null, MarketIcon) }
            };

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = copyTemplate;
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (ToolEffectIcon == null)
                ToolEffectIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Tool_Card_Effect");
            if (MarketIcon == null)
                MarketIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Market");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
