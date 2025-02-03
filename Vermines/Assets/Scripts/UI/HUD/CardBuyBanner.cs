using UnityEngine;
using TMPro;

namespace Vermines.HUD
{
    using Vermines.CardSystem.Data;

    public class CardBuyBanner : MonoBehaviour
    {
        // -- Texts
        [SerializeField]
        private TextMeshProUGUI title;
        [SerializeField]
        private TextMeshProUGUI cost;
        [SerializeField]
        private TextMeshProUGUI souls;
        [SerializeField]
        private TextMeshProUGUI type;
        [SerializeField]
        private TextMeshProUGUI effectDescription;

        // -- Images
        [SerializeField]
        private SpriteRenderer typeIcon;

        public void Setup(CardData card)
        {
            title.text = card.Name;
            cost.text = card.Eloquence.ToString();
            souls.text = card.Souls.ToString();
            type.text = card.Type.ToString();
            effectDescription.text = card.Description;
        }

        public void Buy()
        {
            Debug.Log("Card bought!");
            HUDManager.Instance.CloseCardBuyOverlay();
        }
    }
}
