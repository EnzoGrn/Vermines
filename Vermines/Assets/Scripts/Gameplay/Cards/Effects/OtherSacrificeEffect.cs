using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Played Effect", menuName = "Vermines/Card System/Card/Effects/Other Sacrifice Effect.")]
    public class OtherSacrificeEffect : AEffect {

        #region Constants

        private static readonly string descriptionTemplate = "Sacrifice an other Partisan";
        private static readonly string linkerTemplate = " to ";

        #endregion

        #region Informations

        [SerializeField]
        private EffectType _Type = EffectType.OnOtherSacrifice;

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

        #region UI Elements

        public Sprite OtherSacrificeThisCardIcon = null;

        #endregion

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null, OtherSacrificeThisCardIcon) }
            };

            if (SubEffect != null)
            {
                elements.Add((" : ", null));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            if (SubEffect != null)
            {
                string subDescription = SubEffect.Description;

                if (subDescription.Length > 0)
                    subDescription = char.ToLower(subDescription[0]) + subDescription[1..];
                if (Card != null && Card.Data != null)
                {
                    Description = $"{string.Format(descriptionTemplate, Card.Data.Name)}{linkerTemplate}{subDescription}";
                }
                else
                {
                    Description = $"{string.Format(descriptionTemplate, "{card_name}")}{linkerTemplate}{subDescription}";
                }
            }

            Description += ".";
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (OtherSacrificeThisCardIcon == null)
                OtherSacrificeThisCardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificed_Other_Partisan_Card");
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
