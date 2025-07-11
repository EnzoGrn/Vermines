﻿using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
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
            PlayerController.Local.RemoveCopiedEffect(Card);
        }

        private void CopiedEffect(ICard card)
        {
            GameEvents.OnCardCopiedEffect.RemoveListener(CopiedEffect);
            UIContextManager.Instance.PopContext();

            _CardCopied = card;

            RoundEventDispatcher.RegisterEvent(PlayerController.Local.PlayerRef, Stop);

            PlayerController.Local.CopiedEffect(Card, _CardCopied);
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
