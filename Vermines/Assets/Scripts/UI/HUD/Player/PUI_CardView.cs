using UnityEngine;
using UnityEngine.UI;
using Vermines.CardSystem.Elements;

namespace Vermines.UI {

    // Player UI Card View
    public class PUI_CardView : MonoBehaviour {

        #region Attributes

        public CardSpriteDatabase CSDB;

        public ICard Card { get; private set; }

        [SerializeField]
        private Sprite _DottedCard;

        [SerializeField]
        private Image _Background;

        #endregion

        #region Methods

        public void Bind(ICard card)
        {
            Card = card;

            _Background.sprite = CSDB.Get(card.Data.Type);
        }

        public void SetGhost(bool value)
        {
            if (value) {
                _Background.sprite = _DottedCard;
            } else {
                _Background.sprite = CSDB.Get(Card.Data.Type);
            }
        }

        #endregion
    }
}
