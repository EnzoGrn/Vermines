using NUnit.Framework;

using Vermines.Config.Utils;

namespace Test.Vermines.Settings {

    public class IntSettingTests {

        private IntSetting _IntSetting;

        [SetUp]
        public void SetUp()
        {
            // Create a new instance of IntSetting for testing
            _IntSetting = new IntSetting("Test Int Setting", 10, 1, 20, "Test Category");
        }

        [Test]
        public void RestrictionCheck_ShouldThrowError_WhenValueIsBelowMinValue()
        {
            // Try to set the value to below the minimum (e.g., set Value to 0, while MinValue is 1)
            Assert.Throws<System.Exception>(() => {
                _IntSetting.RestrictionCheck(0);
            });
        }

        [Test]
        public void RestrictionCheck_ShouldThrowError_WhenValueIsAboveMaxValue()
        {
            // Try to set the value to above the maximum (e.g., set Value to 21, while MaxValue is 20)
            Assert.Throws<System.Exception>(() =>
            {
                _IntSetting.RestrictionCheck(21);
            });
        }

        [Test]
        public void RestrictionCheck_ShouldThrowError_WhenMinValueIsGreaterThanMaxValue()
        {
            // Create a new IntSetting with MinValue greater than MaxValue
            var invalidSetting = new IntSetting("Invalid Setting", 10, 20, 1, "Invalid Category");

            // Ensure that RestrictionCheck throws an exception when MinValue > MaxValue
            Assert.Throws<System.Exception>(() => {
                invalidSetting.RestrictionCheck(10);
            });
        }

        [Test]
        public void RestrictionCheck_ShouldNotThrowError_WhenValueIsWithinRange()
        {
            // Ensure that no exception is thrown when the value is within the valid range (e.g., 10 between 1 and 20)
            Assert.DoesNotThrow(() => {
                _IntSetting.RestrictionCheck(10);
            });
        }
    }
}
