using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines;

public class ShopUI : MonoBehaviourPunCallbacks, IPunObservable {

    #region Attributes

    [SerializeField]
    public Shop Market;

    [SerializeField]
    private GameObject[] _MarketCellsGO = new GameObject[5];

    [SerializeField]
    private GameObject _CardUIPrefabs;

    #endregion

    #region Methods

    public void Init()
    {
        Market = new Shop();

        Market.OnRefill += OnRefill;
    }

    public void Update()
    {
        if (Market == null)
            return;
        Deck shop = Market.AvailableMarket;

        for (int i = 0; i < shop.Cards.Count; i++) {
            MarketCell marketCell = _MarketCellsGO[i].GetComponent<MarketCell>();

            if (shop.Cards[i] == null) {
                if (marketCell != null && !GameManager.Instance.IsMyTurn()) {
                    foreach (Transform child in marketCell.transform)
                        Destroy(child.gameObject);
                }
            } else if (marketCell == null) {
                marketCell = _MarketCellsGO[i].AddComponent<MarketCell>();

                marketCell.OnClick += Buy;

                marketCell.Card = shop.GetCardByIndex(i);

                marketCell.CardUIView = CreateCardUIView(marketCell, marketCell.Card);
            } else if (marketCell.Card.ID != shop.Cards[i].ID) {
                marketCell.Card = shop.Cards[i];

                marketCell.CardUIView = CreateCardUIView(marketCell, marketCell.Card);
            }
        }
    }

    private CardUIView CreateCardUIView(MarketCell cell, ICard card)
    {
        GameObject cardView   = GameObject.Instantiate(_CardUIPrefabs);
        CardUIView cardUIView = cardView.GetComponent<CardUIView>();

        cardUIView.SetCard(card);

        RectTransform rectTransform = cell.GetComponent<RectTransform>();

        RectTransform cardRectTransform = cardUIView.GetComponent<RectTransform>();

        cardRectTransform.SetParent(rectTransform);

        cardRectTransform.localScale    = Vector3.one;
        cardRectTransform.localPosition = Vector3.zero;
        cardRectTransform.localRotation = Quaternion.identity;

        return cardUIView;
    }

    public void OnRefill()
    {
        Sync();
    }

    private void Buy(ICard card)
    {
        GameObject cellObj = _MarketCellsGO[Market.AvailableMarket.Cards.IndexOf(card)];
        MarketCell cell = cellObj.GetComponent<MarketCell>();
        bool     canBuy = Market.Buy(card);

        if (canBuy) {
            foreach (Transform child in cell.transform)
                Destroy(child.gameObject);
            Sync();
        }
    }

    public void Sync()
    {
        string syncJson = Market.DataToString();

        photonView.RPC("RPC_SyncShop", RpcTarget.OthersBuffered, syncJson);
    }

    [PunRPC]
    public void RPC_SyncShop(string shopJson)
    {
        if (!string.IsNullOrEmpty(shopJson))
            Market = JsonUtility.FromJson<Shop>(shopJson);
        RefreshShop();
    }

    private void RefreshShop()
    {
        foreach (ICard card in Market.AvailableMarket.Cards)
            if (card != null)
                card.IsAnonyme = false;
    }

    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(JsonUtility.ToJson(Market));
        } else {
            string playerDataJson = (string)stream.ReceiveNext();

            Market = JsonUtility.FromJson<Shop>(playerDataJson);
        }
    }

    #endregion
}
