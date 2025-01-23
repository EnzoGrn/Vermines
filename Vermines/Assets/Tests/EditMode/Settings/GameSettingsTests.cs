using NUnit.Framework;
using UnityEngine;
using Vermines;

namespace Test.Vermines.Settings
{
    public class GameSettingsTests
    {
        private GameSettings gameSettings;

        [SetUp]
        public void SetUp()
        {
            // Create a new instance of GameSettings before each test
            gameSettings = ScriptableObject.CreateInstance<GameSettings>();
        }

        [Test]
        public void MaxPlayers_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(4, gameSettings.MaxPlayers.Value);
            Assert.AreEqual(2, gameSettings.MaxPlayers.MinValue);
            Assert.AreEqual(4, gameSettings.MaxPlayers.MaxValue);
            Assert.AreEqual("Player Settings", gameSettings.MaxPlayers.Category);
        }

        [Test]
        public void MinPlayers_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(2, gameSettings.MinPlayers.Value);
            Assert.AreEqual(2, gameSettings.MinPlayers.MinValue);
            Assert.AreEqual(4, gameSettings.MinPlayers.MaxValue);
            Assert.AreEqual("Player Settings", gameSettings.MinPlayers.Category);
        }

        [Test]
        public void IsRoundBased_ShouldBeInitializedCorrectly()
        {
            Assert.IsFalse(gameSettings.IsRoundBased.Value);
            Assert.AreEqual("Game Flow Settings", gameSettings.IsRoundBased.Category);
        }

        [Test]
        public void MaxRounds_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(15, gameSettings.MaxRounds.Value);
            Assert.AreEqual(1, gameSettings.MaxRounds.MinValue);
            Assert.AreEqual(100, gameSettings.MaxRounds.MaxValue);
            Assert.AreEqual("Game Flow Settings", gameSettings.MaxRounds.Category);
        }

        [Test]
        public void MaxTurnTime_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(120, gameSettings.MaxTurnTime.Value);
            Assert.AreEqual(60, gameSettings.MaxTurnTime.MinValue);
            Assert.AreEqual(300, gameSettings.MaxTurnTime.MaxValue);
            Assert.AreEqual("Game Flow Settings", gameSettings.MaxTurnTime.Category);
        }

        [Test]
        public void MaxCardsPerPlayerInHand_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(10, gameSettings.MaxCardsPerPlayerInHand.Value);
            Assert.AreEqual(5, gameSettings.MaxCardsPerPlayerInHand.MinValue);
            Assert.AreEqual(25, gameSettings.MaxCardsPerPlayerInHand.MaxValue);
            Assert.AreEqual("Hand Settings", gameSettings.MaxCardsPerPlayerInHand.Category);
        }

        [Test]
        public void MaxCardsToPlay_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(3, gameSettings.MaxCardsToPlay.Value);
            Assert.AreEqual(1, gameSettings.MaxCardsToPlay.MinValue);
            Assert.AreEqual(3, gameSettings.MaxCardsToPlay.MaxValue);
            Assert.AreEqual("Cards Settings", gameSettings.MaxCardsToPlay.Category);
        }

        [Test]
        public void MaxSoulsToWin_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(100, gameSettings.MaxSoulsToWin.Value);
            Assert.AreEqual(1, gameSettings.MaxSoulsToWin.MinValue);
            Assert.AreEqual(100, gameSettings.MaxSoulsToWin.MaxValue);
            Assert.AreEqual("Game Rules Settings", gameSettings.MaxSoulsToWin.Category);
        }

        [Test]
        public void DebugMode_ShouldBeInitializedCorrectly()
        {
            Assert.IsFalse(gameSettings.DebugMode.Value);
            Assert.AreEqual("Advanced Settings", gameSettings.DebugMode.Category);
        }

        [Test]
        public void RandomSeed_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(0, gameSettings.RandomSeed.Value);
            Assert.AreEqual(0, gameSettings.RandomSeed.MinValue);
            Assert.AreEqual(1, gameSettings.RandomSeed.MaxValue);
            Assert.AreEqual("Advanced Settings", gameSettings.RandomSeed.Category);
        }
    }
}

