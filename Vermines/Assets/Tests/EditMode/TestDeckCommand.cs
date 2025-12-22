using Fusion;
using NUnit.Framework;
using OMGG.DesignPattern;

#region Vermines namespace
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Utilities;
using Vermines.Configuration;
using Vermines.Core.Scene;
using Vermines.Gameplay.Commands;
using Vermines.Gameplay.Commands.Deck;
using Vermines.Player;
#endregion

namespace Test.Vermines.Gameplay.Deck
{

    public class TestDeckCommand
    {

        private PlayerRef _LocalPlayer;

        private PlayerController _Player;
        private PlayerController _Player2;

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
            _Player2 = new();

            _Player.UpdateDeck(new());

            PlayerDeck deck = new();

            deck.Initialize(Seed);

            deck.Deck.Add(CardSetDatabase.Instance.GetCardByID(45));
            deck.Deck.Add(CardSetDatabase.Instance.GetCardByID(46));
            deck.PlayedCards.Add(CardSetDatabase.Instance.GetCardByID(47));
            deck.PlayedCards.Add(CardSetDatabase.Instance.GetCardByID(48));
            deck.Hand.Add(CardSetDatabase.Instance.GetCardByID(49));
            deck.Hand.Add(CardSetDatabase.Instance.GetCardByID(50));

            _Player.UpdateDeck(deck);

            _Player2.Deck.Initialize(Seed);
        }

        #endregion

        [Test]
        public void SacrifiedCard()
        {
            // -- Normal sacrifice
            ICommand sacrificeCommand = new CLIENT_CardSacrifiedCommand(_Player, 75);

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
            ICommand playedCommand = new CLIENT_PlayCommand(_Player, 77);

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
            ICommand discardCommand = new CLIENT_DiscardCommand(_Player, 77);

            CommandInvoker.ExecuteCommand(discardCommand);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);

            // -- Undo
            CommandInvoker.UndoCommand();

            // TODO: Implement and test the undo function.
        }

        /*[Test]
        public void DrawCard()
        {
            // -- Normal draw
            ICommand drawCommand = new DrawCommand(_Player);

            CommandInvoker.ExecuteCommand(drawCommand);

            Assert.AreEqual(CommandStatus.Success, CommandInvoker.State.Status);

            // -- Undo
            CommandInvoker.UndoCommand();

            Assert.AreEqual(_Player.Deck.Hand.Count, 2);

            // -- Draw with an empty deck
            ICommand emptyDeckDrawCommand = new DrawCommand(_Player2);

            CommandInvoker.ExecuteCommand(emptyDeckDrawCommand);

            Assert.AreEqual(CommandStatus.Failure, CommandInvoker.State.Status);
        }*/
    }
}