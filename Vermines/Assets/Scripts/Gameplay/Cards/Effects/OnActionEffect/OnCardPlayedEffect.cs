using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Action/On Card Played.")]
    public class OnCardPlayedEffect : AEffect {

        #region Constants

        private static readonly string Template = "Each time you play a <b>Partisan</b> card";

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

        public Sprite PartisanIcon = null;

        #endregion

        public override void OnAction(string ActionMessage, PlayerRef player, ICard card)
        {
            if (player == Card.Owner) {
                if (card.Data.Type == CardType.Partisan && ActionMessage == "Play")
                    SubEffect.Play(player);
            }

            base.OnAction(ActionMessage, player, card);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (SubEffect != null) {
                elements.AddRange(SubEffect.Draw());
                elements.Add(("/", null));
            }

            elements.Add((null, PartisanIcon));

            return elements;
        }

        protected override void UpdateDescription()
        {
            string subDescription = string.Empty;

            if (SubEffect != null)
                subDescription = SubEffect.Description;
            string templateDescription = Template;

            if (subDescription.Length > 0)
                templateDescription = char.ToLower(templateDescription[0]) + templateDescription[1..];
            Description = $"{subDescription} {templateDescription}";
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (PartisanIcon == null)
                PartisanIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Played");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
