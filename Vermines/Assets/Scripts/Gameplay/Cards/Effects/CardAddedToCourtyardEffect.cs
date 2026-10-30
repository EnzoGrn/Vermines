using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Card Added to Courtyard.")]
    public class CardAddedToCourtyardEffect : AEffect {

        #region Constants

        private static readonly string descriptionTemplate = "As long as this card is placed in the partisan area, then each time a new card is added to the courtyard";
        private static readonly string linkerTemplate = ", ";

        #endregion

        #region Informations

        [SerializeField]
        private EffectType _Type = EffectType.OnCardAddedToCourtyard;

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

        public Sprite PlayIcon = null;
        public Sprite CardAddedIcon = null;
        public Sprite CourtyardIcon = null;

        #endregion

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                (null, PlayIcon),
                (":" , null    ),
                (null, CardAddedIcon),
                (null, CourtyardIcon)
            };

            if (SubEffect != null) {
                elements.Add(("=", null));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = descriptionTemplate;

            if (SubEffect != null) {
                string subDescription = SubEffect.Description;

                if (subDescription.Length > 0)
                    subDescription = char.ToLower(subDescription[0]) + subDescription[1..];
                Description += $"{linkerTemplate}{subDescription}";
            }

            Description += ".";
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (PlayIcon == null)
                PlayIcon = Resources.Load<Sprite>("Sprites/UI/Effects/CardPlayedPassive");
            if (CardAddedIcon == null)
                CardAddedIcon = Resources.Load<Sprite>("Sprites/UI/Effects/AddCard");
            if (CourtyardIcon == null)
                CourtyardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Courtyard");
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
