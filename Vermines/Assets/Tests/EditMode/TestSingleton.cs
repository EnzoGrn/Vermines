using NUnit.Framework;
using OMGG.DesignPattern;
using System.Diagnostics;
using System.Reflection;

namespace Test.Pattern {

    class TestManager : Singleton<TestManager> {

        public int number = 0;

        public void OnCall()
        {
            number++;
        }
    }

    public class TestNonMonoBehaviourSingleton {

        [Test]
        public void CheckSingletonInit()
        {
            Assert.IsNotNull(TestManager.Instance);
        }

        [Test]
        public void TryCallOnCall()
        {
            TestManager.Instance.OnCall();

            Assert.AreEqual(1, TestManager.Instance.number);
        }

        [Test]
        public void Admin_TrySingletonSetter()
        {
            // Why here the number is not 2 ? It's because the Unity Test Runner, delete the old test for each new test.
            TryCallOnCall();

            // Get the Singleton class
            var singletonType = typeof(Singleton<TestManager>);

            // Set the Instance property to public
            var setMethod = singletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetSetMethod(true);

            // Check if the method exists
            Assert.IsNotNull(setMethod);

            // Call the setter with an instance of Test2Manager
            setMethod.Invoke(null, new object[] {
                new TestManager()
            });

            // Get the _Instance field and check if it contains an instance of Test2Manager
            var instanceField = singletonType.GetField("_Instance", BindingFlags.NonPublic | BindingFlags.Static);
            var instance      = instanceField.GetValue(null);

            Assert.IsInstanceOf<TestManager>(instance);

            // Check if the class got reset to his default value
            Assert.AreEqual(0, TestManager.Instance.number);
        }
    }
}
