using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestExample
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestExampleSimplePasses()
    {
        // Use the Assert class to test conditions
        int i = 5;

        Assert.AreEqual(5, i);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestExampleWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
