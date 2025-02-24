using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vermines.HUD
{
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.HUD.Card;
    using Vermines.ShopSystem.Enumerations;

    public class CardBuyBanner : MonoBehaviour
    {
        #region UI Elements
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI type;
        [SerializeField] private Image typeIcon;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI souls;
        [SerializeField] private TextMeshProUGUI effectDescription;
        [SerializeField] private GameObject CardGameObject;
        #endregion

        #region Debug
        [SerializeField] private ICard _Card;
        [SerializeField] private bool debugMode = false;
        #endregion

        void Start()
        {
            //if (debugMode)
            //{
            //    Setup(cardData, 0);
            //}
        }

        /// <summary>
        /// Setup the card buy banner by loading the card data.
        /// It need to be called before the banner is displayed.
        /// </summary>
        /// <param name="card"></param>
        public void Setup(ICard card)
        {
            if (card == null)
            {
                Debug.LogError("Card is null!");
                return;
            }

            CardBase cardBase = CardGameObject.GetComponent<CardBase>();
            if (cardBase)
            {
                cardBase.Setup(card);
            }

            cardName.text = card.Data.Name;
            cost.text = $"Cost: {card.Data.Eloquence} eloquences";
            souls.text = $"+{card.Data.Souls} souls on sacrifice";
            souls.gameObject.SetActive(card.Data.Type == CardType.Partisan);
            type.text = card.Data.Type == CardType.Partisan ? card.Data.Family.ToString() : card.Data.Type.ToString();
            
            LoadTypeIcon(card.Data.Type);
            LoadEffects(card.Data);

            _Card = card;
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

            (ShopType, int)? cardInfo = CardSpawner.Instance.GetCard(_Card.ID);

            if (cardInfo == null)
            {
                Debug.LogError("Card not found!");
                return;
            }

            GameEvents.OnCardBought.Invoke(cardInfo.Value.Item1, cardInfo.Value.Item2);

            // Destroy the card bought
            CardSpawner.Instance.DestroyCard(_Card.ID);

            ShopManager.instance.GetShop().CloseCardBuyOverlay();
        }

        private void OnValidate()
        {
            if (debugMode)
            {
                Debug.LogWarning("CardBuyBanner: Debug mode is enabled. Make sure to disable it before building the game.");
            }

            if (CardGameObject != null && debugMode)
            {
                Setup(_Card);
            }
        }
    }
}
