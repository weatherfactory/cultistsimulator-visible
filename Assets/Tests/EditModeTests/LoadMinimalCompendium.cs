using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using SecretHistories.Entities;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;


public class FucineLoadingTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void LoadMinimalCompendium()
    {
        var compendium=new Compendium();
        var cl = new CompendiumLoader("testcontent");
        var importLog= cl.PopulateCompendium(compendium, "en");
        foreach(var m in importLog.GetMessages())
            Debug.Log(m.Description);
        Assert.AreEqual(1,compendium.GetEntitiesAsList<DeckSpec>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Dictum>().Count);
        Assert.AreEqual(5, compendium.GetEntitiesAsList<Element>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Ending>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Legacy>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Recipe>().Count);
        Assert.AreEqual(27, compendium.GetEntitiesAsList<Setting>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<BasicVerb>().Count);



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
