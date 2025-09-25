using System.Collections.Generic;
using Fusion;
using NUnit.Framework;
using OMGG.DesignPattern;
using UnityEngine;
using Vermines;

#region Vermines namespace
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Utilities;
    using Vermines.Configuration;
using Vermines.Gameplay.Commands;
using Vermines.Gameplay.Commands.Deck;
    using Vermines.Player;
#endregion

namespace Test.Vermines.Gameplay.Deck {

    public class TestDeckCommand {

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

            localDeck.Deck.Add(CardSetDatabase.Instance.GetCardByID(72));
            localDeck.Deck.Add(CardSetDatabase.Instance.GetCardByID(73));
            localDeck.PlayedCards.Add(CardSetDatabase.Instance.GetCardByID(74));
            localDeck.PlayedCards.Add(CardSetDatabase.Instance.GetCardByID(75));
            localDeck.Hand.Add(CardSetDatabase.Instance.GetCardByID(76));
            localDeck.Hand.Add(CardSetDatabase.Instance.GetCardByID(77));

            GameDataStorage.Instance.PlayerDeck = _Decks;
        }

        #endregion

        [Test]
        public void SacrifiedCard()
        {
            // -- Normal sacrifice
            ICommand sacrificeCommand = new CLIENT_CardSacrifiedCommand(_LocalPlayer, 75);

            CommandInvoker.ExecuteCommand(sacrificeCommand);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);

            // -- Undo
            CommandInvoker.UndoCommand();

            // TODO: Implement and test the undo function.
        }

        [Test]
        public void PlayedCard()
        {
            // -- Normal played
            ICommand playedCommand = new CLIENT_PlayCommand(_LocalPlayer, 77);

            CommandInvoker.ExecuteCommand(playedCommand);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);

            // -- Undo
            CommandInvoker.UndoCommand();

            // TODO: Implement and test the undo function.
        }

        [Test]
        public void DiscardCard()
        {
            // -- Normal discard
            ICommand discardCommand = new CLIENT_DiscardCommand(_LocalPlayer, 77);

            CommandInvoker.ExecuteCommand(discardCommand);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);

            // -- Undo
            CommandInvoker.UndoCommand();

            // TODO: Implement and test the undo function.
        }

        [Test]
        public void DrawCard()
        {
            // -- Unknow player
            ICommand UPdrawCommand = new DrawCommand(PlayerRef.FromEncoded(4));

            CommandInvoker.ExecuteCommand(UPdrawCommand);

            Assert.AreEqual(CommandStatus.Invalid, CommandInvoker.State.Status);
            Assert.AreEqual("Player [Player:3] does not have a deck.", CommandInvoker.State.Message);

            // -- Normal draw
            ICommand drawCommand = new DrawCommand(_LocalPlayer);

            CommandInvoker.ExecuteCommand(drawCommand);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);
            Assert.AreEqual("Player [Player:0] drew a card.", CommandInvoker.State.Message);

            // -- Undo
            CommandInvoker.UndoCommand();

            Assert.AreEqual(_Decks[_LocalPlayer].Hand.Count, 2);

            // -- Draw with an empty deck
            ICommand emptyDeckDrawCommand = new DrawCommand(PlayerRef.FromEncoded(0x02));

            CommandInvoker.ExecuteCommand(emptyDeckDrawCommand);

            Assert.AreEqual(CommandStatus.Failure, CommandInvoker.State.Status);
            Assert.AreEqual("Player [Player:1] does not have any card left in his deck.", CommandInvoker.State.Message);
        }
    }
}
