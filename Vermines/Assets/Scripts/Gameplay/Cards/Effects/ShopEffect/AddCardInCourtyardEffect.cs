using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Shop/Add card in Courtyard.")]
    public class AddCardInCourtyardEffect : AEffect {

        #region Constants

        private static readonly string replaceTemplate = "You can add a card in the courtyard";
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

        #endregion

        #region UI Elements

        public Sprite AddCardIcon   = null;
        public Sprite CourtyardIcon = null;
        public Sprite ThenIcon      = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player == Context.Runner.LocalPlayer) {
                // TODO: Open a context to chose if we want to add a level1 card or a level2.
                // In this context add a logic for when one of the deck is empty or both.
                // 1 Deck empty can't be choosen
                // Both deck empty, close context

                // -- Example
                // UIContextManager.Instance.PushContext(new AddCardInCourtyardContext(AddCard));

                AddCard(1);
            }
        }

        private void AddCard(int level)
        {
            if (UIContextManager.Instance)
                UIContextManager.Instance.PopContextOfType<ReplaceEffectContext>();
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            player.OnRequestNewCardInCourtyard(level);

            base.Play(Context.Runner.LocalPlayer);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null, AddCardIcon  ) },
                { (null, CourtyardIcon) }
            };

            if (SubEffect != null) {
                elements.Add((null, ThenIcon));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            string description = replaceTemplate;

            if (SubEffect != null) {
                string subDescription = SubEffect.Description;

                if (subDescription.Length > 0)
                    subDescription = char.ToLower(subDescription[0]) + subDescription[1..];
                description += $"{linkerTemplate}{subDescription}";
            }

            Description = description;
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (AddCardIcon == null)
                AddCardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/AddCard");
            if (CourtyardIcon == null)
                CourtyardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Courtyard");
            if (ThenIcon == null)
                ThenIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Then");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
