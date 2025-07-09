using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;
    using Vermines.UI.Screen;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Copy/Copy a tools effect.")]
    public class CopyToolEffect : AEffect {

        #region Constants

        private static readonly string copyTemplate = "Plays the effect of a tool in the market.";

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

        public Sprite ToolEffectIcon = null;
        public Sprite MarketIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;

            if (UIContextManager.Instance)
            {
                CardCopyEffectContext cardCopyEffectContext = new CardCopyEffectContext(CardType.Tools, Card);
                CopyContext copyContext = new CopyContext(cardCopyEffectContext);
                UIContextManager.Instance.PushContext(copyContext);
            }
            GameEvents.OnCardCopiedEffect.AddListener(OnCardCopied);
        }

        private void OnCardCopied(ICard card)
        {
            GameEvents.OnCardCopiedEffect.RemoveListener(OnCardCopied);
            Debug.Log($"[CopyPartisanEffect] {card.Data.Name} effect copied by {Card.Data.Name}.");
            UIContextManager.Instance.PopContext();
            if (card.Data.Type != CardType.Tools)
                return;
            PlayerController.Local.NetworkEventCardEffect(Card.ID, card.ID.ToString());
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            // Here the data is equal to the id of the card copied
            ICard card = CardSetDatabase.Instance.GetCardByID(data);

            foreach (var effect in card.Data.Effects)
                effect.Play(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null, ToolEffectIcon) },
                { (null, MarketIcon) }
            };

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = copyTemplate;
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (ToolEffectIcon == null)
                ToolEffectIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Tool_Card_Effect");
            if (MarketIcon == null)
                MarketIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Market");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
