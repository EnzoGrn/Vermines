using System.Collections.Generic;
using Newtonsoft.Json;
using OMGG.Chronicle;
using UnityEngine;
using System;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {
    using UnityEditor.SceneManagement;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Player;
    using Vermines.UI.Screen;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Copy/Copy a partisan effect.")]
    public class CopyPartisanEffect : AEffect {

        #region Constants

        private static readonly string copyTemplate = "Play the effect of a different partisan.";

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

        private ICard _CardCopied = null;

        #endregion

        #region UI Elements

        public Sprite PartisanCardIcon = null;
        public Sprite PartisanEffectIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;
            if (UIContextManager.Instance) {
                CardCopyEffectContext cardCopyEffectContext = new(CardType.Partisan, Card);
                CopyContext           copyContext           = new(cardCopyEffectContext);

                UIContextManager.Instance.PushContext(copyContext);
            }

            GameEvents.OnCardCopiedEffect.AddListener(CopiedEffect);
        }

        public override void Stop(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;
            Card.Data.RemoveEffectCopied();
        }

        private void CopiedEffect(ICard card)
        {
            GameEvents.OnCardCopiedEffect.RemoveListener(CopiedEffect);
            UIContextManager.Instance.PopContext();

            if (card.Data.Type != CardType.Partisan)
                return;
            _CardCopied = card;

            RoundEventDispatcher.RegisterEvent(PlayerController.Local.PlayerRef, Stop);
            PlayerController.Local.NetworkEventCardEffect(Card.ID, card.ID.ToString());
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(data);

            Card.Data.CopyEffect(card.Data.Effects);

            foreach (var effect in card.Data.Effects) {
                if (effect.Type == EffectType.Sacrifice || effect.Type == EffectType.OnOtherSacrifice)
                    continue;
                effect.Play(player);
            }
            PlayerData pData = GameDataStorage.Instance.PlayerData[player];

            ChronicleEntry entry = new() {
                Id              = $"copied-{Card.ID}-{card.ID}",
                TimestampUtc    = DateTime.UtcNow.Ticks,
                EventType       = new VerminesLogEventType(VerminesLogsType.CopiedEffect),
                TitleKey        = $"T_CardReplaced",
                MessageKey      = $"D_CardReplaced",
                IconKey         = $"Partisan_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_CardReplaced",
                    pData.Nickname,
                    Card.Data.Name,
                    card.Data.Name
                }
            };

            var payloadObject = new {
                DescriptionArgs = entry.DescriptionArgs,
                CardId = Card.ID,
                copiedCardId = card.ID,
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
                { (null, PartisanEffectIcon) },
                { (" != ", null) },
                { (null, PartisanCardIcon) }
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

            if (PartisanEffectIcon == null)
                PartisanEffectIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Effect");
            if (PartisanCardIcon == null)
                PartisanCardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Played");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
