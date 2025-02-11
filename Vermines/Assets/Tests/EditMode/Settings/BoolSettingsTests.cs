using NUnit.Framework;

using Vermines.Config.Utils;

namespace Test.Vermines.Settings {

    public class BoolSettingsTests {

        private BoolSetting _BoolSetting;

        [SetUp]
        public void SetUp()
        {
            // Create a new instance of BoolSetting for testing
            _BoolSetting = new BoolSetting("Test Bool Setting", true, "Test Category");
        }

        [Test]
        public void RestrictionCheck_ShouldNotThrowError_WhenValueIsBoolean()
        {
            // Ensure that no exception is thrown when the value is a boolean (true)
            Assert.DoesNotThrow(() => {
                _BoolSetting.RestrictionCheck(true); // True is valid
            });

            // Ensure that no exception is thrown when the value is a boolean (false)
            Assert.DoesNotThrow(() => {
                _BoolSetting.RestrictionCheck(false); // False is valid
            });
        }
    }
}
