using System.Collections;
using UnityEngine.TestTools;
using OMGG.DesignPattern;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Security.Cryptography;
using System.Security.Permissions;

namespace Test.OMGG.Pattern {

    class TestManager : MonoBehaviourSingleton<TestManager> {

        public int number = 0;

        public void OnCall()
        {
            number++;
        }
    }

    public class TestMonoBehaviourSingleton {

        private const string sceneName     = "Test.PlayMode.TestSingleton";
        private const string singletonName = "Auto-generated TestManager";

        [SetUp]
        public void Setup()
        {
            Scene newScene = SceneManager.CreateScene(sceneName);

            SceneManager.SetActiveScene(newScene);
        }

        [TearDown]
        public void Teardown()
        {
            var objects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            // Destroy properly the objects.
            foreach (var obj in objects)
                GameObject.Destroy(obj);

            // Unload the scene.
            SceneManager.UnloadSceneAsync(sceneName);
        }

        [UnityTest]
        public IEnumerator SimpleTest()
        {
            Assert.IsNotNull(TestManager.Instance);
            Assert.IsNotNull(GameObject.Find(singletonName));

            TestManager.Instance.OnCall();

            Assert.AreEqual(1, TestManager.Instance.number);

            // Get the Singleton class
            var singletonType = typeof(MonoBehaviourSingleton<TestManager>);

            // Set the Instance property to public
            var setMethod = singletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetSetMethod(true);

            // Check if the method exists
            Assert.IsNotNull(setMethod);

            GameObject.Destroy(GameObject.Find(singletonName));

            setMethod.Invoke(null, new object[] {
                null
            });

            Assert.IsNotNull(TestManager.Instance);

            // Get the _Instance field and check if it contains an instance of Test2Manager
            var instanceField = singletonType.GetField("_Instance", BindingFlags.NonPublic | BindingFlags.Static);
            var instance = instanceField.GetValue(null);

            Assert.IsInstanceOf<TestManager>(instance);

            // Check if the class got reset to his default value
            Assert.AreEqual(1, TestManager.Instance.number);

            yield return null;
        }
    }
}
