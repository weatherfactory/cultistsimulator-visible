using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class FucineLoadingTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void LoadMinimalCompendium()
    {
        var compendium=new Compendium();
        var cl = new CompendiumLoader("content");
        cl.PopulateCompendium(compendium, "en");
        Assert.AreEqual(1,1);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator CreateTokenWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
