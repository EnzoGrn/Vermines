using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OMGG.Network;

namespace Test.OMGG.Network {

    public class NetworkRoomOptions {

        private RoomOptions _Options;

        [SetUp]
        public void Setup()
        {
            _Options = new RoomOptions();

            _Options.IsOpen = true;
            _Options.IsVisible = true;

            _Options.MaxPlayers = 4;
            _Options.MinPlayers = 2;

            _Options.CustomProperties = new();

            _Options.CustomProperties.Add("RoomName", "Room #1");
            _Options.CustomProperties.Add("Language", "fr");
            _Options.CustomProperties.Add("GameMode", "Deathmatch");
        }

        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            Assert.IsTrue(_Options.IsOpen);
            Assert.IsTrue(_Options.IsVisible);

            Assert.AreEqual(4, _Options.MaxPlayers);
            Assert.AreEqual(2, _Options.MinPlayers);

            Assert.AreEqual("Room #1"   , _Options.CustomProperties["RoomName"]);
            Assert.AreEqual("fr"        , _Options.CustomProperties["Language"]);
            Assert.AreEqual("Deathmatch", _Options.CustomProperties["GameMode"]);
        }
    }
}
