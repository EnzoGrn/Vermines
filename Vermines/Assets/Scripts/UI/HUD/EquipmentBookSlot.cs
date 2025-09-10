using UnityEngine;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;

namespace Vermines.UI
{
    using Text = TMPro.TMP_Text;
    using Image = UnityEngine.UI.Image;

    public class EquipmentBookSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text nameText;

        private ICardClickHandler _clickHandler = new HandClickHandler();

        private ICard _card = null;

        public void Setup(ICard card)
        {
            _card = card;
            icon.sprite = card.Data.Sprite;
            nameText.text = card.Data.Name;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_card == null) return;
            _clickHandler?.OnCardClicked(_card);
        }
    }
}
