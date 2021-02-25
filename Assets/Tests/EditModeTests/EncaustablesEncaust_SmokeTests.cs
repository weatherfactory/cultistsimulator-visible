using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Commands;
using Assets.Scripts.Application.Entities.NullEntities;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;


[TestFixture]
    public class EncaustablesEncaust_SmokeTests
    {
        private Sphere worldSphere;

        [SetUp]
        public void Setup()
        {
            Watchman.ForgetEverything();
            var sphereObject = new GameObject();
            worldSphere = sphereObject.AddComponent<NullSphere>();
        }

        [Test]
        public void DeckInstance_Encausts()
        {
            var encaustery = new Encaustery<DeckInstanceCreationCommand>();
            var deckInstance = new GameObject().AddComponent<DeckInstance>();
            encaustery.Encaust(deckInstance);
        }
    [Test]
        public void ElementStack_Encausts()
        {
            var encaustery = new Encaustery<ElementStackCreationCommand>();
            var elementStack = new ElementStack();
            encaustery.Encaust(elementStack);
        }



    [Test]
        public void Character_Encausts()
        {

         var encaustery=new Encaustery<CharacterCreationCommand>();
         var characterObject = new GameObject();
         characterObject.AddComponent<Character>();
        characterObject.GetComponent<Character>().ActiveLegacy=new NullLegacy();
        characterObject.GetComponent<Character>().EndingTriggered=NullEnding.Create();
        //pretty horrible, right? worth considering not passing Monobehaviours to encausting, OR use the CreationCommand in the first place!


        encaustery.Encaust(characterObject.GetComponent<Character>());
                }

        [Test]
        public void ElementStackToken_Encausts()
        {
            var encaustery = new Encaustery<TokenCreationCommand>();
          
            var tokenObject=new GameObject();
            var token=tokenObject.AddComponent<Token>();
            var elementStack = new ElementStack();
        token.SetPayload(elementStack);
        worldSphere.AcceptToken(token,new Context(Context.ActionSource.Unknown));
            
            encaustery.Encaust(token);
        }
        [Test]
        public void Situation_Encausts()
        {
            var situationEncaustery = new Encaustery<SituationCreationCommand>();
            var verb = NullVerb.Create();
            var situation = new Situation(verb,verb.DefaultUniqueTokenId());
            situationEncaustery.Encaust(situation);
        }

    
    [Test]
        public void SituationToken_Encausts()
        {
           var encaustery=new Encaustery<TokenCreationCommand>();
           var tokenObject = new GameObject();
           var token=tokenObject.AddComponent<Token>();
           var verb = NullVerb.Create();
           var situation=new Situation(verb,verb.DefaultUniqueTokenId());
           token.SetPayload(situation);
           worldSphere.AcceptToken(token, new Context(Context.ActionSource.Unknown));
           
           encaustery.Encaust(token);
        }

        [Test]
        public void Sphere_Encausts()
        {
            var encaustery = new Encaustery<SphereCreationCommand>();
        var sphereObject = new GameObject();
        Sphere sphere = sphereObject.AddComponent<ThresholdSphere>();
        sphere.GoverningSphereSpec=new SphereSpec(typeof(ThresholdSphere),"chione");
        var sphereCommand= encaustery.Encaust(sphere);
        Assert.AreEqual(sphere.GoverningSphereSpec.Id, sphereCommand.GoverningSphereSpec.Id);
        }


        [Test]
        public void Sphere_Encausts_With_Contents()
        {
            var encaustery = new Encaustery<SphereCreationCommand>();
            var sphereObject = new GameObject();
            Sphere sphere = sphereObject.AddComponent<CardsPile>();
            var tokenObject = new GameObject();
            var token = tokenObject.AddComponent<Token>();
            var payload = new ElementStack();
            token.SetPayload(payload);
            sphere.AcceptToken(token,new Context(Context.ActionSource.Unknown));
            var sphereCreationCommand = encaustery.Encaust(sphere);

            Assert.AreEqual(token.Payload.Quantity, sphereCreationCommand.Tokens.First().Payload.Quantity);
        }

    [Test]
        public void DropzoneToken_Encausts()
        {
            var encaustery = new Encaustery<DropzoneCreationCommand>();
           var dropzone=new Dropzone();
           encaustery.Encaust(dropzone);
        }

}
