using NUnit.Framework;

using Vermines.Settings;

namespace Test.Vermines.Settings
{
    public class BoolSettingsTests
    {
        private BoolSetting boolSetting;

        [SetUp]
        public void SetUp()
        {
            // Create a new instance of BoolSetting for testing
            boolSetting = new BoolSetting("Test Bool Setting", true, "Test Category");
        }

        [Test]
        public void RestrictionCheck_ShouldThrowError_WhenValueIsNotABoolean()
        {
            // Ensure that RestrictionCheck throws an exception when a non-bool value is passed
            Assert.Throws<System.Exception>(() =>
            {
                boolSetting.RestrictionCheck("not a bool"); // A string, should throw an exception
            });
            Assert.Throws<System.Exception>(() =>
            {
                boolSetting.RestrictionCheck(123); // An integer, should throw an exception
            });

            Assert.Throws<System.Exception>(() =>
            {
                boolSetting.RestrictionCheck(12.34f); // A float, should throw an exception
            });
        }

        [Test]
        public void RestrictionCheck_ShouldNotThrowError_WhenValueIsBoolean()
        {
            // Ensure that no exception is thrown when the value is a boolean (true)
            Assert.DoesNotThrow(() =>
            {
                boolSetting.RestrictionCheck(true); // True is valid
            });

            // Ensure that no exception is thrown when the value is a boolean (false)
            Assert.DoesNotThrow(() =>
            {
                boolSetting.RestrictionCheck(false); // False is valid
            });
        }
    }
}
