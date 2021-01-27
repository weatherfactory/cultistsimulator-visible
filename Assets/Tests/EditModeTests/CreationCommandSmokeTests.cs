using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Application.Commands.SituationCommands;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Interfaces;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.TestTools;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

[TestFixture]
public class CreationCommandsSmokeTests
{

    [SetUp]
    public void WithMinimalCompendiumLoad()
    {
        Watchman.ForgetEverything();

        var compendium = new Compendium();
        var cl = new CompendiumLoader("testcontent");
        var importLog = cl.PopulateCompendium(compendium, "en");
        foreach (var m in importLog.GetMessages())
            Debug.Log(m.Description);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<DeckSpec>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Dictum>().Count);
        Assert.AreEqual(5, compendium.GetEntitiesAsList<Element>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Ending>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Legacy>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Recipe>().Count);
        Assert.AreEqual(27, compendium.GetEntitiesAsList<Setting>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<BasicVerb>().Count);

        var watchman = new Watchman();
        watchman.Register(compendium);

        var worldSphere = Object.FindObjectOfType<TabletopSphere>();
        Watchman.Get<SphereCatalogue>().RegisterSphere(worldSphere);
        var enRouteSphere = Object.FindObjectOfType<EnRouteSphere>();
        Watchman.Get<SphereCatalogue>().RegisterSphere(worldSphere);

    }

    [Test]
    public void CreateElementStack()
    {
    string elementId= Watchman.Get<Compendium>().GetEntitiesAsList<Element>().First().Id;
        int elementQuantity= 3;
        var elementStackCreationCommand = new ElementStackCreationCommand(elementId, elementQuantity);
        var elementStack=elementStackCreationCommand.Execute(new Context(Context.ActionSource.Unknown));
        Assert.IsTrue(elementStack.IsValidElementStack());
        Assert.AreEqual(elementId,elementStack.Id);
        Assert.AreEqual(elementQuantity, elementStack.Quantity);
    }

    [Test]
        public void CreateElementStackToken()
        {
            var element = Watchman.Get<Compendium>().GetEntitiesAsList<Element>().First();
            var elementStackCreationCommand = new ElementStackCreationCommand(element.Id,1);
            var elementStack = elementStackCreationCommand.Execute(new Context(Context.ActionSource.Unknown));
        var location = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        var elementStackTokenCreationCommand = new TokenCreationCommand(elementStack, location, null);
       var elementStackToken=elementStackTokenCreationCommand.Execute(Watchman.Get<SphereCatalogue>());
       Assert.IsTrue(elementStackToken.Payload.IsValidElementStack());
    }

        [Test]
        public void CreateBasicVerbToken()
        {
            var basicVerb = Watchman.Get<Compendium>().GetEntitiesAsList<BasicVerb>().First();
        var location= new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        var basicVerbCreationCommand=new TokenCreationCommand(basicVerb,location,null);
        var basicVerbToken=basicVerbCreationCommand.Execute(Watchman.Get<SphereCatalogue>());
        Assert.IsTrue(basicVerbToken.Payload.IsValidVerb());

    }


    [Test]
    public void CreateDropzoneVerbToken()
    {
        
        var dropzoneVerb = new DropzoneVerb();
        
        var dropzoneLocation = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        var dropzoneCreationCommand = new TokenCreationCommand(dropzoneVerb, dropzoneLocation, null);
        var dropzone=dropzoneCreationCommand.Execute(Watchman.Get<SphereCatalogue>());
        Assert.IsInstanceOf<Token>(dropzone);

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