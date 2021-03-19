using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.Tests.EditModeTests
{
    [TestFixture]
 public class SituationFlowTests
    {
        private HornedAxe _hornedAxe;
        private PrefabFactory _prefabFactory;
        private Sphere _sphereInRoot;
        private Sphere _minimalTabletopSurrogate;
        private Sphere _minimalWindowsSurrogate;

        [SetUp]
        public void Setup()
        {

            Watchman.ForgetEverything();
            FucineRoot.Reset();

            var compendium = new Compendium();
            var cl = new CompendiumLoader("testcontent");
            var importLog = cl.PopulateCompendium(compendium, "en");
            if (importLog.ImportFailed())
            {
                throw new ApplicationException(string.Concat(importLog.GetMessages()));
            }

            var watchman = new Watchman();
            watchman.Register(compendium);
            var gobjStable = new GameObject();
            var stableComponent = gobjStable.AddComponent<Stable>();
            watchman.Register(stableComponent);

            _hornedAxe = Watchman.Get<HornedAxe>();

            var defaultSphereSpec = new SphereSpec(typeof(MinimalSphere), "tabletop");
            defaultSphereSpec.WindowsSpherePath = new FucinePath("~/windows");
            _minimalTabletopSurrogate = Watchman.Get<PrefabFactory>().InstantiateSphere(defaultSphereSpec);
            var windowsSphereSpec = new SphereSpec(typeof(MinimalSphere), "windows");
            _minimalWindowsSurrogate = Watchman.Get<PrefabFactory>().InstantiateSphere(windowsSphereSpec);

        }

        [Test]
        public void ActivateRecipeManually()
        {
            SituationCreationCommand situationCreationCommand = new SituationCreationCommand("t");
            situationCreationCommand.StateIdentifier = StateEnum.Unstarted;
            var situation = situationCreationCommand.Execute(Context.Unknown()) as Situation;

            var activationCommand = TryActivateRecipeCommand.ManualRecipeActivation("req0effect1");
            situation.CommandQueue.AddCommand(activationCommand);
            Assert.AreEqual(StateEnum.Unstarted, situation.StateIdentifier);
            situation.ExecuteHeartbeat(1f);
            Assert.AreEqual(StateEnum.Ongoing, situation.StateIdentifier);

        }

        [Test]
        public void RecipeWithSlot_AddsRecipeThreshold()
        {
            SituationCreationCommand situationCreationCommand = new SituationCreationCommand("t");
            situationCreationCommand.StateIdentifier = StateEnum.Unstarted;
            var situation = situationCreationCommand.Execute(Context.Unknown()) as Situation;

            var recipeWithThreshold = Watchman.Get<Compendium>().GetEntityById<Recipe>("apls");

            var activationCommand = TryActivateRecipeCommand.OverridingRecipeActivation(recipeWithThreshold.Id);
            situation.CommandQueue.AddCommand(activationCommand);
            
            situation.ExecuteHeartbeat(0f); //first time switches to Ongoing
            situation.ExecuteHeartbeat(0f); //second time executes the outstanding command
            Assert.AreEqual(recipeWithThreshold.Id,situation.GetSpheresByCategory(SphereCategory.Threshold).First().Id);
        }

        [Test]
        public void RecipeWithSlot_Replaces_RecipeThreshold_FromPreviousRecipe()
        {
            SituationCreationCommand situationCreationCommand = new SituationCreationCommand("t");
            situationCreationCommand.StateIdentifier = StateEnum.Unstarted;
            var situation = situationCreationCommand.Execute(Context.Unknown()) as Situation;

            var recipeWithThreshold = Watchman.Get<Compendium>().GetEntityById<Recipe>("apls.linkto.bpls.loop");

            var secondRecipeWithThreshold = Watchman.Get<Compendium>().GetEntityById<Recipe>("bpls.loop");

            var activationCommand = TryActivateRecipeCommand.OverridingRecipeActivation(recipeWithThreshold.Id);
            situation.CommandQueue.AddCommand(activationCommand);
            situation.ExecuteHeartbeat(recipeWithThreshold.Warmup+1); //first time switches to Ongoing
            situation.ExecuteHeartbeat(0f); //second time executes the outstanding sphere creation command and switches to RecipeExecution
            situation.ExecuteHeartbeat(0f); //third time switches back to Ongoing
            situation.ExecuteHeartbeat(0f); //fourth time time executes the outstanding sphere creation command and switches to RecipeExecution

            Assert.AreEqual(secondRecipeWithThreshold.Slots.First().Id,situation.GetSpheresByCategory(SphereCategory.Threshold).First().Id);
        }


        [Test]
        public void RecipeSlot_IsRetained_WhenMovingToALinkedRecipeThatDoesntSpecifySlot()
        {
            SituationCreationCommand situationCreationCommand = new SituationCreationCommand("t");
            situationCreationCommand.StateIdentifier = StateEnum.Unstarted;
            var situation = situationCreationCommand.Execute(Context.Unknown()) as Situation;

            var recipeWithThreshold = Watchman.Get<Compendium>().GetEntityById<Recipe>("apls.linkto.hiatus.loop");

            
            var activationCommand = TryActivateRecipeCommand.OverridingRecipeActivation(recipeWithThreshold.Id);
            situation.CommandQueue.AddCommand(activationCommand);
            situation.ExecuteHeartbeat(recipeWithThreshold.Warmup + 1); //first time switches to Ongoing
            situation.ExecuteHeartbeat(0f); //second time executes the outstanding sphere creation command and switches to RecipeExecution
            situation.ExecuteHeartbeat(0f); //third time switches back to Ongoing
            situation.ExecuteHeartbeat(0f); //fourth time time executes the outstanding sphere creation command and switches to RecipeExecution

            Assert.AreEqual(recipeWithThreshold.Slots.First().Id, situation.GetSpheresByCategory(SphereCategory.Threshold).First().Id);
        }

        [Test]
        public void ConsumingAngel_ConsumesWhenTokenFlushed()
        {
            
            var consumingSpec = new SphereSpec(typeof(ThresholdSphere), "consumingthreshold");
            consumingSpec.Consumes = true;
            var consumingSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(consumingSpec);
            
            var outSphereSpec=new SphereSpec(typeof(MinimalSphere),"outsphere");
            var outSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(outSphereSpec);

            var element = Watchman.Get<Compendium>().GetEntitiesAsList<Element>().First();
            var elementStackTokenCreationCommand = new TokenCreationCommand().WithElementStack(element.Id, 1);


            var elementStackToken = elementStackTokenCreationCommand.Execute(new Context(Context.ActionSource.Debug), consumingSphere);
            Assert.IsTrue(elementStackToken.Payload.IsValidElementStack());

            outSphere.AcceptToken(elementStackToken,new Context(Context.ActionSource.FlushingTokens));

            Assert.AreEqual(0,outSphere.Tokens.Count);
        }

        [Test]
        public void UnstartedVerbDisplaysDefaultHint()
        {
            var verb = Watchman.Get<Compendium>().GetEntityById<Verb>("t");
            SituationCreationCommand situationCreationCommand = new SituationCreationCommand(verb.Id);
            
            var situation = situationCreationCommand.Execute(Context.Unknown()) as Situation;
            situation.FirstHeartbeat();
            situation.ExecuteHeartbeat(0f);

            var notesDominion = situation.Dominions.Single(d => d.Identifier == DominionEnum.Notes);
           
            var notesSphere=notesDominion.Spheres.Single();
            var notesStack =notesSphere.Tokens.SingleOrDefault().Payload;
            var notesText = notesStack.GetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY);
            Assert.AreEqual(verb.Description,notesText);
            Assert.AreEqual(situation.Description, notesText);



        }
    }
}
