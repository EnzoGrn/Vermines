using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DeckTester {

    static public string json1 = @"
        {
            ""id"": ""courtesan_000"",
            ""name"": ""Courtisane"",
            ""description"": ""Piochez deux cartes."",
            ""type"": 0,
            ""eloquence"": 12,
            ""souls"": 20,
            ""sprite"": ""Courtesan""
        }
    ";

    static public string json2 = @"
        {
            ""id"": ""apothecary_000"",
            ""name"": ""Apothicaire"",
            ""description"": ""Vos achats côutent <b>2E</b> de moins."",
            ""type"": 7,
            ""eloquence"": 8,
            ""souls"": 20,
            ""sprite"": ""Apothecary""
        }
    ";

    static public string json3 = @"
        {
            ""id"": ""worker_000"",
            ""name"": ""Ouvrière"",
            ""description"": ""Défaussez l'<b>ouvrière</b> pour gagnez 1E.<br>Si vous avez sacrifié 3 ouvrières au cours de la partie gagne <b>10A</b>."",
            ""type"": 0,
            ""souls"": 0,
            ""sprite"": ""Worker""
        }
    ";

    static public string json4 = @"
        {
            ""id"": ""priest_000"",
            ""name"": ""Prêtre"",
            ""description"": ""Gagnez <b>2E</b>."",
            ""type"": 5,
            ""eloquence"": 5,
            ""souls"": 15,
            ""sprite"": ""Priest""
        }
    ";

    static public string json5 = @"
        {
            ""id"": ""dragoleon_000"",
            ""name"": ""Dragoleon"",
            ""description"": ""Jouez le pouvoir d'un <b>partisan</b> joué par un adversaire ou présent dans la cour"",
            ""type"": 0,
            ""eloquence"": 12,
            ""souls"": 20,
            ""sprite"": ""Dragoleon""
        }
    ";

    static public Deck HandDeck;
    static public Deck Deck;

    public void AddCard(Deck deck, string jsonCard, int deckExpectedSize, bool randomly = false)
    {
        Assert.AreEqual(deck.GetNumberCards(), deckExpectedSize - 1);

        if (randomly)
            deck.AddRandomly(CardFactory.CreateCard(jsonCard));
        else
            deck.AddCard(CardFactory.CreateCard(jsonCard));

        Assert.AreEqual(deck.GetNumberCards(), deckExpectedSize);
    }

    [SetUp]
    public void Setup()
    {
        // Deck creation with 4 cards, generated from json1 to json4
        HandDeck = new();

        AddCard(HandDeck, json1, 1);
        AddCard(HandDeck, json2, 2);
        AddCard(HandDeck, json3, 3);
        AddCard(HandDeck, json4, 4);
        AddCard(HandDeck, json5, 5);

        // Deck creation with 4 cards, generated from json1 to json4 but added randomly
        Deck = new();

        AddCard(Deck, json1, 1, true);
        AddCard(Deck, json2, 2, true);
        AddCard(Deck, json3, 3, true);
        AddCard(Deck, json4, 4, true);
        AddCard(Deck, json5, 5, true);
    }

    public void CompareDeck(Deck deck1, Deck deck2, bool isSame = true)
    {
        bool isEgal = true;

        Assert.AreEqual(deck1.GetNumberCards(), deck2.GetNumberCards());

        for (int i = 0; i < deck1.GetNumberCards(); i++)
            if (deck1.Cards[i] != deck2.Cards[i])
                isEgal = false;

        if (isSame)
            Assert.IsTrue(isEgal);
        else
            Assert.IsFalse(isEgal);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void DeckTesterSimplePasses()
    {
        Assert.AreEqual(HandDeck.GetNumberCards(), 5);
        Assert.AreEqual(Deck.GetNumberCards()    , 5);

        Deck tempHandDeck = new();

        for (int i = 0; i < HandDeck.GetNumberCards(); i++)
            tempHandDeck.AddCard(HandDeck.Cards[i]);
        tempHandDeck.Shuffle();

        CompareDeck(HandDeck, tempHandDeck, false);
        CompareDeck(HandDeck, Deck        , false); // Note: if the result is not the one we expected, it's because the shuffle method is a random method, so it can be rarely the same

        Deck mergeDeck = new();

        mergeDeck.Merge(HandDeck);
        mergeDeck.Merge(Deck);

        Assert.AreEqual(mergeDeck.GetNumberCards(), HandDeck.GetNumberCards() + Deck.GetNumberCards());
    }
}
