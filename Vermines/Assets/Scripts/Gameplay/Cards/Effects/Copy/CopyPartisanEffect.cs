using System.Collections.Generic;
using Newtonsoft.Json;
using OMGG.Chronicle;
using UnityEngine;
using System;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Player;

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
            if (player != Context.Runner.LocalPlayer)
                return;
            if (UIContextManager.Instance) {
                CardSelectedEffectContext cardCopyEffectContext = new(CardType.Partisan, Card);
                CopyContext               copyContext           = new(cardCopyEffectContext);

                UIContextManager.Instance.PushContext(copyContext);
            }

            GameEvents.OnEffectSelectCard.AddListener(CopiedEffect);
        }

        public override void Stop(PlayerRef player)
        {
            if (_CardCopied != null) {
                foreach (var effect in _CardCopied.Data.Effects) {
                    if ((effect.Type & EffectType.Passive) != 0)
                        effect.Stop(player);
                }

                Card.Data.RemoveEffectCopied();

                _CardCopied = null;
            }
        }

        private void CopiedEffect(ICard card)
        {
            GameEvents.OnEffectSelectCard.RemoveListener(CopiedEffect);
            UIContextManager.Instance.PopContext();

            if (card.Data.Type != CardType.Partisan)
                return;
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            _CardCopied = card;

            RoundEventDispatcher.RegisterEvent(player.Object.InputAuthority, Stop);

            player.NetworkEventCardEffect(Card.ID, card.ID.ToString());
        }

        public override void NetworkEventFunction(PlayerRef playerRef, string data)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            ICard card = CardSetDatabase.Instance.GetCardByID(data);

            Card.Data.CopyEffect(card.Data.Effects);

            foreach (var effect in card.Data.Effects) {
                if ((effect.Type & EffectType.Sacrifice) != 0 || (effect.Type & EffectType.OnOtherSacrifice) != 0)
                    continue;
                effect.Play(playerRef);
            }

            ChronicleEntry entry = new() {
                Id              = $"copied-{Card.ID}-{card.ID}",
                TimestampUtc    = DateTime.UtcNow.Ticks,
                EventType       = new VerminesLogEventType(VerminesLogsType.CopiedEffect),
                TitleKey        = $"T_CardReplaced",
                MessageKey      = $"D_CardReplaced",
                IconKey         = $"Partisan_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_CardReplaced",
                    player.Nickname,
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

            player.AddChronicle(entry);
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
