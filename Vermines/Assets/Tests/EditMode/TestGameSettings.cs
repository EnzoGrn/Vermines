using NUnit.Framework;
using UnityEngine;
using Vermines.Settings;

namespace Test.Vermines.Settings
{
    public class TestGameSettings
    {
        private GameSettings _GameSettings;

        [SetUp]
        public void SetUp()
        {
            // Create an instance of the ScriptableObject
            _GameSettings = ScriptableObject.CreateInstance<GameSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup the ScriptableObject after each test
            if (_GameSettings != null)
            {
                ScriptableObject.DestroyImmediate(_GameSettings);
            }
        }

        [Test]
        public void MaxPlayers_InitialValue_IsCorrect()
        {
            // Assert that the initial value is set correctly
            Assert.AreEqual(4, _GameSettings.MaxPlayers.Value, "Initial MaxPlayers value should be 4.");
        }

        [Test]
        public void DebugMode_DefaultValue_IsFalse()
        {
            // Assert that the default value of DebugMode is false
            Assert.IsFalse(_GameSettings.DebugMode.Value, "DebugMode should default to false.");
        }

        [Test]
        public void RandomSeed_CanBeSetAndRetrieved()
        {
            // Change the random seed value
            _GameSettings.MaxSoulsToWin.Value = 65;

            // Assert that the value was updated correctly
            Assert.AreEqual(65, _GameSettings.MaxSoulsToWin.Value, "RandomSeed should be settable and retrievable.");
        }
    }
}
