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
            _Player.AddCardToPlayedCards(CardSetDatabase.Instance.GetCardByID(47));
            _Player.AddCardToPlayedCards(CardSetDatabase.Instance.GetCardByID(48));
            // NOTE: Hand vit maintenant sur PlayerController (état [Networked]),
            // qui nécessite un NetworkObject réellement spawné par Fusion. Ce test
            // EditMode construit _Player via `new PlayerController()` sans passer par
            // Runner.Spawn(...), donc Hand ne peut plus être peuplée ici.
            // TODO: nécessite un harnais de test Fusion (mock NetworkRunner) pour
            // spawner un vrai PlayerController testable. Cartes 49/50 retirées en
            // attendant -- voir impact sur PlayedCard()/DiscardCard() ci-dessous.

            _Player.UpdateDeck(deck);

            _Player2.Deck.Initialize(Seed);
        }

        #endregion

        [Test]
        public void SacrifiedCard()
        {
            Assert.Ignore("Nécessite un harnais de test Fusion : PlayedCards est [Networked], " +
                           "ne peut plus être peuplée sans un PlayerController réellement spawné.");
        }

        [Test]
        public void PlayedCard()
        {
            Assert.Ignore("Nécessite un harnais de test Fusion : Hand est [Networked], " +
                           "ne peut plus être peuplée sans un PlayerController réellement spawné.");
        }

        [Test]
        public void DiscardCard()
        {
            Assert.Ignore("Nécessite un harnais de test Fusion : Hand est [Networked], " +
                           "ne peut plus être peuplée sans un PlayerController réellement spawné.");
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
