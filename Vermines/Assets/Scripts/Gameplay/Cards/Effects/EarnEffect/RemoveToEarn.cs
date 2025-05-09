using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Sacrifice/Remove a card effect.")]
    public class RemoveToEarn : AEffect {

        #region Constants

        private static readonly string sacrificeTemplate = "Sacrifice 1 <b>{0}</b> from your discard deck or hand";
        private static readonly string linkerTemplate = " to ";

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
        private CardType _CardType;

        public CardType CardType
        {
            get => _CardType;
            set
            {
                _CardType = value;

                UpdateDescription();
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

        public Sprite ToolsIcon = null;
        public Sprite PartisanIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;
            // TODO: Subscribe to the function for removed a card.
        }

        private void CardToRemove(ICard card)
        {
            if (card.Data.Type != _CardType) {
                Debug.LogWarning($"You can't remove from the game the card {card.Data.Name} because it's not a {_CardType} card.");

                return;
            }

            // TODO: Unsubscribe the function for removed a card.

            PlayerController.Local.OnCardSacrified(card.ID);
            PlayerController.Local.NetworkEventCardEffect(Card.ID);
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            base.Play(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (_CardType == CardType.Partisan)
                elements.Add((null, PartisanIcon));
            else if (_CardType == CardType.Tools)
                elements.Add((null, ToolsIcon));

            if (SubEffect != null) {
                elements.Add((": ", null));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = string.Format(sacrificeTemplate, CardType.ToString());

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

            if (ToolsIcon == null)
                ToolsIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificed_Tool_Card");
            if (PartisanIcon == null)
                PartisanIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificed_Other_Partisan_Card");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
