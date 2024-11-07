using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using OMGG.Optimizer;

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

        [SetUp]
        public void Setup()
        {
            Scene newScene = SceneManager.CreateScene("Test.PlayMode.TestObservers");

            SceneManager.SetActiveScene(newScene);
        }

        [TearDown]
        public void Teardown()
        {
            var objects = GameObject.FindObjectsOfType<GameObject>();

            // Destroy properly the objects.
            foreach (var obj in objects)
                GameObject.Destroy(obj);

            // Unload the scene.
            SceneManager.UnloadSceneAsync("Test.PlayMode.TestObservers");
        }

        [UnityTest]
        public IEnumerator CheckIfObservedObjectAreCalled()
        {
            { // -- Check the well-done creation of the ObservedObject.
                GameObject testObj = new("TestObservedObject");

                Assert.IsNotNull(testObj);

                TestObservedObject testObjComponent = testObj.AddComponent<TestObservedObject>();

                Assert.IsNotNull(testObjComponent);
            }

            { // -- Check if MonoBehaviourSingleton GameObjects are created.
                Assert.IsNotNull(GameObject.Find("Auto-generated UpdateManager"));
                Assert.IsNotNull(GameObject.Find("Auto-generated FixedUpdateManager"));
                Assert.IsNotNull(GameObject.Find("Auto-generated LateUpdateManager"));
            }

            yield return RunTest<TestObservedObject>();
        }

        public IEnumerator RunTest<T>() where T : MonoBehaviour
        {
            var objects = new List<GameObject>();

            for (int i = 0; i < 100; i++)
                objects.Add(new GameObject(typeof(T).Name + " " + i, typeof(T)));
            yield return null;
        }
    }
}
