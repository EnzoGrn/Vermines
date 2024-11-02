using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

[Serializable]
public class Deck : ISerializationCallbackReceiver {

    public List<ICard> Cards { get; set; }

    public Deck()
    {
        Cards = new();
    }

    public Deck(int numberCards)
    {
        Cards = new();

        for (int i = 0; i < numberCards; i++)
            Cards.Add(null);
    }

    public void Shuffle()
    {
        int numberCards = GetNumberCards();

        System.Random rng = new();

        for (int i = 0; i < numberCards; i++) {
            int   randomIndex = rng.Next(i, numberCards);
            ICard temp        = Cards[i];

            Cards[i]           = Cards[randomIndex];
            Cards[randomIndex] = temp;
        }
    }

    public int GetNumberCards()
    {
        return Cards.Count;
    }

    public void AddCard(ICard card)
    {
        Cards.Add(card);
    }

    public void RemoveCard(ICard card)
    {
        Cards.Remove(card);
    }

    public void AddRandomly(ICard card)
    {
        System.Random rng = new();

        int randomIndex = rng.Next(0, GetNumberCards());

        Cards.Insert(randomIndex, card);
    }

    public void AddRandomly(List<ICard> cards)
    {
        if (cards == null || cards.Count == 0)
            return;
        foreach (ICard card in cards)
            AddRandomly(card);
    }

    public void Merge(Deck other)
    {
        if (other == null)
            return;
        foreach (ICard card in other.Cards)
            AddCard(card);
    }

    public ICard PickACard()
    {
        if (GetNumberCards() == 0)
            return null;
        ICard card = Cards[GetNumberCards() - 1];

        Cards.RemoveAt(GetNumberCards() - 1);

        return card;
    }

    public ICard GetCard(int cardID)
    {
        foreach (ICard card in Cards) {
            if (card.ID == cardID)
                return card;
        }

        return null;
    }

    public ICard GetCardByIndex(int index)
    {
        if (index < 0 || index >= GetNumberCards())
            return null;
        return Cards[index];
    }

    public ICard GetLast()
    {
        int numberCards = GetNumberCards();

        if (numberCards == 0)
            return null;
        return Cards[numberCards - 1];
    }

    #region ISerializationCallbackReceiver implementation

    [SerializeField]
    private List<string> _IDs = new();

    public void OnBeforeSerialize()
    {
        _IDs.Clear();

        for (int i = 0; i < Cards.Count; i++) {
            int id = -1;

            if (Cards[i] != null)
                id = Cards[i].ID;

            _IDs.Add(id.ToString());
        }
    }

    public void OnAfterDeserialize()
    {
        Cards = new();

        for (int i = 0; i < _IDs.Count; i++) {
            try {
                if (_IDs[i] == "-1") {
                    Cards.Add(null);
                } else {
                    ICard card = CardManager.Instance.FoundACard(int.Parse(_IDs[i]));

                    if (card != null)
                        Cards.Add(card);
                }
            } catch (Exception e) {
                Debug.Log($"Error on card {_IDs[i]}: " + e.Message + " - " + e.StackTrace);

                continue;
            }
        }
    }

    #endregion
}
