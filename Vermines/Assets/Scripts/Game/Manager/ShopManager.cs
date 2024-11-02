using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour {

    #region Attributes

    public ShopUI PartisanShop;
    public ShopUI ObjectShop;

    #endregion

    #region Methods

    public void OnEnable()
    {
        PartisanShop.Init();
        ObjectShop.Init();

        PartisanShop.Market.Deck = CardManager.Instance.GetPartisanDeck();
        ObjectShop.Market.Deck   = CardManager.Instance.GetObjectDeck();

        Refill();
    }

    #endregion

    public void Refill()
    {
        PartisanShop.Market.Refill();
        ObjectShop.Market.Refill();

        PartisanShop.Sync();
        ObjectShop.Sync();
    }
}
