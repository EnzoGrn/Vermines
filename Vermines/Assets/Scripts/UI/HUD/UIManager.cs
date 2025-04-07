using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Shop;

namespace Vermines.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public ShopMarketUI ShopMarket;
        //public ShopBUI shopB;
        //public TableUI table;
        //public HandUI hand;
        //public BookUI book;

        private void Awake()
        {
            // Initialize the UI elements
            //hand.Init();
            //table.Init();
            //book.Init();
            ShopMarket.gameObject.SetActive(false);
            //shopB.gameObject.SetActive(false);

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
            ShopMarket.gameObject.SetActive(true);
            //CamManager.Instance.GoOnMarketLocation();
        }

        //public void OpenShopB(List<CardData> cards)
        //{
        //    shopB.gameObject.SetActive(true);
        //    shopB.Init(cards);
        //}

        //public void OpenBook() => book.Open();
        //public void OpenTable() => table.Open();

        public void CloseAll()
        {
            ShopMarket.gameObject.SetActive(false);
            CamManager.Instance.GoOnNoneLocation();
            //shopB.gameObject.SetActive(false);
            //table.Close();
            //hand.Close();
            //book.Close();
        }
    }
}