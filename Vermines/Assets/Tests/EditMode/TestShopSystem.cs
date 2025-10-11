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
#endregion

using Vermines.Configuration;
using Vermines.Player;
using Vermines;
using Vermines.ShopSystem;
using Vermines.Gameplay.Commands;

namespace Test.Vermines.ShopSystem {

    public class TestShopSystem {

        private GameConfiguration _Config;

        private PlayerRef _LocalPlayer;

        private Dictionary<PlayerRef, PlayerDeck> _Decks;

        int Seed => 0x015;

        #region Setup

        [SetUp]
        public void Setup()
        {
            // -- Initialize a default game configuration
            _Config = ScriptableObject.CreateInstance<GameConfiguration>();

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

        private ShopData InitializeShop(GameConfiguration config)
        {
            List<ICard> everyBuyableCard = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == false);

            if (everyBuyableCard == null || everyBuyableCard.Count == 0)
                return null;
            List<ICard> objectCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Tools).ToList();

            if (objectCards == null || objectCards.Count == 0)
                return null;
            objectCards.Shuffle(Seed);

            List<ICard> partisanCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Partisan).ToList();

            if (partisanCards == null || partisanCards.Count == 0)
                return null;

            List<ICard> partisan1Cards = partisanCards.Where(card => card.Data.Level == 1).ToList();
            List<ICard> partisan2Cards = partisanCards.Where(card => card.Data.Level == 2).ToList();

            if (partisan1Cards == null || partisan2Cards == null || partisan1Cards.Count == 0 || partisan2Cards.Count == 0)
                return null;
            partisan1Cards.Shuffle(Seed);
            partisan2Cards.Shuffle(Seed);

            partisan1Cards.Merge(partisan2Cards);

            partisanCards = partisan1Cards;

