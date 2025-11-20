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
            if (player != Context.Runner.LocalPlayer)
                return;
            if (UIContextManager.Instance) {
                CardSelectedEffectContext cardCopyEffectContext = new(CardType.Tools, Card);

                CopyContext copyContext = new CopyContext(cardCopyEffectContext);
                UIContextManager.Instance.PushContext(copyContext);
            }

            GameEvents.OnEffectSelectCard.AddListener(OnCardCopied);
        }

        private void OnCardCopied(ICard card)
        {
            GameEvents.OnEffectSelectCard.RemoveListener(OnCardCopied);
            UIContextManager.Instance.PopContext();

            if (card.Data.Type != CardType.Tools)
                return;
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            player.NetworkEventCardEffect(Card.ID, card.ID.ToString());
        }

        public override void NetworkEventFunction(PlayerRef playerRef, string data)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            ICard card = CardSetDatabase.Instance.GetCardByID(data);

            foreach (var effect in card.Data.Effects)
                effect.Play(playerRef);
            ChronicleEntry entry = new() {
                Id           = $"copied-{Card.ID}-{card.ID}",
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType    = new VerminesLogEventType(VerminesLogsType.CopiedEffect),
                TitleKey     = $"T_CardReplaced",
                MessageKey   = $"D_CardReplaced",
                IconKey      = $"Tool_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_CardReplaced",
                    player.Nickname,
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

            player.AddChronicle(entry);
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
