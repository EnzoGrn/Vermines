using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using OMGG.Network.Fusion;
using OMGG.Network;
using System.Collections;
using System;
using System.Diagnostics;

namespace Test.Vermines.Controller {

    public class MenuControllerTests {

        private GameObject     _MenuControllerGameObject;
        private MenuController _MenuController;
        private TMP_Text       _StatusTextMock;

        [SetUp]
        public void SetUp()
        {
            // Create a MenuController GameObject, and assign it to the MenuController field
            _MenuControllerGameObject = new GameObject("MenuController");
            _MenuController = _MenuControllerGameObject.AddComponent<MenuController>();

            // Create a TextMeshPro GameObject, and assign it to the _StatusText field
            GameObject tmpObject = new GameObject("StatusText");

            _StatusTextMock = tmpObject.AddComponent<TextMeshProUGUI>();

            _MenuControllerGameObject.AddComponent<Canvas>();
            _MenuControllerGameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            // Link TMP_Text to MenuController
            var statusTextField = _MenuController.GetType().GetField("_StatusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            statusTextField.SetValue(_MenuController, _StatusTextMock);
        }

        [TearDown]
        public void TearDown()
        {
            _MenuController.ShowError("The controller are being destroyed!");
            _MenuController.OnDestroy();

            GameObject.Destroy(_MenuControllerGameObject);
            GameObject.Destroy(_StatusTextMock.gameObject);
        }

        [UnityTest]
        public IEnumerator TestNetworkManager()
        {
            while (_StatusTextMock.text != "Connected to the server.")
                yield return null;
            Assert.AreEqual("Connected to the server.", _StatusTextMock.text);

            var networkManagerField = _MenuController.GetType().GetField("_NetworkManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            INetworkManager networkManager = (INetworkManager)networkManagerField.GetValue(_MenuController);

            Assert.IsNotNull(networkManager);
            Assert.AreEqual(ConnectionState.Connected, networkManager.GetConnectionState());

            networkManager.Connect();

            yield return null;
        }
    }
}
