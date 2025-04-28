using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

#region Vermines CardSystem namespace
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.CardSystem.Elements;
#endregion

namespace Test.Vermines.CardSystem {

    public class TestCardSetDatabase {

        private static readonly int Seed = 123456789;

        private static readonly int NumberOfCardsForTwoPlayers   =  82;
        private static readonly int NumberOfCardsForThreePlayers =  93;
        private static readonly int NumberOfCardsForFourPlayers  = 104;

        private static readonly int NumberOfStarterCardsForThreePlayers = 15;

        [Test]
        public void TwoPlayersSet()
        {
            List<CardFamily> families = FamilyUtils.GenerateFamilies(Seed, 2);

            CardSetDatabase.Instance.Initialize(families);

            Assert.AreEqual(NumberOfCardsForTwoPlayers, CardSetDatabase.Instance.Size);

            CardSetDatabase.Instance.Reset();
        }

        [Test]
        public void ThreePlayersSet()
        {
            List<CardFamily> families = FamilyUtils.GenerateFamilies(Seed, 3);

            CardSetDatabase.Instance.Initialize(families);

            Assert.AreEqual(NumberOfCardsForThreePlayers, CardSetDatabase.Instance.Size);

            CardSetDatabase.Instance.Reset();
        }

        [Test]
        public void FourPlayersSet()
        {
            List<CardFamily> families = FamilyUtils.GenerateFamilies(Seed, 4);

            CardSetDatabase.Instance.Initialize(families);

            Assert.AreEqual(NumberOfCardsForFourPlayers, CardSetDatabase.Instance.Size);

            CardSetDatabase.Instance.Reset();
        }

        [Test]
        public void GetEveryStarterCard()
        {
            List<CardFamily> families = FamilyUtils.GenerateFamilies(Seed, 3);

            CardSetDatabase.Instance.Initialize(families);

            List<ICard> starterCards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == true);

            Assert.AreEqual(NumberOfStarterCardsForThreePlayers, starterCards.Count);

            CardSetDatabase.Instance.Clear();
        }

        [Test]
        public void GetCard()
        {
            Assert.IsNull(CardSetDatabase.Instance.GetCardByID("5"));
            Assert.IsNull(CardSetDatabase.Instance.GetCardByID(5));

            List<CardFamily> families = FamilyUtils.GenerateFamilies(Seed, 3);

            CardSetDatabase.Instance.Initialize(families);

            ICard cardbyIDInt = CardSetDatabase.Instance.GetCardByID(5);

            Assert.IsNotNull(cardbyIDInt);
            Assert.IsNull(CardSetDatabase.Instance.GetCardByID(5000000));

            ICard cardbyIDString = CardSetDatabase.Instance.GetCardByID("5");

            Assert.IsNotNull(cardbyIDString);
            Assert.IsNull(CardSetDatabase.Instance.GetCardByID("5000000"));

            CardSetDatabase.Instance.Clear();
        }

        [Test]
        public void GetCards()
        {
            List<CardFamily> families = FamilyUtils.GenerateFamilies(Seed, 3);

            CardSetDatabase.Instance.Initialize(families);

            List<ICard> cardbyIDIntArray = CardSetDatabase.Instance.GetCardByIds(new int[] { 5, 6, 7 });

            Assert.AreEqual(3, cardbyIDIntArray.Count);
            Assert.AreEqual(2, CardSetDatabase.Instance.GetCardByIds(new int[] { 2, 6, 50000000 }).Count);
            Assert.AreEqual(0, CardSetDatabase.Instance.GetCardByIds(new int[] { -5 }).Count);

            List<ICard> cardbyIDStringArray = CardSetDatabase.Instance.GetCardByIds("5,6,7");

            Assert.AreEqual(3, cardbyIDStringArray.Count);
            Assert.AreEqual(2, CardSetDatabase.Instance.GetCardByIds("5,6,50000000").Count);
            Assert.AreEqual(0, CardSetDatabase.Instance.GetCardByIds("     ").Count);

            CardSetDatabase.Instance.Clear();
        }

        [Test]
        public void Debug_PrintDatabase()
        {
            List<CardFamily> families = FamilyUtils.GenerateFamilies(Seed, 1);

            CardSetDatabase.Instance.Initialize(families);
            CardSetDatabase.Instance.Print();
            CardSetDatabase.Instance.Reset();
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
            } catch (System.Exception) {
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

            card.IsAnonyme = true;

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

    public class TestFamilyCard {

        [Test]
        public void FamilyListToIds()
        {
            int[] ids = { 1, 3, 4, 6 };
            List<CardFamily> families = FamilyUtils.FamiliesIdsToList(ids);

            Assert.AreEqual(4, families.Count);
            Assert.AreEqual(CardFamily.Cricket, families[0]);
            Assert.AreEqual(CardFamily.Fly, families[1]);
            Assert.AreEqual(CardFamily.Ladybug, families[2]);
            Assert.AreEqual(CardFamily.Scarab, families[3]);
        }

        [Test]
        public void FamiliesIdsToList()
        {
            List<CardFamily> families = new() {
                CardFamily.Cricket,
                CardFamily.Fly,
                CardFamily.Ladybug,
                CardFamily.Scarab
            };
            int[] ids = FamilyUtils.FamiliesListToIds(families);

            Assert.AreEqual(4, ids.Length);
            Assert.AreEqual(1, ids[0]);
            Assert.AreEqual(3, ids[1]);
            Assert.AreEqual(4, ids[2]);
            Assert.AreEqual(6, ids[3]);
        }
    }
}
