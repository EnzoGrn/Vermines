using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Deck {

    public List<ICard> Cards;

    public Deck()
    {
        Cards = new();
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
}
