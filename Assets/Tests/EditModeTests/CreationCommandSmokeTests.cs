using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Application.Commands;
using Assets.Scripts.Application.Entities;
using NUnit.Framework;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.TestTools;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

[TestFixture]
public class CreationCommandsSmokeTests
{
    private HornedAxe _hornedAxe;
    private Sphere _minimalTabletopSurrogate;
    private Sphere _minimalWindowsSurrogate;

    [SetUp]
    public void WithMinimalCompendiumLoad()
    {
        Watchman.ForgetEverything();
        FucineRoot.Reset();

        var compendium = new Compendium();
        var cl = new CompendiumLoader("testcontent");
        var importLog = cl.PopulateCompendium(compendium, "en");
        foreach (var m in importLog.GetMessages())
            Debug.Log(m.Description);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<DeckSpec>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Dictum>().Count);
        Assert.AreEqual(6, compendium.GetEntitiesAsList<Element>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Ending>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Legacy>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Recipe>().Count);
        Assert.AreEqual(27, compendium.GetEntitiesAsList<Setting>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Verb>().Count);

        var watchman = new Watchman();
        watchman.Register(compendium);
        var gobjStable = new GameObject();
        var stableComponent=gobjStable.AddComponent<Stable>();
        watchman.Register(stableComponent);

        _hornedAxe = Watchman.Get<HornedAxe>();

        var defaultSphereSpec = new SphereSpec(typeof(MinimalSphere), "tabletop");
        defaultSphereSpec.WindowsSpherePath = new FucinePath("~/windows");
        _minimalTabletopSurrogate = Watchman.Get<PrefabFactory>().InstantiateSphere(defaultSphereSpec);
        var windowsSphereSpec = new SphereSpec(typeof(MinimalSphere), "windows");
        _minimalWindowsSurrogate = Watchman.Get<PrefabFactory>().InstantiateSphere(windowsSphereSpec);

    }

    [Test]
    public void CreateElementStack()
    {
    string elementId= Watchman.Get<Compendium>().GetEntitiesAsList<Element>().First().Id;
        int elementQuantity= 3;
        var elementStackCreationCommand = new ElementStackCreationCommand(elementId, elementQuantity);
        var elementStack=elementStackCreationCommand.Execute(new Context(Context.ActionSource.Unknown), _minimalTabletopSurrogate);
        Assert.IsTrue(elementStack.IsValidElementStack());
        Assert.AreEqual(elementId,elementStack.EntityId);
        Assert.AreEqual(elementQuantity, elementStack.Quantity);
    }

    [Test]
        public void CreateElementStackToken()
        {
        var element = Watchman.Get<Compendium>().GetEntitiesAsList<Element>().First();
        var elementStackCreationCommand = new ElementStackCreationCommand(element.Id,1);
        var location = new TokenLocation(Vector3.zero, Watchman.Get<HornedAxe>().GetDefaultSphere());
        var elementStackTokenCreationCommand = new TokenCreationCommand(elementStackCreationCommand, location);
       var elementStackToken=elementStackTokenCreationCommand.Execute(new Context(Context.ActionSource.Debug), _minimalTabletopSurrogate);
        Assert.IsTrue(elementStackToken.Payload.IsValidElementStack());
    }

        [Test]
        public void CreateSituation()
        {
            var situationCreationCommand=new SituationCreationCommand();
            situationCreationCommand.VerbId = NullVerb.Create().Id;
            situationCreationCommand.RecipeId = NullRecipe.Create().Id;
            situationCreationCommand.StateForRehydration = StateEnum.Unstarted;
            var situation = situationCreationCommand.Execute(new Context(Context.ActionSource.Unknown), _minimalTabletopSurrogate);
            Assert.IsInstanceOf<Situation>(situation);
        }


        [Test]
        public void CreateSituationToken()
        {
            var situationCreationCommand = new SituationCreationCommand();
            situationCreationCommand.VerbId = NullVerb.Create().Id;
        situationCreationCommand.RecipeId = NullRecipe.Create().Id;
        situationCreationCommand.StateForRehydration = StateEnum.Unstarted;
        var location = new TokenLocation(Vector3.zero, Watchman.Get<HornedAxe>().GetDefaultSphere());

        var tokenCreationCommand =new TokenCreationCommand(situationCreationCommand,location);
        var token = tokenCreationCommand.Execute(new Context(Context.ActionSource.Unknown), Watchman.Get<HornedAxe>().GetDefaultSphere());
        Assert.IsInstanceOf<Token>(token);

    }


    [Test]
    public void CreateDropzoneToken()
    {
        
        var dropzonePayloadCreationCommand=new DropzoneCreationCommand();
        var dropzoneLocation = new TokenLocation(Vector3.zero, Watchman.Get<HornedAxe>().GetDefaultSphere());
        var dropzoneCreationCommand = new TokenCreationCommand(dropzonePayloadCreationCommand, dropzoneLocation);
        var dropzone=dropzoneCreationCommand.Execute(new Context(Context.ActionSource.Debug), Watchman.Get<HornedAxe>().GetDefaultSphere());
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
