using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Passive Effect", menuName = "Vermines/Card System/Card/Effects/Passive Effect.")]
    public class PassiveEffect : AEffect {

        #region Informations

        [SerializeField]
        private EffectType _Type = EffectType.Passive;

        public override EffectType Type
        {
            get => _Type;
            set
            {
                _Type = value;
            }
        }

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

        [SerializeField]
        private AEffect _SubEffect = null;

        public override AEffect SubEffect
        {
            get => _SubEffect;
            set
            {
                _SubEffect = value;

                UpdateDescription();
            }
        }

        #endregion

        protected override void UpdateDescription()
        {
            base.UpdateDescription();

            if (SubEffect != null)
                Description += ".";
        }

        public override List<(string, Sprite)> Draw()
        {
            if (SubEffect != null)
                return SubEffect.Draw();
            return base.Draw();
        }

        private void OnEnable()
        {
            UpdateDescription();
        }

        #region Editor

        public override void OnValidate()
        {
            if (SubEffect != null)
                SubEffect.OnValidate();
            UpdateDescription();
        }

        #endregion
    }
}
