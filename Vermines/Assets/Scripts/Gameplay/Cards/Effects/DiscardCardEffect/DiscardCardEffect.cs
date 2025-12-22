using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Discard/Discard cards.")]
    public class DiscardCardEffect : AEffect {

        #region Constants

        private static readonly string discardTemplate = "Discard 1 card";
        private static readonly string linkerTemplate = " then ";

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

        public Sprite DiscardIcon = null;
        public Sprite Then = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player == Context.Runner.LocalPlayer) {
                var context = new ForceDiscardContext(OnDiscarded);
                UIContextManager.Instance.PushContext(context);
            }
        }

        public void OnDiscarded(ICard card)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            player.OnDiscard(card.ID);

            base.Play(player.Object.InputAuthority);

            UIContextManager.Instance.PopContext();
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null       , DiscardIcon) },
                { ($"1", null) }
            };

            if (SubEffect != null) {
                elements.Add((null, Then));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = $"{discardTemplate}";

            if (SubEffect != null) {
                string subDescription = SubEffect.Description;

                if (subDescription.Length > 0)
                    subDescription = char.ToLower(subDescription[0]) + subDescription[1..];
                Description += $"{linkerTemplate}{subDescription}";
            }
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (DiscardIcon == null)
                DiscardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Discard_A_Card");
            if (Then == null)
                Then = Resources.Load<Sprite>("Sprites/UI/Effects/Then");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
