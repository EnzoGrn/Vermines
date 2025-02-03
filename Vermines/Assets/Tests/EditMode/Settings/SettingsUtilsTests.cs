using System.Collections.Generic;
using NUnit.Framework;
using Vermines;
using UnityEngine;
using UnityEngine.TestTools;
using Vermines.Settings;

namespace Test.Vermines.Settings
{

    public class SettingsUtilsTests
    {
        private GameSettings _GameSettings;

        [SetUp]
        public void SetUp()
        {
            // Create a new GameSettings instance for testing
            _GameSettings = ScriptableObject.CreateInstance<GameSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_GameSettings);
        }

        private List<string> GetDifferentCategorieName()
        {
            List<string> categories = new List<string>();

            // Loop over fields of gameSettings
            foreach (var field in _GameSettings.GetType().GetFields())
            {
                // Check if the field is an ASetting
                try
                {
                    ASetting value = field.GetValue(_GameSettings) as ASetting;

                    // Add the category to the list if it is not already in it
                    if (!categories.Contains(value.Category))
                    {
                        categories.Add((value.Category));
                    }
                }
                catch
                {
                    continue; 
                }
            }
            return categories;
        }

        [Test]
        public void GetSettingsByCategory_CategorizesSettingsCorrectly()
        {
            var settingsByCategory = SettingsUtils.GetSettingsByCategory(_GameSettings);

            List<string> categories = GetDifferentCategorieName();

            foreach (var category in categories)
            {
                Assert.IsTrue(settingsByCategory.ContainsKey(category), $"{category} should be a category.");
                Assert.GreaterOrEqual(settingsByCategory[category].Count, 1, $"{category} should have at least one setting.");
            }
        }

        [Test]
        public void GetSettingsByCategory_HandlesInvalidFieldTypesGracefully()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var settingsByCategory = SettingsUtils.GetSettingsByCategory(_GameSettings);
                // Assert that there is no Debug.LogWarning message
                LogAssert.NoUnexpectedReceived();
            }, "The method should not throw an exception for non-ASetting fields.");
        }
    }
}
