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

using Vermines.Config;
using Vermines.Player;

namespace Test.Vermines.ShopSystem {

    public class TestShopSystem {

        private GameConfig _Config;

        private PlayerRef _LocalPlayer;

        private Dictionary<PlayerRef, PlayerDeck> _Decks;

        #region Setup

        [SetUp]
        public void Setup()
        {
            // -- Initialize a default game configuration
            _Config = ScriptableObject.CreateInstance<GameConfig>();

            _Config.Seed = 0x015;
            _Config.DrawPerTurn = 3;
            _Config.EloquencePerTurn = 2;
            _Config.FirstDraw = 3;
            _Config.FirstEloquence = 0;
            _Config.MaxEloquence = 20;
            _Config.WinCondition = 100;
            _Config.NumerOfCardsProposed = 5;

            // -- Initialize a card data set for a two players game
            CardSetDatabase.Instance.Initialize(FamilyUtils.GenerateFamilies(_Config.Seed, 2));

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
        }

        #endregion

        #region Initialization

        private ShopData InitializeShop(GameConfig config)
        {
            List<ICard> everyBuyableCard = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == false);

            if (everyBuyableCard == null || everyBuyableCard.Count == 0)
                return null;
            List<ICard> objectCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Equipment || card.Data.Type == CardType.Tools).ToList();

            if (objectCards == null || objectCards.Count == 0)
                return null;
            objectCards.Shuffle(config.Seed);

            List<ICard> partisanCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Partisan).ToList();

            if (partisanCards == null || partisanCards.Count == 0)
                return null;

            List<ICard> partisan1Cards = partisanCards.Where(card => card.Data.Level == 1).ToList();
            List<ICard> partisan2Cards = partisanCards.Where(card => card.Data.Level == 2).ToList();

            if (partisan1Cards == null || partisan2Cards == null || partisan1Cards.Count == 0 || partisan2Cards.Count == 0)
                return null;
            partisan1Cards.Shuffle(config.Seed);
            partisan2Cards.Shuffle(config.Seed);

            partisan1Cards.Merge(partisan2Cards);

            partisanCards = partisan1Cards;

            return ShopBuilder(config, partisanCards, objectCards);
        }

        private ShopData ShopBuilder(GameConfig config, List<ICard> partisans, List<ICard> objects)
        {
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            shop.Initialize(config.NumerOfCardsProposed);
            shop.FillShop(ShopType.Market, objects);
            shop.FillShop(ShopType.Courtyard, partisans);

            return shop;
        }

        private ShopData InitializeAndFillShop(GameConfig config)
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

            ICommand syncCommand = new SyncShopCommand(shop2, data1, _Config);

            CommandInvoker.ExecuteCommand(syncCommand);

            // Serialize the shop (second time)
            string data2 = shop2.Serialize();

            // Compare the two serialized data
            Assert.AreEqual(data1, data2);
        }

        /// <summary>
        /// This test will check if the shop system is correctly initialized, and if the fill command is correctly executed.
        /// </summary>
        [Test]
        public void FillShopCommand()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop(_Config);

            // -- Check if the shop is correctly fill
            foreach (var shopSection in shop.Sections) {
                foreach (var slot in shopSection.Value.AvailableCards) {
                    if (slot.Value == null)
                        Assert.Fail($"The slot {slot.Key} in the {shopSection.Key} should be filled.");
                }
            }

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // -- Check if the shop is empty
            foreach (var shopSection in shop.Sections) {
                foreach (var slot in shopSection.Value.AvailableCards) {
                    if (slot.Value != null)
                        Assert.Fail($"The slot {slot.Key} in the {shopSection.Key} should be empty.");
                }
            }
        }

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
            ICommand changeCardCommand = new ChangeCardCommand(shop, ShopType.Courtyard, 0);

            CommandInvoker.ExecuteCommand(changeCardCommand);

            // -- Check if the card is correctly changed
            ICard cardAfterTheChange = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            Assert.AreNotEqual(cardBeforeTheChange.ID, cardAfterTheChange.ID);

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // -- Check if the card is correctly changed
            cardAfterTheChange = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            Assert.AreEqual(cardBeforeTheChange.ID, cardAfterTheChange.ID);
        }

        /// <summary>
        /// Local is because it's not the networking buy command, but the command execute locally in each client.
        /// </summary>
        [Test]
        public void BuyCardLocalInShop()
        {
            // -- Shop initialization with default settings.
            ShopData shop = InitializeAndFillShop(_Config);

            // -- Store the card before the buy
            ICard cardBeforeTheBuy = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            // -- Buy a card in the 'Courtyard' at the place '0'
            BuyParameters parameters = new() {
                Decks    = _Decks,
                Player   = _LocalPlayer,
                Shop     = shop,
                ShopType = ShopType.Courtyard,
                Slot     = 0 // Buy the first card available in the shop
            };

            ICommand buyCommand = new BuyCommand(parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            // -- Check if the card is correctly bought
            ICard cardAfterTheBuy = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            // -- Check that the card after is a null card (because the shop didn't be refilled)
            Assert.IsNull(cardAfterTheBuy);

            // -- Check that the player have now a new card in his discard deck
            Assert.AreEqual(1, _Decks[_LocalPlayer].Discard.Count);

            // -- Undo the command
            CommandInvoker.UndoCommand();

            // -- Check if the card is correctly bought
            cardAfterTheBuy = shop.Sections[ShopType.Courtyard].AvailableCards[0];

            // -- Check that the card before and after the buy are equals
            Assert.IsNotNull(cardAfterTheBuy);
            Assert.AreEqual(cardBeforeTheBuy.ID, cardAfterTheBuy.ID);

            // -- Check that the player have no more card in his discard deck
            Assert.AreEqual(0, _Decks[_LocalPlayer].Discard.Count);
        }
    }
}
