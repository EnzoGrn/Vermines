using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

using OMGG.DesignPattern;

#region Vermines ShopSystem namespace
    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Commands;
#endregion

#region Vermines CardSystem namespace
using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
#endregion

using Vermines.Config;
using Vermines.ShopSystem.Commands.Internal;

namespace Test.Vermines.ShopSystem {

    public class TestShopSystem {

        private GameConfig _Config;

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
            ShopData shop = InitializeShop(_Config);

            // -- Fill the shop
            FillCommand(shop);

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
            ShopData shop = InitializeShop(_Config);

            // -- Fill the shop
            FillCommand(shop);

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
    }
}
