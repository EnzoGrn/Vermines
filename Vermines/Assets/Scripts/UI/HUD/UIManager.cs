using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Shop;

namespace Vermines.UI
{
    public class UIManager : MonoBehaviour
    {
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
        }

        public void OpenShopMarket()
        {
            ShopMarket.gameObject.SetActive(true);
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
            //shopB.gameObject.SetActive(false);
            //table.Close();
            //hand.Close();
            //book.Close();
        }
    }
}