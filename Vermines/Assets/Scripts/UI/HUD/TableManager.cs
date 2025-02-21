using UnityEngine;

namespace Vermines.HUD
{
    using Vermines.HUD.Card;
    using Vermines.ShopSystem.Enumerations;

    public class TableManager : MonoBehaviour
    {
        public static TableManager instance;

        [SerializeField] private GameObject sacrificeOverlay;
        [SerializeField] private CardBase selectedCard;
        [SerializeField] private GameObject discardObject;
        [SerializeField] private GameObject discardBanner;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            sacrificeOverlay.SetActive(false);
            selectedCard = null;
        }

        public void OpenSacrificeOverlay(CardBase cardBase)
        {
            selectedCard = cardBase;
            sacrificeOverlay.SetActive(true);
        }

        public void CloseSacrificeOverlay()
        {
            sacrificeOverlay.SetActive(false);
            selectedCard = null;
        }

        public void SacrificeCard()
        {
            if (selectedCard != null)
            {
                Debug.Log("Sacrificing card: " + selectedCard.Card.Data.Name);
                // TODO: Implement card sacrifice logic

                GameEvents.OnCardSacrified.Invoke(selectedCard.Card.ID);

                Destroy(selectedCard.gameObject);
                CloseSacrificeOverlay();
            }
            else
            {
                Debug.LogError("No card selected to sacrifice.");
            }
        }

        public void EnableDiscard(bool state)
        {
            discardObject.SetActive(state);
            discardBanner.SetActive(state);
        }
    }
}