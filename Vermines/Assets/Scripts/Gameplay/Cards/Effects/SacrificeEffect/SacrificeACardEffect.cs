using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Sacrifice/Sacrifice cards.")]
    public class SacrificeACard : AEffect {

        #region Constants

        private static readonly string sacrificeTemplate = "Sacrifice a <b>Partisan</b>";
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

        public Sprite SacrificedIcon = null;
        public Sprite SoulIcon       = null;
        public Sprite Then           = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player];

            // Check if their is card to sacrifice.
            if (deck.PlayedCards.Count == 0) {
                base.Play(player);
            } else if (player == PlayerController.Local.PlayerRef) {
                // TODO: Force the player to be in the sacrifice view.
                // TODO: Implement the observer pattern here to trigger the sacrifice event.
            }
        }

        public void OnSacrificed(ICard card)
        {
            // TODO: Remove the event observer
            PlayerController.Local.OnCardSacrified(card.ID);
            PlayerController.Local.NetworkEventCardEffect(card.ID);
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            base.Play(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null  , SacrificedIcon) },
                { ($"+ X", null)           },
                { (null  , SoulIcon)       }
            };

            if (SubEffect != null) {
                elements.Add((null, Then));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = $"{sacrificeTemplate}";

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

            if (SacrificedIcon == null)
                SacrificedIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificed_Other_Partisan_Card");
            if (SoulIcon == null)
                SoulIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Souls");
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
