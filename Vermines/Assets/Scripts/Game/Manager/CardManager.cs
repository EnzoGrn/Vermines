using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour {

    #region Attributes

    private Dictionary<int, Deck> _PlayerDecks        = new();
    private Deck                  _MarketPartisanDeck = new();
    private Deck                  _MarketObjectDeck   = new();

    #endregion

    #region Methods

    public void OnEnable()
    {
        CardLoader cardLoader = new CardLoader();

        cardLoader.SetPlayerToLoad(PhotonNetwork.CurrentRoom.PlayerCount);

        _PlayerDecks        = cardLoader.LoadEveryPlayersDeck();
        _MarketPartisanDeck = cardLoader.LoadEveryPartisanCard();
        _MarketObjectDeck   = cardLoader.LoadEveryItemCard();

        Debug.Log($"Partisan Deck : {_MarketPartisanDeck.Cards.Count}");
        Debug.Log($"Object Deck   : {_MarketObjectDeck.Cards.Count}");

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            Debug.Log($"Player {i} Deck : {_PlayerDecks[i].Cards.Count}");
    }

    #endregion

    #region Getters & Setters

    public Deck GetPlayerDeck(int playerID)
    {
        Debug.Assert(_PlayerDecks.ContainsKey(playerID), "Player deck not found in the dictionary.");

        return _PlayerDecks[playerID];
    }

    #endregion
}
