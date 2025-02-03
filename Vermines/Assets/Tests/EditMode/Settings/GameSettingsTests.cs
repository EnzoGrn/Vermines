using NUnit.Framework;
using UnityEngine;

using Vermines.Settings;

namespace Test.Vermines.Settings
{

    public class GameSettingsTests
    {
        private GameSettings _GameSettings;

        [SetUp]
        public void SetUp()
        {
            // Create a new instance of GameSettings before each test
            _GameSettings = ScriptableObject.CreateInstance<GameSettings>();
        }

        [Test]
        public void MaxPlayers_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(4, _GameSettings.MaxPlayers.Value);
            Assert.AreEqual(2, _GameSettings.MaxPlayers.MinValue);
            Assert.AreEqual(4, _GameSettings.MaxPlayers.MaxValue);
            Assert.AreEqual("Player Settings", _GameSettings.MaxPlayers.Category);
        }

        [Test]
        public void MinPlayers_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(2, _GameSettings.MinPlayers.Value);
            Assert.AreEqual(2, _GameSettings.MinPlayers.MinValue);
            Assert.AreEqual(4, _GameSettings.MinPlayers.MaxValue);
            Assert.AreEqual("Player Settings", _GameSettings.MinPlayers.Category);
        }

        [Test]
        public void IsRoundBased_ShouldBeInitializedCorrectly()
        {
            Assert.IsFalse(_GameSettings.IsRoundBased.Value);
            Assert.AreEqual("Game Flow Settings", _GameSettings.IsRoundBased.Category);
        }

        [Test]
        public void MaxRounds_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(15, _GameSettings.MaxRounds.Value);
            Assert.AreEqual(1, _GameSettings.MaxRounds.MinValue);
            Assert.AreEqual(100, _GameSettings.MaxRounds.MaxValue);
            Assert.AreEqual("Game Flow Settings", _GameSettings.MaxRounds.Category);
        }

        [Test]
        public void MaxTurnTime_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(120, _GameSettings.MaxTurnTime.Value);
            Assert.AreEqual(60, _GameSettings.MaxTurnTime.MinValue);
            Assert.AreEqual(300, _GameSettings.MaxTurnTime.MaxValue);
            Assert.AreEqual("Game Flow Settings", _GameSettings.MaxTurnTime.Category);
        }

        [Test]
        public void MaxCardsPerPlayerInHand_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(10, _GameSettings.MaxCardsPerPlayerInHand.Value);
            Assert.AreEqual(5, _GameSettings.MaxCardsPerPlayerInHand.MinValue);
            Assert.AreEqual(25, _GameSettings.MaxCardsPerPlayerInHand.MaxValue);
            Assert.AreEqual("Hand Settings", _GameSettings.MaxCardsPerPlayerInHand.Category);
        }

        [Test]
        public void MaxCardsToPlay_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(3, _GameSettings.MaxCardsToPlay.Value);
            Assert.AreEqual(1, _GameSettings.MaxCardsToPlay.MinValue);
            Assert.AreEqual(3, _GameSettings.MaxCardsToPlay.MaxValue);
            Assert.AreEqual("Cards Settings", _GameSettings.MaxCardsToPlay.Category);
        }

        [Test]
        public void MaxSoulsToWin_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(100, _GameSettings.MaxSoulsToWin.Value);
            Assert.AreEqual(1, _GameSettings.MaxSoulsToWin.MinValue);
            Assert.AreEqual(100, _GameSettings.MaxSoulsToWin.MaxValue);
            Assert.AreEqual("Game Rules Settings", _GameSettings.MaxSoulsToWin.Category);
        }

        [Test]
        public void DebugMode_ShouldBeInitializedCorrectly()
        {
            Assert.IsFalse(_GameSettings.DebugMode.Value);
            Assert.AreEqual("Advanced Settings", _GameSettings.DebugMode.Category);
        }

        [Test]
        public void RandomSeed_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual(0, _GameSettings.RandomSeed.Value);
            Assert.AreEqual(0, _GameSettings.RandomSeed.MinValue);
            Assert.AreEqual(1, _GameSettings.RandomSeed.MaxValue);
            Assert.AreEqual("Advanced Settings", _GameSettings.RandomSeed.Category);
        }
    }
}

