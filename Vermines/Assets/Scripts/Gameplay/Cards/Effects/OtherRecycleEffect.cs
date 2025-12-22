using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Recycle Effect", menuName = "Vermines/Card System/Card/Effects/Other Recycle Effect.")]
    public class OtherRecycleEffect : AEffect {

        #region Constants

        private static readonly string descriptionTemplate = "Recycle a <b>Tools</b>";
        private static readonly string linkerTemplate = " to ";

        #endregion

        #region Informations

        [SerializeField]
        private EffectType _Type = EffectType.OnOtherRecycle;

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

        public Sprite PlayCardIcon = null;
        public Sprite RecycleIcon = null;

        #endregion

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                (null, PlayCardIcon),
                (":" , null        ),
                (null, RecycleIcon )
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

            if (PlayCardIcon == null)
                PlayCardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Played");
            if (RecycleIcon == null)
                RecycleIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Replace");
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
