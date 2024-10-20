using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardLoader {

    #region Attributes

    private int _PlayerToLoad = 0;

    #endregion

    #region Constructor

    public CardLoader() {}

    #endregion

    #region Methods

    /*
     * @brief Function that depending to the number of player in the party,
     * will create to each one a deck with the 5 starting card 
     * 
     * @note If you call this function directly after the constructor, it will return an empty dictionary
     * Please call SetPlayerToLoad before calling this function
     */
    public Dictionary<int, Deck> LoadEveryPlayersDeck()
    {
        Dictionary<int, Deck> playerDecks = new();

        for (int i = 0; i < _PlayerToLoad; i++)
            playerDecks.Add(i, LoadDeckFromPath(Constants.PathToStartingCard));
        return playerDecks;
    }

    /*
     * @brief Function that will load every partisan card from the game,
     * will shuffle them and sort them by level (I and II), then merge them and return them
     */
    public Deck LoadEveryPartisanCard()
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

        return firstLevelDeck;
    }

    /*
     * @brief Function that will load every family card from the game depending on the PlayerData Family.
     * Like the LoadEveryPartisanCard function, it will shuffle them and sort them by level (I and II), then merge them and return them
     */
    private Deck LoadEveryFamilyCard(int level)
    {
        List<CardType> types = GameManager.Instance.GetAllFamilyPlayed();

        string path = $"{Constants.PathToPartisan}{level}/Family/";
        Deck   deck = new();

        for (int i = 0; i < types.Count; i++) {
            // -- Create a new deck for one family, and set all this card to the same type
            Deck family = LoadDeckFromPath(path);

            foreach (ICard card in family.Cards) {
                card.IsAnonyme = false;

                CardData data = card.Data;

                data.Type = types[i];

                card.IsAnonyme = true;
            }

            deck.Merge(family);
        }
        return deck;
    }

    /*
     * @brief Function that will load every item card from the game,
     * will shuffle them and return them.
     */
    public Deck LoadEveryItemCard()
    {
        Deck equipment = LoadDeckFromPath(Constants.PathToEquipmentCard);
        Deck tools     = LoadDeckFromPath(Constants.PathToToolsCard);

        equipment.Merge(tools);
        equipment.Shuffle();

        return equipment;
    }

    /*
     * @brief Function that will load every Partisan of neutral types depending on their level in Game.
     * Currently, level can only be (1 or 2).
     * 
     * @note If the level is not 1 or 2, it will return an empty deck.
     * 
     * @note The neutral types are: Bee, Mosquito and Firefly.
     */
    private Deck LoadEveryPartisanFromLevel(int level)
    {
        Deck deck = new();

        foreach (string type in Constants.BasicTypes) {
            string typePath = $"{Constants.PathToPartisan}{level}/{type}/";

            deck.Merge(LoadDeckFromPath(typePath));
        }
        return deck;
    }

    /*
     * @brief Function that will load every CardData from a specific path in the Resources folder,
     * and put them in a deck.
     * 
     * @note Deck given by this function is shuffled.
     */
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

    /*
     * @brief Load the same card multiple time depending on each examplair needed.
     * 
     * @note Currently for know the number of examplair needed, we use the ID of the CardData.
     * Once this ID is used, we put the real ID used in the game that is the increment of the CardFactory.CardCount.
     */
    private List<ICard> LoadXTimeTheCard(CardData data, int numberNeeded)
    {
        List<ICard> cards = new();

        for (int i = 0; i < numberNeeded; i++) {
            CardData copyData = Object.Instantiate(data);

            copyData.ID        = CardFactory.CardCount;
            copyData.IsAnonyme = true;

            ICard card = CardFactory.CreateCard(copyData);

            cards.Add(card);
        }

        return cards;
    }

    #endregion

    #region Setters

    /*
     * @brief Function that will set the number of starting deck to load.
     */
    public void SetPlayerToLoad(int playerIndex)
    {
        _PlayerToLoad = playerIndex;
    }

    #endregion
}
