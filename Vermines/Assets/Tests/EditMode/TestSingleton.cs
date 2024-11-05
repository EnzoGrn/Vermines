using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Test {

    public class TestManager : Singleton<TestManager> {

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