            return ShopBuilder(config, partisanCards, objectCards);
        }

        private ShopData ShopBuilder(GameConfiguration config, List<ICard> partisans, List<ICard> objects)
        {
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            shop.Initialize(ShopType.Market);
            shop.FillShop(ShopType.Market, objects);

            shop.Initialize(ShopType.Courtyard);
            shop.FillShop(ShopType.Courtyard, partisans);

            return shop;
        }

        private ShopData InitializeAndFillShop(GameConfiguration config)
        {
            ShopData shop = InitializeShop(config);

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
            ShopData shop1 = InitializeShop(_Config);

            // Serialize the shop (first time)
            string data1 = shop1.Serialize();

            // -- Synchronise the shop2 with data of shop1
            ShopData shop2 = ShopBuilder(_Config, new List<ICard>(), new List<ICard>());

            ICommand syncCommand = new SyncShopCommand(shop2, data1);

            CommandInvoker.ExecuteCommand(syncCommand);

            // Serialize the shop (second time)
            string data2 = shop2.Serialize();

            // Compare the two serialized data
            Assert.AreEqual(data1, data2);

            // Undo the data synchronization
            CommandInvoker.UndoCommand();

            // Serialize the shop (third time)
            data2 = shop2.Serialize();

            // Compare the two serialized data
            Assert.AreNotEqual(data1, data2);
        }

        /// <summary>
        /// This test will check if the shop system is correctly initialized, and if the fill command is correctly executed.
        /// </summary>
        [Test]
        public void FillShopCommand()
        {
            // -- Invalid parameters
            ICommand fillCommand = new FillShopCommand(null);

            CommandInvoker.ExecuteCommand(fillCommand);

            Assert.AreEqual(CommandStatus.Invalid, CommandInvoker.State.Status);
            Assert.AreEqual("Failed to fill the shop.", CommandInvoker.State.Message);

            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop(_Config);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);
            Assert.AreEqual("Shop filled.", CommandInvoker.State.Message);

            // -- Fill again, when it's full
            FillCommand(shop);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);
            Assert.AreEqual("Shop filled.", CommandInvoker.State.Message);

            // -- Undo all the fill command
            CommandInvoker.UndoCommand();
            CommandInvoker.UndoCommand();

            // -- Check if the shop is empty
            foreach (var shopSection in shop.Sections) {
                foreach (var slot in shopSection.Value.AvailableCards) {
                    if (slot.Value != null)
                        Assert.Fail($"The slot {slot.Key} in the {shopSection.Key} should be empty.");
                }
            }

            // -- Fill a shop with an empty deck & discard deck
            ShopData emptyShop = ShopBuilder(_Config, new List<ICard>(), new List<ICard>());

            FillCommand(emptyShop);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);
            Assert.AreEqual("Shop filled.", CommandInvoker.State.Message);
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
            ShopData shop = InitializeAndFillShop(_Config);

            // -- Store the card before the change
            ICard cardBeforeTheChange = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            // -- Change a card in the 'Courtyard' at the place '0'
            ICommand changeCardCommand = new CLIENT_ChangeCardCommand(new ShopArgs(shop, ShopType.Courtyard, 0));

            CommandInvoker.ExecuteCommand(changeCardCommand);

            // -- Check if the card is correctly changed
            ICard cardAfterTheChange = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            Assert.AreNotEqual(cardBeforeTheChange.ID, cardAfterTheChange.ID, "The card should have been replaced by a new one.");

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // -- Check that the original card is back in the shop
            ICard cardAfterUndo = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            Assert.AreEqual(cardBeforeTheChange.ID, cardAfterUndo.ID, "The original card should be back in the shop.");

            // -- Verify discard and deck integrity
            ShopSection courtyard = shop.Sections[ShopType.Courtyard];

            Assert.IsFalse(courtyard.Deck.Contains(cardAfterUndo), "The original card should not remain in the deck after undo.");
            Assert.IsTrue(courtyard.Deck.Contains(cardAfterTheChange), "The new card should be in the deck pile after undo.");
        }

        /// <summary>
        /// If there is no more card in the shop, the card that is change will return into his same slot.
        /// Because we first put it in discard, if deck is empty, we merge discard and we put the card into the shop.
        /// </summary>
        [Test]
        public void ChangeCardInEmptyShop()
        {
            // -- Initialize an empty shop
            ShopData shop = ShopBuilder(_Config, new List<ICard>(), new List<ICard>());

            // -- Add a card to the slot 0 of the courtyard
            ICard card = CardSetDatabase.Instance.GetEveryCardWith(c => c.Data.Type == CardType.Partisan).FirstOrDefault();

            shop.Sections[ShopType.Courtyard].AvailableCards[0] = card;

            // -- Change a card in the 'Courtyard' at the place '0'
            ICommand changeCardCommand = new CLIENT_ChangeCardCommand(new ShopArgs(shop, ShopType.Courtyard, 0));

            CommandInvoker.ExecuteCommand(changeCardCommand);

            // -- Check if the card is correctly changed
            Assert.IsTrue(CommandInvoker.State.Status == CommandStatus.Success);
            Assert.IsTrue(card.ID == shop.Sections[ShopType.Courtyard].AvailableCards[0].ID);
        }

        #endregion

        #region Buy Command

        // TODO: Try to test admin side buy command.

        [Test]
        public void ClientBuyPartisanInShop()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop(_Config);

            // -- Store the card before the buy
            ICard cardBeforeTheBuy = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            // -- Buy a card in the 'Courtyard' at the place '0'
            ShopArgs parameters = new(shop, ShopType.Courtyard, 0);
            ICommand buyCommand = new CLIENT_BuyCommand(_LocalPlayer, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            // -- Check if the card is correctly bought
            ICard cardAfterTheBuy = shop.Sections[ShopType.Courtyard].AvailableCards[0];

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
        public void ClientBuyEquipmentInShop()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop(_Config);

            // -- Force put a equipment card in the first slot of the market
            ICard equipment = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.Type == CardType.Equipment).FirstOrDefault();

            shop.Sections[ShopType.Market].AvailableCards[0] = equipment;

            // -- Buy a card in the 'Market' at the place '0'
            ShopArgs parameters = new(shop, ShopType.Market, 0);
            ICommand buyCommand = new CLIENT_BuyCommand(_LocalPlayer, parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            // -- Check if the card is correctly bought
            ICard cardAfterTheBuy = shop.Sections[ShopType.Market].AvailableCards[0];

            // -- Check that the card after is a null card (because the shop didn't be refilled)
            Assert.IsNull(cardAfterTheBuy);

            // -- Check that the player have now a new card in his equipment area
            Assert.AreEqual(1, GameDataStorage.Instance.PlayerDeck[_LocalPlayer].Equipments.Count);

            // -- Check that the card store before buy is in the discard deck
            Assert.AreEqual(equipment.ID, GameDataStorage.Instance.PlayerDeck[_LocalPlayer].Equipments[0].ID);

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // TODO: Test the undo command, when it will be implemented in the buy command.
        }

        #endregion
    }
}
