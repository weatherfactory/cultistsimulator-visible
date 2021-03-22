using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Logic;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;


[TestFixture]
    public class EncaustablesEncaust_SmokeTests
    {
        private Sphere worldSphere;
        private DealersTable dealersTable;

        [SetUp]
        public void Setup()
        {
            Watchman.ForgetEverything();
            FucineRoot.Reset();
            
            worldSphere = Watchman.Get<PrefabFactory>()
                .InstantiateSphere(new SphereSpec(typeof(MinimalSphere), "minimalworldsphere"));

            var dealersTableObj=new GameObject();
            dealersTable = dealersTableObj.AddComponent<DealersTable>();
            var w=new Watchman();
            w.Register(dealersTable);

        }

        [Test]
        public void Root_Encausts()
        {
            var encaustery = new Encaustery<RootPopulationCommand>();
            var root = FucineRoot.Get();
            encaustery.Encaust(root);

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
         characterObject.GetComponent<Character>().ActiveLegacy = NullLegacy.Create();
        characterObject.GetComponent<Character>().EndingTriggered=Ending.NotEnded();
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
            worldSphere.AcceptToken(token, new Context(Context.ActionSource.Unknown));
        token.SetPayload(elementStack);
        
            
            encaustery.Encaust(token);
        }
        [Test]
        public void Situation_Encausts()
        {
            var situationEncaustery = new Encaustery<SituationCreationCommand>();
            var verb = NullVerb.Create();
            var situation = new Situation(verb,verb.Id);
            situationEncaustery.Encaust(situation);
        }

    
    [Test]
        public void SituationToken_Encausts()
        {
           var encaustery=new Encaustery<TokenCreationCommand>();
           var tokenObject = new GameObject();
           var token=tokenObject.AddComponent<Token>();
           var verb = NullVerb.Create();
           var situation=new Situation(verb,verb.Id);
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
            
            var tokenObject = new GameObject();
            var token = tokenObject.AddComponent<Token>();
            var payload = new ElementStack();
            token.SetPayload(payload);
            worldSphere.AcceptToken(token,new Context(Context.ActionSource.Unknown));
            var sphereCreationCommand = encaustery.Encaust(worldSphere);

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
