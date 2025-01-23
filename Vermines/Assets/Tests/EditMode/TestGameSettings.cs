using NUnit.Framework;
using UnityEngine;


namespace Tests
{
    public class TestGameSettings
    {
        private Vermines.GameSettings _gameSettings;

        [SetUp]
        public void SetUp()
        {
            // Create an instance of the ScriptableObject
            _gameSettings = ScriptableObject.CreateInstance<GameSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup the ScriptableObject after each test
            if (_gameSettings != null)
            {
                ScriptableObject.DestroyImmediate(_gameSettings);
            }
        }

        [Test]
        public void MaxPlayers_InitialValue_IsCorrect()
        {
            // Assert that the initial value is set correctly
            Assert.AreEqual(4, _gameSettings.MaxPlayers.Value, "Initial MaxPlayers value should be 4.");
        }

        [Test]
        public void MaxPlayers_RestrictionCheck_ThrowsException_WhenInvalid()
        {
            // Set an invalid value
            _gameSettings.MaxPlayers.Value = 5;

            // Assert that RestrictionCheck throws an exception
            Assert.Throws<System.Exception>(() => _gameSettings.MaxPlayers.RestrictionCheck(),
                "RestrictionCheck should throw an exception when value is out of range.");
        }

        [Test]
        public void DebugMode_DefaultValue_IsFalse()
        {
            // Assert that the default value of DebugMode is false
            Assert.IsFalse(_gameSettings.DebugMode.Value, "DebugMode should default to false.");
        }

        [Test]
        public void RandomSeed_CanBeSetAndRetrieved()
        {
            // Change the random seed value
            _gameSettings.RandomSeed.Value = 123;

            // Assert that the value was updated correctly
            Assert.AreEqual(123, _gameSettings.RandomSeed.Value, "RandomSeed should be settable and retrievable.");
        }
    }
}
