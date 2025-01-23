// -- Remove the warning CS0168 (Unused variable) for the whole file
#pragma warning disable 0168

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#region Vermines CardSystem namespace
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.CardSystem.Elements;
using System.Reflection;
#endregion

namespace Test.Vermines.CardSystem {


    public class TestCardSetDatabase {

        private static int NumberOfCardsForTwoPlayers = 112;
        private static int NumberOfCardsForThreePlayers = 123;
        private static int NumberOfCardsForFourPlayers = 134;

        [Test]
        public void TwoPlayersSet()
        {
            List<CardFamily> families = new() {
                CardFamily.Ladybug,
                CardFamily.Scarab
            };

            CardSetDatabase.Instance.Initialize(families);

            Assert.AreEqual(NumberOfCardsForTwoPlayers, CardSetDatabase.Instance.Size);

            Assert.IsTrue(CardSetDatabase.Instance.CardExist(5));
            Assert.IsNotNull(CardSetDatabase.Instance.GetCardByID(5));

            Assert.IsFalse(CardSetDatabase.Instance.CardExist(5000000));
            Assert.IsNull(CardSetDatabase.Instance.GetCardByID(5000000));

            CardSetDatabase.Instance.Reset();

            Assert.IsFalse(CardSetDatabase.Instance.CardExist(5));
            Assert.IsNull(CardSetDatabase.Instance.GetCardByID(5));
        }

        [Test]
        public void ThreePlayersSet()
        {
            List<CardFamily> families = new() {
                CardFamily.Ladybug,
                CardFamily.Scarab,
                CardFamily.Fly
            };

            CardSetDatabase.Instance.Initialize(families);

            Assert.AreEqual(NumberOfCardsForThreePlayers, CardSetDatabase.Instance.Size);
        }

        [Test]
        public void FourPlayersSet()
        {
            List<CardFamily> families = new() {
                CardFamily.Ladybug,
                CardFamily.Scarab,
                CardFamily.Fly,
                CardFamily.Cricket
            };

            CardSetDatabase.Instance.Initialize(families);

            Assert.AreEqual(NumberOfCardsForFourPlayers, CardSetDatabase.Instance.Size);
        }
    }

    public class TestCardLoader {

        [Test]
        public void NoInitialization()
        {
            CardLoader loader = new();

            List<ICard> cards = loader.Load();

            Assert.IsNull(cards);
        }
    }

    public class TestCardFactory {

        [Test]
        public void CreateCardWithoutData()
        {
            ICard card = CardFactory.CreateCard(null);

            Assert.IsNull(card);
        }
    }

    public class TestCardBuilder {

        [Test]
        public void UnknownTypeCard()
        {
            try {
                CardBuilder builder = new();

                builder.CreateCard(CardType.None).Build();
            } catch (System.Exception _) {
                Assert.Pass();
            }
        }
    }
    
    public class TestCard {

        [Test]
        public void TryAccessAnonymousCard()
        {
            List<CardFamily> families = new() {
                CardFamily.Ladybug
            };

            CardSetDatabase.Instance.Initialize(families);

            ICard card = CardSetDatabase.Instance.GetCardByID(5);

            Assert.IsNotNull(card);
            Assert.IsNull(card.Data);

            card.IsAnonyme = false;

            Assert.IsNotNull(card.Data);
        }

        [Test]
        public void TestCardData()
        {
            CardData data = ScriptableObject.CreateInstance<CardData>();

            data.Type         = CardType.Partisan;
            data.Family       = CardFamily.Ladybug;
            data.Level        = 2;
            data.Souls        = 15;
            data.IsFamilyCard = true;
            data.SpriteName   = "Merchant";

            Assert.AreEqual(CardFamily.Ladybug, data.Family);
            Assert.AreEqual(2, data.Level);
            Assert.AreEqual(15, data.Souls);
            Assert.AreEqual("Merchant", data.SpriteName);

            data.Type         = CardType.Tools;
            data.IsFamilyCard = false;

            Assert.AreNotEqual(CardFamily.Ladybug, data.Family);
            Assert.AreNotEqual(2, data.Level);
            Assert.AreNotEqual(15, data.Souls);
            Assert.AreNotEqual("Merchant", data.SpriteName);

            data.Family = CardFamily.Scarab;
            data.Level = 3;
            data.Souls = 25;
            data.SpriteName = "Archbishop";

            Assert.AreNotEqual(CardFamily.Scarab, data.Family);
            Assert.AreNotEqual( 3, data.Level);
            Assert.AreNotEqual(25, data.Souls);
            Assert.AreNotEqual("Archbishop", data.SpriteName);

            Assert.IsNull(data.Sprite);
        }
    }
}
