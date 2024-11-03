using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BuildingCardSystem {

    static public string jsonCard = @"       
        {
            ""id"": 1,
            ""name"": ""Bard"",
            ""description"": ""Gagnez 8E."",
            ""type"": 0,
            ""eloquence"": 14,
            ""souls"": 25,
            ""sprite"": ""Bard"",
            ""turnStartEffect"": ""WonEloquence"",
            ""turnStartParameters"": [
                8
            ]
        }
    ";

    /*
     * @note If I didn't test the bad json format by giving a null when creating a card, or by giving a json without ID.
     * It's because Unit Test get Debug.Assert() so my code automatically fail due to a crash, that is not here in Release mode.
     * Solution is maybe to wrap the Debug.Assert() to ignore them in Unit Test.
     */

    [Test]
    public void BuildingCardSystemSimplePasses()
    {
        ICard card = CardFactory.CreateCard(jsonCard);

        Assert.AreNotEqual(card, null);

        if (card is PartisanCard) {
            PartisanCard partisanCard = (PartisanCard)card;

            Assert.AreEqual(card, partisanCard);
        } else {
            Assert.AreNotEqual(false, false); // Test not past because the card is not a PartisanCard
        }

        CardData cardData = card.Data;

        if (cardData != null) {
            Assert.AreEqual(cardData.ID, 1);
            Assert.AreEqual(cardData.Name, "Bard");
            Assert.AreEqual(cardData.Description, "Gagnez 8E.");
            Assert.AreEqual(cardData.Type, CardType.Bee);
            Assert.AreEqual(cardData.Eloquence, 14);
            Assert.AreEqual(cardData.Souls, 25);
            Assert.AreNotEqual(cardData.Sprite, null);
            Assert.AreEqual("Assets/Sprites/Card/Bee/Bard.png", "Assets/Sprites/Card/" + cardData.Type.ToString() + "/" + cardData.Name + ".png");
        } else {
            Assert.AreNotEqual(false, false); // Test not past because the cardData is null
        }

        ICard card2 = CardFactory.CreateCard(card.Data);

        Assert.AreNotEqual(card2, null);

        if (card2 is PartisanCard)
        {
            PartisanCard partisanCard2 = (PartisanCard)card2;

            Assert.AreEqual(card2, partisanCard2);
        } else {
            Assert.AreNotEqual(false, false); // Test not past because the card2 is not a PartisanCard
        }

        CardData cardData2 = card2.Data;

        if (cardData2 != null) {
            Assert.AreEqual(cardData.ID, 1);
            Assert.AreEqual(cardData2.Name, "Bard");
            Assert.AreEqual(cardData2.Description, "Gagnez 8E.");
            Assert.AreEqual(cardData2.Type, CardType.Bee);
            Assert.AreEqual(cardData2.Eloquence, 14);
            Assert.AreEqual(cardData2.Souls, 25);
            Assert.AreNotEqual(cardData2.Sprite, null);
            Assert.AreEqual("Assets/Sprites/Card/Bee/Bard.png", "Assets/Sprites/Card/" + cardData2.Type.ToString() + "/" + cardData2.Name + ".png");
        } else {
            Assert.AreNotEqual(false, false); // Test not past because the cardData2 is null
        }
    }
}
