using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Silenced/Reduced to silence a card.")]
    public class SilencedEffect : AEffect {

        #region Constants

        private static readonly string silencedTemplate = "Cancel the next activation of another partisan's effects, that partisan returns to <b><color=red>0A</color></b> until the next turn.";

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

        private ICard _CardToReduced = null;
        private int _SoulsOfTheCardReducedBeforeReduction = 0;

        #endregion

        #region UI Elements

        public Sprite PartisanCardIcon = null;
        public Sprite PartisanEffectIcon = null;
        public Sprite SoulsBannerIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;
            // TODO: Subscribe to the function for reduced in silence the partisan's effect
        }

        public override void Stop(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;
            PlayerController.Local.RemoveReducedInSilenced(_CardToReduced, _SoulsOfTheCardReducedBeforeReduction);
        }

        private void ReducedInSilenced(ICard card)
        {
            // TODO: Unsubscribe the function for reduced in silence the partisan's effect

            _CardToReduced = card;
            _SoulsOfTheCardReducedBeforeReduction = _CardToReduced.Data.CurrentSouls;

            RoundEventDispatcher.RegisterEvent(PlayerController.Local.PlayerRef, Stop);

            PlayerController.Local.OnReducedInSilenced(_CardToReduced);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null, PartisanCardIcon) },
                { (" = ", null) },
                { (null, PartisanEffectIcon) },
                { (" & ", null) },
                { ("0"  , null) },
                { (null , SoulsBannerIcon) }
            };
            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = silencedTemplate;
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (SoulsBannerIcon == null)
                SoulsBannerIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Soul_banner");
            if (PartisanEffectIcon == null)
                PartisanEffectIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Effect");
            if (PartisanCardIcon == null)
                PartisanCardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
