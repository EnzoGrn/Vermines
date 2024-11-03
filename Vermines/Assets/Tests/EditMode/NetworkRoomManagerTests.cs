using NUnit.Framework;
using UnityEngine;
using System;
using Photon.Pun;
using UnityEngine.UIElements;

[TestFixture]
public class NetworkRoomManagerTests
{
    private NetworkSettings _networkSettings;

    [SetUp]
    public void Setup()
    {
        _networkSettings = Resources.Load<NetworkSettings>("Network/Settings/NetworkSettings");

        if (_networkSettings == null)
        {
            Debug.LogError("NetworkSettings not found in Resources folder !");
            return;
        }
    }

    [TearDown]
    public void Teardown()
    {
    }

    [Test]
    public void GenerateRandomCode_ReturnsStringOfGivenLength()
    {
        int length = 6;

        // Act
        string randomCode = NetworkUtils.GenerateRandomCode(_networkSettings.keyStringCodeGeneration, length, _networkSettings.random);

        // Assert
        Assert.AreEqual(length, randomCode.Length, "The generated code length is incorrect.");
    }

    [Test]
    public void GenerateRandomCode_ContainsOnlyAllowedCharacters()
    {
        int length = 25;

        // Act
        string randomCode = NetworkUtils.GenerateRandomCode(_networkSettings.keyStringCodeGeneration, length, _networkSettings.random);

        // Assert
        foreach (char c in randomCode)
        {
            Assert.Contains(c, _networkSettings.keyStringCodeGeneration.ToCharArray(), "Code contains invalid character.");
        }
    }
}
