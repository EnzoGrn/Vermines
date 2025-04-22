using UnityEngine;
using Vermines.UI.Shop;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.GameTable;

namespace Vermines.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public TableUI table;
        //public HandUI hand;
        //public BookUI book;

        private void Awake()
        {
            // Initialize the UI elements
            //hand.Init();
            //table.Init();
            //book.Init();

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OpenShopMarket()
        {
            CloseContextualUI();
            ShopUIManager.Instance.OpenShop(ShopType.Market);
            //CamManager.Instance.GoOnMarketLocation();
        }

        public void OpenShopCourtyard()
        {
            CloseContextualUI();
            ShopUIManager.Instance.OpenShop(ShopType.Courtyard);
            //CamManager.Instance.GoOnCourtyardLocation();
        }

        public void CloseContextualUI()
        {
            ShopUIManager.Instance.CloseCurrentShop();
            if (UIContextManager.Instance.CurrentContext is ShopConfirmPopup)
            {
                Debug.Log("Closing ShopConfirmPopup");
                UIContextManager.Instance.ClearContext();
            }
        }

        //public void OpenBook() => book.Open();
        public void OpenTable() => table.OpenTableUI();

        public void CloseAll()
        {
            ShopUIManager.Instance.CloseCurrentShop();
            //shopB.gameObject.SetActive(false);
            //table.Close();
            //hand.Close();
            //book.Close();
        }
    }
}