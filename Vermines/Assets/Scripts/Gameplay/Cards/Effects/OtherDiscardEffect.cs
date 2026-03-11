using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {
    using Fusion;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Other Discard Effect", menuName = "Vermines/Card System/Card/Effects/Other Discard Effect.")]
    public class OtherDiscardEffect : AEffect {

        #region Constants

        private static readonly string descriptionTemplate = "As long as this card is placed in the partisan zone, then each time another {0} card is discarded";
        private static readonly string godDescriptionTemplate = "Each turn the first another {0} card is discarded allows you to";
        private static readonly string linkerTemplate = ", ";

        #endregion

        #region Informations

        [SerializeField]
        private EffectType _Type = EffectType.OnOtherDiscard;

        public override EffectType Type
        {
            get => _Type;
            set
            {
                _Type = value;
            }
        }

        [SerializeField]
        private CardType _TargetType = CardType.Tools;

        public CardType TargetType
        {
            get => _TargetType;
            set
            {
                _TargetType = value;
            }
        }

        [SerializeField]
        private bool _IsGodEffect = false;

        public bool IsGodEffect
        {
            get => _IsGodEffect;
            set
            {
                _IsGodEffect = value;

                UpdateDescription();
            }
        }

        private bool _IsFirstDiscardThisTurn = true;

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

        public Sprite PlayIcon            = null;
        public Sprite PartisanDiscardIcon = null;
        public Sprite ToolsDiscardIcon    = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (IsGodEffect) {
                if (_IsFirstDiscardThisTurn)
                    _IsFirstDiscardThisTurn = false;
                else
                    return;
            }

            base.Play(player);
        }

        public override void Stop(PlayerRef player)
        {
            if (IsGodEffect)
                _IsFirstDiscardThisTurn = true;
            base.Stop(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (!IsGodEffect) {
                elements.Add((null, PlayIcon));
                elements.Add((":" , null));
            }

            if (TargetType == CardType.Tools)
                elements.Add((null, ToolsDiscardIcon));
            else
                elements.Add((null, PartisanDiscardIcon));
            if (SubEffect != null) {
                elements.Add(("=", null));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            if (IsGodEffect)
                Description = string.Format(godDescriptionTemplate, TargetType.ToString().ToLower());
            else
                Description = string.Format(descriptionTemplate, TargetType.ToString().ToLower());

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
            if (PartisanDiscardIcon == null)
                PartisanDiscardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/DiscardPartisan");
            if (ToolsDiscardIcon == null)
                ToolsDiscardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/DiscardTool");
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
