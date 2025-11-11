using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Fusion;

using OMGG.DesignPattern;

#region Vermines ShopSystem namespace

    using Vermines.ShopSystem.Commands.Internal;
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

#endregion

namespace Test.Vermines.ShopSystem {

    public class TestShopSystem {

        private PlayerRef _LocalPlayer;

        private Dictionary<PlayerRef, PlayerDeck> _Decks;

        int Seed => 0x015;

        #region Setup

        [SetUp]
        public void Setup()
        {
            // -- Initialize a card data set for a two players game
            CardSetDatabase.Instance.Initialize(FamilyUtils.GenerateFamilies(Seed, 2));

            // -- Player initialization
            _LocalPlayer = PlayerRef.FromEncoded(0x01);

            PlayerDeck localDeck = new PlayerDeck();

            localDeck.Initialize();

            PlayerRef playerTwo = PlayerRef.FromEncoded(0x02);
            PlayerDeck playerTwoDeck = new PlayerDeck();

            playerTwoDeck.Initialize();

            // -- Initialize the decks for two players game
            _Decks = new Dictionary<PlayerRef, PlayerDeck> {
                { _LocalPlayer, localDeck     },
                { playerTwo   , playerTwoDeck }
            };

            GameDataStorage.Instance.PlayerDeck = _Decks;

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
            List<ICard> partisanCards  = everyBuyableCard.Where(card => card.Data.Type == CardType.Partisan).ToList();
            List<ICard> partisan1Cards = partisanCards.Where(card => card.Data.Level == 1).ToList();
            List<ICard> partisan2Cards = partisanCards.Where(card => card.Data.Level == 2).ToList();

            partisan1Cards.Shuffle(Seed);
            partisan2Cards.Shuffle(Seed);

            // 3. Create the shop.
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            // 3.a. Courtyard Initialization.
            CourtyardSection courtyard = ScriptableObject.CreateInstance<CourtyardSection>();

            courtyard.Initialize();

            courtyard.Deck1 = partisan1Cards;
            courtyard.Deck2 = partisan2Cards;

            shop.AddSection(ShopType.Courtyard, courtyard);

            // 3.b. Market Initialization.
            var groupedByName = objectCards.GroupBy(c => c.Data.Name).ToDictionary(g => g.Key, g => g.ToList());

            MarketSection market = ScriptableObject.CreateInstance<MarketSection>();

            market.Initialize();

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
            CourtyardSection courtyard = ScriptableObject.CreateInstance<CourtyardSection>();

            courtyard.Initialize();

            shop.AddSection(ShopType.Courtyard, courtyard);

            // 3.b. Market Initialization.
            MarketSection market = ScriptableObject.CreateInstance<MarketSection>();

            shop.AddSection(ShopType.Market, market);

            return shop;
        }

        private ShopData InitializeAndFillShop()
        {
            ShopData shop = InitializeShop();

            FillCommand(shop);

            return shop;
        }

        #endregion

        #region Commands

        public void FillCommand(ShopData shop)
        {
            ICommand fillCommand = new FillShopCommand(shop);

            CommandInvoker.ExecuteCommand(fillCommand);
        }

        #endregion

        [Test]
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
        }

