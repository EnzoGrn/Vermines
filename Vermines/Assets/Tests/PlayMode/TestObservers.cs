using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using OMGG.Optimizer;

namespace Test.Optimizer {

    public class TestObservedObject : MonoBehaviour, IUpdateObserver, IFixedUpdateObserver, ILateUpdateObserver {

        public int numberUpdate = 0;
        public int numberFixedUpdate = 0;
        public int numberLateUpdate = 0;

        public void Awake()
        {
            UpdateManager.Instance.RegisterObserver(this);
            FixedUpdateManager.Instance.RegisterObserver(this);
            LateUpdateManager.Instance.RegisterObserver(this);
        }

        public void Shutdown()
        {
            UpdateManager.Instance.UnregisterObserver(this);
            FixedUpdateManager.Instance.UnregisterObserver(this);
            LateUpdateManager.Instance.UnregisterObserver(this);
        }

        public void ObservedUpdate()
        {
            numberUpdate++;
        }

        public void ObservedFixedUpdate()
        {
            numberFixedUpdate++;
        }

        public void ObservedLateUpdate()
        {
            numberLateUpdate++;
        }
    }

    [TestFixture]
    public class TestUpdateOptimizerManager {

        private const string sceneName = "Test.PlayMode.TestObservers";
        private string[] singletonNames = {
            "Auto-generated UpdateManager",
            "Auto-generated FixedUpdateManager",
            "Auto-generated LateUpdateManager"
        };

        [SetUp]
        public void Setup()
        {
            Scene newScene = SceneManager.CreateScene(sceneName);

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
            SceneManager.UnloadSceneAsync(sceneName);
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
                for (int i = 0; i < singletonNames.Length; i++)
                    Assert.IsNotNull(GameObject.Find(singletonNames[i]));
            }

            yield return RunTest<TestObservedObject>();

            Assert.IsTrue(UpdateManager.Instance.HasObservers);
            Assert.IsTrue(FixedUpdateManager.Instance.HasObservers);
            Assert.IsTrue(LateUpdateManager.Instance.HasObservers);

            { // -- Shutdown the ObservedObject.
                GameObject testObj = GameObject.Find("TestObservedObject");

                Assert.IsNotNull(testObj);

                testObj.GetComponent<TestObservedObject>().Shutdown();

                UpdateManager.Instance.Update();
                FixedUpdateManager.Instance.FixedUpdate();
                LateUpdateManager.Instance.LateUpdate();

                Assert.Less(0, testObj.GetComponent<TestObservedObject>().numberUpdate);
                Assert.Less(0, testObj.GetComponent<TestObservedObject>().numberFixedUpdate);
                Assert.Less(0, testObj.GetComponent<TestObservedObject>().numberLateUpdate);
            }

            yield return null;
        }

        public IEnumerator RunTest<T>() where T : MonoBehaviour
        {
            var objects = new List<GameObject>();

            for (int i = 0; i < 100; i++) {
                GameObject obj = new(typeof(T).Name + " " + i, typeof(T));

                objects.Add(obj);
            }

            yield return null;
        }
    }
}
