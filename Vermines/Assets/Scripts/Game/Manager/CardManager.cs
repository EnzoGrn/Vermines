using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardManager : MonoBehaviour {

    #region Attributes

    public static CardManager Instance;

    private Dictionary<int, Deck> _PlayerDecks        = new();
    private Deck                  _MarketPartisanDeck = new();
    private Deck                  _MarketObjectDeck   = new();

    /*
     * @brief Deck containing all the cards of the game.
     */
    private Deck _AllCardsOfTheGame = new();

    #endregion

    #region Methods

    public void OnEnable()
    {
        if (Instance == null)
            Instance = this;
        CardLoader cardLoader = new();

        cardLoader.SetPlayerToLoad(PhotonNetwork.CurrentRoom.PlayerCount);

        _PlayerDecks        = cardLoader.LoadEveryPlayersDeck();
        _MarketPartisanDeck = cardLoader.LoadEveryPartisanCard();
        _MarketObjectDeck   = cardLoader.LoadEveryItemCard();

        for (int i = 0; i < _PlayerDecks.Count; i++)
            _AllCardsOfTheGame.Merge(_PlayerDecks[i]);
        _AllCardsOfTheGame.Merge(_MarketPartisanDeck);
        _AllCardsOfTheGame.Merge(_MarketObjectDeck);
    }

    #endregion

    #region Getters & Setters

    public Deck GetPlayerDeck(int playerID)
    {
        Debug.Assert(_PlayerDecks.ContainsKey(playerID), "Player deck not found in the dictionary.");

        return _PlayerDecks[playerID];
    }

    public Deck GetPartisanDeck()
    {
        return _MarketPartisanDeck;
    }

    public Deck GetObjectDeck()
    {
        return _MarketObjectDeck;
    }

    public ICard FoundACard(int id)
    {
        if (_AllCardsOfTheGame == null)
            return null;
        foreach (ICard card in _AllCardsOfTheGame.Cards)
            if (card.ID == id)
                return card;
        return null;
    }

    #endregion
}
