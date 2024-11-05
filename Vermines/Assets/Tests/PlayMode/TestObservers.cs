using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using System.Diagnostics;
using System.Collections.Generic;

namespace Tests {

    public class TestObservedObject : MonoBehaviour, IUpdateObserver, IFixedUpdateObserver, ILateUpdateObserver {

        public void Awake()
        {
            UpdateManager.Instance.RegisterObserver(this);
            FixedUpdateManager.Instance.RegisterObserver(this);
            LateUpdateManager.Instance.RegisterObserver(this);
        }

        public void ObservedUpdate()
        {
            Assert.Pass("ObservedUpdate is called.");
        }

        public void ObservedFixedUpdate()
        {
            Assert.Pass("ObservedFixedUpdate is called.");
        }

        public void ObservedLateUpdate()
        {
            Assert.Pass("ObservedLateUpdate is called.");
        }
    }

    [TestFixture]
    public class TestObserver {

        private const string _SceneName = "Test - Update Manager";

        /*
         * @brief Load a test scene before running tests
         * Wait the scene is totally loaded before running tests
         */
        [SetUp]
        public void Setup()
        {
            SceneManager.LoadScene(_SceneName);
        }

        [UnityTest]
        public IEnumerator CheckIfObservedObjectAreCalled()
        {
            GameObject         testObj          = new("TestObservedObject");
            TestObservedObject testObjComponent = testObj.AddComponent<TestObservedObject>();

            Assert.IsNotNull(testObjComponent);

            yield return RunTest<TestObservedObject>();
        }

        public IEnumerator RunTest<T>() where T : MonoBehaviour
        {
            var objects = new List<GameObject>();

            for (int i = 0; i < 100; i++)
                objects.Add(new GameObject(typeof(T).Name + " " + i, typeof(T)));

            yield return null;

            foreach (var obj in objects)
                GameObject.Destroy(obj);
            yield return null;

        }
    }
}