        /// <summary>
        /// This test will check if the shop system is correctly initialized, and if the fill command is correctly executed.
        /// </summary>
        [Test]
        public void FillShopCommand()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop();

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);

            // -- Fill again, when it's full
            FillCommand(shop);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);

            // -- Check if the shop is empty
            foreach (var shopSection in shop.Sections) {
                if (shopSection.Value is CourtyardSection courtyard) {
                    foreach (var slot in courtyard.AvailableCards) {
                        if (slot.Value == null)
                            Assert.Fail($"The slot {slot.Key} in {shopSection.Key} should be filled.");
                    }
                } else if (shopSection.Value is MarketSection market) {
                    foreach (var kvp in market.CardPiles) {
                        if (kvp.Value.Count == 0)
                            Assert.Fail($"The slot in {shopSection.Key} is empty, but should be filled.");
                    }
                } else {
                    Assert.Fail($"Type {shopSection.Key} refill tests not implemented.");
                }
            }

            // -- Fill a shop with an empty deck & discard deck
            ShopData emptyShop = InitializeEmptyShop();

            FillCommand(emptyShop);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);
        }

        #region Change Command

        // TODO: Try to test admin side change command.

        /// <summary>
        /// Change card represent the 'Royale Missive' / 'Squire' action in the game.
        /// </summary>
        [Test]
        public void ChangeCardInShop()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop();

            CourtyardSection courtyard = (CourtyardSection)shop.Sections[ShopType.Courtyard];

            // -- Store the card before the change
            ICard cardBeforeTheChange = courtyard.AvailableCards[0];

            // -- Change the cardBeforeTheChange card in the 'Courtyard'
            ICommand changeCardCommand = new CLIENT_ChangeCardCommand(new ShopArgs(shop, ShopType.Courtyard, cardBeforeTheChange.ID));

            CommandInvoker.ExecuteCommand(changeCardCommand);

            // -- Check if the card is correctly changed
            ICard cardAfterTheChange = courtyard.AvailableCards[0];

            Assert.AreNotEqual(cardBeforeTheChange.ID, cardAfterTheChange.ID, "The card should have been replaced by a new one.");

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // TODO: Undo command.
        }

        /// <summary>
        /// If there is no more card in the shop, the card that is change will return into his same slot.
        /// Because we first put it in discard, if deck is empty, we merge discard and we put the card into the shop.
        /// </summary>
        [Test]
        public void ChangeCardInEmptyShop()
        {
            // -- Initialize an empty shop
            ShopData shop = InitializeEmptyShop();

            // -- Add a card to the slot 0 of the courtyard
            ICard card = CardSetDatabase.Instance.GetEveryCardWith(c => c.Data.Type == CardType.Partisan).FirstOrDefault();

            CourtyardSection courtyard = (CourtyardSection)shop.Sections[ShopType.Courtyard];

            courtyard.AvailableCards[0] = card;

            // -- Change the card in the 'Courtyard' 
            ICommand changeCardCommand = new CLIENT_ChangeCardCommand(new ShopArgs(shop, ShopType.Courtyard, card.ID));

            CommandInvoker.ExecuteCommand(changeCardCommand);

            // -- Check if the card is correctly changed
            Assert.IsTrue(CommandInvoker.State.Status == CommandStatus.Success);
            Assert.IsTrue(card.ID == courtyard.AvailableCards[0].ID); // When the shop is empty, the card changed stay the same as before.
        }

        #endregion

        #region Buy Command

        // TODO: Try to test admin side buy command.

        [Test]
        public void ClientBuyPartisanInShop()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop();

            // -- Store the card before the buy
            CourtyardSection courtyard = (CourtyardSection)shop.Sections[ShopType.Courtyard];

            ICard cardBeforeTheBuy = courtyard.AvailableCards[0];

            // -- Buy a card in the 'Courtyard' the cardBeforeTheBuy
            ShopArgs parameters = new(shop, ShopType.Courtyard, cardBeforeTheBuy.ID);

            ICommand buyCommand = new CLIENT_BuyCommand(_LocalPlayer, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            // -- Check if the card is correctly bought
            ICard cardAfterTheBuy = courtyard.AvailableCards[0];

            // -- Check that the card after is a null card (because the shop didn't be refilled)
            Assert.IsNull(cardAfterTheBuy);

            // -- Check that the player have now a new card in his discard deck
            Assert.AreEqual(1, GameDataStorage.Instance.PlayerDeck[_LocalPlayer].Discard.Count);

            // -- Check that the card store before buy is in the discard deck
            Assert.AreEqual(cardBeforeTheBuy.ID, GameDataStorage.Instance.PlayerDeck[_LocalPlayer].Discard[0].ID);

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

            ICommand buyCommand = new CLIENT_BuyCommand(_LocalPlayer, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            // -- Check if the card is correctly bought
            ICard cardAfterTheBuy = market.CardPiles[0][^1];

            // -- Check that the card after is a new exemplars (because the market auto refilled).
            Assert.IsFalse(cardBeforeTheBuy.ID == cardAfterTheBuy.ID);
            Assert.IsTrue(cardBeforeTheBuy.Data.Name == cardAfterTheBuy.Data.Name);

            // -- Check that the player have now a new card in his discard deck
            Assert.AreEqual(1, GameDataStorage.Instance.PlayerDeck[_LocalPlayer].Discard.Count);

            // -- Check that the card store before buy is in the discard deck
            Assert.AreEqual(cardBeforeTheBuy.ID, GameDataStorage.Instance.PlayerDeck[_LocalPlayer].Discard[0].ID);

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // TODO: Test the undo command, when it will be implemented in the buy command.
        }

        #endregion
    }
}
