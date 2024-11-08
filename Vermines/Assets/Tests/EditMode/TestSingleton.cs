using NUnit.Framework;
using OMGG.DesignPattern;

namespace Test {

    public class TestManager : MonoBehaviourSingleton<TestManager> {

        public void OnCall()
        {
            Assert.Pass("OnCall is called.");
        }
    }

    public class TestSingleton {

        [Test]
        public void TestSingletonSimplePasses()
        {
            Assert.IsNotNull(TestManager.Instance);

            TestManager.Instance.OnCall();
        }
    }
}
