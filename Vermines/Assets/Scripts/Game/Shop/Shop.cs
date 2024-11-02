using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop {

    #region Attributes

    /*
     * @brief This deck contains every card of the shop, that are not buyed yet, or discarded.
     * When is empty, the shop is refilled with the cards of the DiscardDeck.
     */
    [SerializeField]
    public Deck Deck = new();

    /*
     * @brief This deck contains the cards that know one wants to buy.
     * Each time a shop card is discarded, it is added to this deck.
     * And when the main deck is empty, the shop is refilled with the cards of this deck.
     */
    [SerializeField]
    public Deck DiscardDeck = new();

    /*
     * @brief This deck contains the cards that are available to buy.
     * It size is limited to 5 cards.
     */
    [SerializeField]
    public Deck AvailableMarket = new(5);

    public event Action OnRefill;

    #endregion

    #region Methods

    public bool Buy(ICard card)
    {
        if (!GameManager.Instance.IsMyTurn())
            return false;
        if (!PlayerController.localPlayer.CanBuy(card.Data.Eloquence)) {
            // TODO: Display a message to the player.
            Debug.Log("You can't buy this card.");

            return false;
        }
        ICard cardToBuy = AvailableMarket.Cards.Find(c => {
            if (c == null)
                return false;
            return c.ID == card.ID;
        });

        if (cardToBuy == null)
            return false;

        AvailableMarket.Cards[AvailableMarket.Cards.IndexOf(cardToBuy)] = null;

        PlayerController.localPlayer.BuyCard(cardToBuy);

        return true;
    }

    public void ChangeCard(ICard card)
    {
        if (!GameManager.Instance.IsMyTurn())
            return;
        ICard cardToChange = AvailableMarket.Cards.Find(c => c.ID == card.ID);

        if (cardToChange == null)
            return;
        DiscardDeck.Cards.Add(cardToChange);
        AvailableMarket.Cards[AvailableMarket.Cards.IndexOf(cardToChange)] = Deck.PickACard();

        if (AvailableMarket.Cards[AvailableMarket.Cards.IndexOf(cardToChange)] == null) {
            RefreshDeck();

            AvailableMarket.Cards[AvailableMarket.Cards.IndexOf(cardToChange)] = Deck.PickACard();

            // -- If failed again, it's mean that the deck and the discard deck are empty.
        }
    }

    public void Refill()
    {
        if (!GameManager.Instance.IsMyTurn())
            return;
        for (int i = 0; i < AvailableMarket.Cards.Count; i++) {
            if (AvailableMarket.Cards[i] == null) {
                AvailableMarket.Cards[i] = Deck.PickACard();

                if (AvailableMarket.Cards[i] == null) {
                    RefreshDeck();

                    AvailableMarket.Cards[i] = Deck.PickACard();

                    // -- If failed again, it's mean that the deck and the discard deck are empty.
                }

                if (AvailableMarket.Cards[i] != null)
                    AvailableMarket.Cards[i].IsAnonyme = false;
            }
        }

        OnRefill?.Invoke();
    }

    private void RefreshDeck()
    {
        Deck.Merge(DiscardDeck);

        DiscardDeck.Cards.Clear();
    }

    public string DataToString()
    {
        Debug.Log(JsonUtility.ToJson(this));

        return JsonUtility.ToJson(this);
    }

    #endregion
}
