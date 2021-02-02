using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Application.Commands;
using Assets.Scripts.Application.Commands.SituationCommands;
using Assets.Scripts.Application.Entities;
using NUnit.Framework;
using SecretHistories.Abstract;
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
        var location = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        var elementStackTokenCreationCommand = new TokenCreationCommand(elementStackCreationCommand, location);
       var elementStackToken=elementStackTokenCreationCommand.Execute(new Context(Context.ActionSource.Debug));
        Assert.IsTrue(elementStackToken.Payload.IsValidElementStack());
    }

        [Test]
        public void CreateSituationToken()
        {
   throw new NotImplementedException();
        }


    [Test]
    public void CreateDropzoneToken()
    {
        
        var dropzonePayloadCreationCommand=new DropzoneCreationCommand();
        
        var dropzoneLocation = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        var dropzoneCreationCommand = new TokenCreationCommand(dropzonePayloadCreationCommand, dropzoneLocation);
        var dropzone=dropzoneCreationCommand.Execute(new Context(Context.ActionSource.Debug));
        Assert.IsInstanceOf<Token>(dropzone);

    }



    [Test]
    public void CreateCharacter()
    {
        
        var characterStable=new GameObject().AddComponent<Stable>();
        var characterCreationCommand=new CharacterCreationCommand();
        var character = characterCreationCommand.Execute(characterStable);
        Assert.IsInstanceOf<Character>(character);
    }

    [Test]
    public void CreateDeckInstance()
    {

        var deckInstanceCreationCommand=new DeckInstanceCreationCommand();
        var deckInstance = deckInstanceCreationCommand.Execute();
        Assert.IsInstanceOf<DeckInstance>(deckInstance);

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
