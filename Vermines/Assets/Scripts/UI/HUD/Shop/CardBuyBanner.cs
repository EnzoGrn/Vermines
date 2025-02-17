using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vermines.HUD
{
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Enumerations;
    using Vermines.HUD.Card;

    public class CardBuyBanner : MonoBehaviour
    {
        #region UI Elements
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI type;
        [SerializeField] private Image typeIcon;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI souls;
        [SerializeField] private TextMeshProUGUI effectDescription;
        [SerializeField] private GameObject card;
        #endregion

        #region Debug
        [SerializeField] private CardData cardData;
        [SerializeField] private bool debugMode = false;
        #endregion

        void Start()
        {
            if (debugMode)
            {
                Setup(cardData);
            }
        }

        /// <summary>
        /// Setup the card buy banner by loading the card data.
        /// It need to be called before the banner is displayed.
        /// </summary>
        /// <param name="cardData"></param>
        public void Setup(CardData cardData)
        {
            if (cardData == null)
            {
                Debug.LogError("Card is null!");
                return;
            }

            CardBase cardBase = card.GetComponent<CardBase>();
            if (cardBase)
            {
                cardBase.Setup(cardData);
            }
            cardName.text = cardData.Name;
            cost.text = $"Coût: {cardData.Eloquence} éloquences";
            souls.text = $"+{cardData.Souls} âmes si sacrifié";
            souls.gameObject.SetActive(cardData.Type == CardType.Partisan);
            type.text = cardData.Type == CardType.Partisan ? cardData.Family.ToString() : cardData.Type.ToString();
            LoadTypeIcon(cardData.Type);
            LoadEffects(cardData);
        }

        private void LoadEffects(CardData card)
        {
            effectDescription.text = string.Empty;
            for (int i = 0; i < card.Effects.Count; i++)
            {
                Debug.Log(card.Effects[i].Description);
                effectDescription.text += card.Effects[i].Description;
                if (i < card.Effects.Count - 1)
                {
                    effectDescription.text += "\n";
                }
            }
        }

        private void LoadTypeIcon(CardType type)
        {
            // TODO: load the icon of the card type dynamically, depends on the type of the card
            // If the card type is not found, hide the icon
        }

        /// <summary>
        /// Buy the card.
        /// </summary>
        public void Buy()
        {
            Debug.Log("Card bought!");
            // TODO: add the logic to buy the card in network
            ShopManager.instance.GetShop().CloseCardBuyOverlay();
        }

        private void OnValidate()
        {
            if (debugMode)
            {
                Debug.LogWarning("CardBuyBanner: Debug mode is enabled. Make sure to disable it before building the game.");
            }

            if (card != null && debugMode)
            {
                Setup(cardData);
            }
        }
    }
}
