using UnityEngine;

namespace Vermines.HUD
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager instance;

        [SerializeField] private GameObject marketOverlay;
        [SerializeField] private GameObject courtyardOverlay;

        private Shop currentShop;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            marketOverlay.SetActive(false);
            courtyardOverlay.SetActive(false);
            currentShop = null;
        }

        public void OpenMarket()
        {
            marketOverlay.SetActive(true);
            currentShop = marketOverlay.GetComponent<Shop>();
            if (currentShop == null)
            {
                Debug.LogError("Shop component not found on marketOverlay GameObject.");
                return;
            }
            courtyardOverlay.SetActive(false);
        }

        public void OpenCourtyard()
        {
            marketOverlay.SetActive(false);
            courtyardOverlay.SetActive(true);
            currentShop = courtyardOverlay.GetComponent<Shop>();
            if (currentShop == null)
            {
                Debug.LogError("Shop component not found on courtyardOverlay GameObject.");
                return;
            }
        }

        public void CloseShop()
        {
            if (instance != null)
            {
                if (currentShop != null)
                    currentShop.CloseCardBuyOverlay();
                marketOverlay.SetActive(false);
                courtyardOverlay.SetActive(false);
                currentShop = null;
            }
        }

        public Shop GetShop()
        {
            return currentShop;
        }
    }
}
