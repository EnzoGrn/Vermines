using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Fusion;

using OMGG.DesignPattern;

#region Vermines ShopSystem namespace

using Vermines.ShopSystem.Enumerations;
using Vermines.ShopSystem.Commands;
using Vermines.ShopSystem.Data;

#endregion

#region Vermines CardSystem namespace

using Vermines.CardSystem.Enumerations;
using Vermines.CardSystem.Utilities;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Elements;

#endregion

#region Vermines Test namespace

using Vermines.Test;
using Vermines.Configuration;
using Vermines.Player;
using Vermines;
using Vermines.ShopSystem;
using UnityEditor.Graphs;
using UnityEngine.PlayerLoop;
using Vermines.Core.Scene;
using Vermines.Core;
using static System.Collections.Specialized.BitVector32;

#endregion

namespace Test.Vermines.ShopSystem
{

    public class TestShopSystem
    {

        private PlayerRef _LocalPlayer;

        private PlayerController _Player;

        int Seed => 0x015;

        #region Setup

        [SetUp]
        public void Setup()
        {
            // -- Initialize a card data set for a two players game
            CardSetDatabase.Instance.Initialize(FamilyUtils.GenerateFamilies(Seed, 2), new SceneContext());

            // -- Player initialization
            _LocalPlayer = PlayerRef.FromEncoded(0x01);

            _Player = new();

            _Player.UpdateDeck(new());

            _Player.Deck.Initialize(Seed);

            // -- Active the test mode to bypass the HUD system
            TestMode.IsTesting = true;
        }

        #endregion

        #region Teardown
        [TearDown]
        public void Teardown()
        {
            TestMode.IsTesting = false;
        }
        #endregion

        #region Initialization

        private ShopData InitializeShop()
        {
            // 1. Get all buyable card.
            List<ICard> everyBuyableCard = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == false);

            if (everyBuyableCard == null || everyBuyableCard.Count == 0)
                return null;

            // 2. Filter card per type.

            // 2.a. Object (Tools & Equipment).
            List<ICard> objectCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Equipment || card.Data.Type == CardType.Tools).ToList();

            objectCards.Shuffle(Seed);

            // 2.b. Partisan (filter per level).
            List<ICard> partisanCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Partisan).ToList();
            List<ICard> partisan1Cards = partisanCards.Where(card => card.Data.Level == 1).ToList();
            List<ICard> partisan2Cards = partisanCards.Where(card => card.Data.Level == 2).ToList();

            partisan1Cards.Shuffle(Seed);
            partisan2Cards.Shuffle(Seed);

            // 3. Create the shop.
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            // 3.a. Courtyard Initialization.
            int level1Slot = 3;
            int level2Slot = 2;

            CourtyardSection courtyard = new(level1Slot, level2Slot);

            courtyard.Deck1 = partisan1Cards;
            courtyard.Deck2 = partisan2Cards;

            for (int i = 0; i < level1Slot; i++)
                courtyard.AddCard(1);
            for (int i = 0; i < level2Slot; i++)
                courtyard.AddCard(2);
            shop.AddSection(ShopType.Courtyard, courtyard);

            // 3.b. Market Initialization.
            var groupedByName = objectCards.GroupBy(c => c.Data.Name).ToDictionary(g => g.Key, g => g.ToList());

            MarketSection market = new(groupedByName.Count);

            int index = 0;

            foreach (var kvp in groupedByName) {
                foreach (ICard card in kvp.Value)
                    market.CardPiles[index].Add(card);
                index++;
            }

            shop.AddSection(ShopType.Market, market);

            return shop;
        }

        private ShopData InitializeEmptyShop()
        {
            // 3. Create the shop.
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            // 3.a. Courtyard Initialization.
            CourtyardSection courtyard = new();

            shop.AddSection(ShopType.Courtyard, courtyard);

            // 3.b. Market Initialization.
            MarketSection market = new(0);

            shop.AddSection(ShopType.Market, market);

            return shop;
        }

        #endregion

        /*[Test]
        public void Serialization()
        {
            // -- Initialize the shop
            ShopData shop1 = InitializeShop();

            // Serialize the shop (first time)
            string data1 = shop1.Serialize();

            Debug.Log(data1);

            // -- Synchronise the shop2 with data of shop1
            ShopData shop2 = InitializeEmptyShop();

            ICommand syncCommand = new SyncShopCommand(shop2, data1);

            CommandInvoker.ExecuteCommand(syncCommand);

            // Serialize the shop (second time)
            string data2 = shop2.Serialize();

            Debug.Log(data2);

            // Compare the two serialized data
            Assert.AreEqual(data1, data2);

            // TODO: Undo command
        }*/

        #region Buy Command

        // TODO: Try to test admin side buy command.

        /*[Test]
        public void ClientBuyPartisanInShop()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop();

            // -- Store the card before the buy
            CourtyardSection courtyard = (CourtyardSection)shop.Sections[ShopType.Courtyard];

            ICard cardBeforeTheBuy = courtyard.AvailableCards[0];

            // -- Buy a card in the 'Courtyard' the cardBeforeTheBuy
            ShopArgs parameters = new(shop, ShopType.Courtyard, cardBeforeTheBuy.ID);

            ICommand buyCommand = new CLIENT_BuyCommand(_Player, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            // -- Check if the card is correctly bought
            ICard cardAfterTheBuy = courtyard.AvailableCards[0];

            // -- Check that the card after is a null card (because the shop didn't be refilled)
            Assert.IsNull(cardAfterTheBuy);

            // -- Check that the player have now a new card in his discard deck
            Assert.AreEqual(1, _Player.Deck.Discard.Count);

            // -- Check that the card store before buy is in the discard deck
            Assert.AreEqual(cardBeforeTheBuy.ID, _Player.Deck.Discard[0].ID);

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // TODO: Test the undo command, when it will be implemented in the buy command.
        }

        [Test]
        public void ClientBuyToolsInShop()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop();

            // -- Store the card before the buy
            MarketSection market = (MarketSection)shop.Sections[ShopType.Market];

            ICard cardBeforeTheBuy = market.CardPiles[0][^1];

            // -- Buy a card in the 'Market' the cardBeforeTheBuy
            ShopArgs parameters = new(shop, ShopType.Market, cardBeforeTheBuy.ID);

            ICommand buyCommand = new CLIENT_BuyCommand(_Player, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            // -- Check if the card is correctly bought
            ICard cardAfterTheBuy = market.CardPiles[0][^1];

            // -- Check that the card after is a new exemplars (because the market auto refilled).
            Assert.IsFalse(cardBeforeTheBuy.ID == cardAfterTheBuy.ID);
            Assert.IsTrue(cardBeforeTheBuy.Data.Name == cardAfterTheBuy.Data.Name);

            // -- Check that the player have now a new card in his discard deck
            Assert.AreEqual(1, _Player.Deck.Discard.Count);

            // -- Check that the card store before buy is in the discard deck
            Assert.AreEqual(cardBeforeTheBuy.ID, _Player.Deck.Discard[0].ID);

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // TODO: Test the undo command, when it will be implemented in the buy command.
        }*/

        #endregion
    }
}
