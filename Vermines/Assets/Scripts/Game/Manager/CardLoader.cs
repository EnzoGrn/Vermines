using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardLoader {

    #region Attributes

    public int CardInstanciated = 0;

    private string _ScriptableCardsPath = "ScriptableObject/Cards/";

    #endregion

    #region Constructor

    public CardLoader()
    {
        LoadEverything();
    }

    #endregion

    #region Methods

    private void LoadEverything()
    {
        LoadEveryPlayersDeck();
        LoadEveryPartisanCard();
        LoadEveryItemCard();
    }

    private void LoadEveryPlayersDeck()
    {
        string path = $"{_ScriptableCardsPath}StartingCards/";

        for (int i = 0; i < GameManager.Instance.GetNumbersOfPlayer; i++)
            GameManager.Instance.GetPlayer(i).Deck = LoadDeckFromPath(path);
    }

    private void LoadEveryPartisanCard()
    {
        Deck firstLevelDeck  = LoadEveryPartisanFromLevel(1);
        Deck firstFamilyDeck = LoadEveryFamilyCard(1);

        firstLevelDeck.Merge(firstFamilyDeck);
        firstLevelDeck.Shuffle();

        Deck secondLevelDeck  = LoadEveryPartisanFromLevel(2);
        Deck secondFamilyDeck = LoadEveryFamilyCard(2);

        secondLevelDeck.Merge(secondFamilyDeck);
        secondLevelDeck.Shuffle();

        firstLevelDeck.Merge(secondLevelDeck);

        // TODO: Fill the market (partisan) by giving the deck to the GameManager
    }

    private Deck LoadEveryFamilyCard(int level)
    {
        List<CardType> types = GameManager.Instance.GetAllFamilyPlayed();
        string path = $"{_ScriptableCardsPath}/Partisans/{level}/Family/";
        Deck deck = new();

        for (int i = 0; i < types.Count; i++) {
            Deck family = LoadDeckFromPath(path);

            foreach (ICard card in family.Cards) {
                CardData data = card.Data;

                data.Type = types[i];
            }

            deck.Merge(family);
        }

        return deck;
    }

    private void LoadEveryItemCard()
    {
        Deck equipment = LoadDeckFromPath($"{_ScriptableCardsPath}Item/Equipment/");
        Deck tools     = LoadDeckFromPath($"{_ScriptableCardsPath}Item/Tools/");

        equipment.Merge(tools);
        equipment.Shuffle();

        // TODO: Fill the market (item) by giving the deck to the GameManager
    }

    private Deck LoadEveryPartisanFromLevel(int level)
    {
        string path = $"{_ScriptableCardsPath}Partisans/{level}/";
        Deck   deck = new();

        foreach (string type in Constants.BasicTypes) {
            string typePath = $"{path}{type}/";

            deck.Merge(LoadDeckFromPath(typePath));
        }
        return deck;
    }

    private Deck LoadDeckFromPath(string path)
    {
        CardData[] cardDataArray = Resources.LoadAll<CardData>(path);

        if (cardDataArray.Length == 0) {
            Debug.LogWarning($"No Card found in Resources/{path}.");

            return null;
        }
        Deck deck = new();

        foreach (CardData cardData in cardDataArray)
            deck.AddRandomly(LoadXTimeTheCard(cardData, cardData.ID));
        return deck;
    }

    private List<ICard> LoadXTimeTheCard(CardData data, int numberNeeded)
    {
        List<ICard> cards = new();

        for (int i = 0; i < numberNeeded; i++) {
            data.ID = CardFactory.CardCount;

            ICard card = CardFactory.CreateCard(data);

            cards.Add(card);
        }

        return cards;
    }

    #endregion
}
