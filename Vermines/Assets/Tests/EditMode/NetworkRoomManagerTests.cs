using NUnit.Framework;
using UnityEngine;
using System;
using Photon.Pun;

[TestFixture]
public class NetworkRoomManagerTests
{
    private NetworkRoomManager _networkRoomManager;

    [SetUp]
    public void Setup()
    {
        // Create a GameObject and add the MonoBehaviour (NetworkRoomManager) to it
        var gameObject = new GameObject();
        _networkRoomManager = gameObject.AddComponent<NetworkRoomManager>();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up the instantiated GameObject
        GameObject.DestroyImmediate(_networkRoomManager.gameObject);
    }

    [Test]
    public void GenerateRandomCode_ReturnsStringOfGivenLength()
    {
        // Arrange
        int length = 6;

        // Act
        string randomCode = _networkRoomManager.GenerateRandomCode(length);

        // Assert
        Assert.AreEqual(length, randomCode.Length, "The generated code length is incorrect.");
    }

    [Test]
    public void GenerateRandomCode_ContainsOnlyAllowedCharacters()
    {
        // Arrange
        int length = 10;
        string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Act
        string randomCode = _networkRoomManager.GenerateRandomCode(length);

        // Assert
        foreach (char c in randomCode)
        {
            Assert.Contains(c, allowedCharacters.ToCharArray(), "Code contains invalid character.");
        }
    }
}
