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

            // NOTE: Deck/Discard/Hand/PlayedCards/Graveyard/etc. sont désormais
            // [Networked] sur PlayerController -- nécessitent un NetworkObject
            // réellement spawné par Fusion (HasStateAuthority == false sur un
            // `new PlayerController()` nu, donc AddCardToX/InitializeDeck seraient
            // des no-op silencieux ici). Toute construction d'état de deck est donc
            // retirée de ce Setup ; les tests concernés restent Assert.Ignore en
            // attendant un harnais de test Fusion (mock NetworkRunner).
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
