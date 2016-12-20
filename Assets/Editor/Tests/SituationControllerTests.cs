using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class SituationControllerTests
    {
        private SituationController sc;
        private ICompendium compendiumMock;
        private ISituationAnchor situationAnchorMock;
        private ISituationDetails situationDetailsMock;
        private ISituationStateMachine situationStateMachineMock;
        private IVerb basicVerb;
            
        [SetUp]
        public void Setup()
        {
            
            situationAnchorMock = Substitute.For<ISituationAnchor>();
            situationDetailsMock = Substitute.For<ISituationDetails>();
            compendiumMock = Substitute.For<ICompendium>();
            situationStateMachineMock = Substitute.For<ISituationStateMachine>();
            basicVerb=new BasicVerb("id","label","description");


            sc = new SituationController(compendiumMock);
            
            var command=new SituationCreationCommand(basicVerb,null);
            sc.Initialise(command, situationAnchorMock,situationDetailsMock);

            sc.SituationStateMachine = situationStateMachineMock;

        }


        

        //item added to /removed from starting slot updates aspects display and recipe description with starting slot aspects
        [Test]
        public void ItemAddedToStartingSlot_UpdatesAspectsAndRecipeDescription_WithStartingSlotAspects()
        {
            IAspectsDictionary startingSlotAspects=new AspectsDictionary {{ "1",1}};
            var recipe = TestObjectGenerator.GenerateRecipe(1);
            situationDetailsMock.GetAspectsFromSlottedElements().Returns(startingSlotAspects);
            compendiumMock.GetFirstRecipeForAspectsWithVerb(null,"").ReturnsForAnyArgs(recipe);
          sc.StartingSlotsUpdated();
            situationDetailsMock.Received(1).DisplayAspects(startingSlotAspects);
            situationDetailsMock.Received().DisplayRecipe(recipe);
        }

        //item added to / removed from ongoing slot updates aspects display and recipe *prediction* with stored aspects and ongoing slot aspects
        [Test]
        public void ItemAddedToOngoingSlot_UpdatesAspectsAndRecipePrediction_WithOngoingSlotAspects()
        {

            sc.OngoingSlotsUpdated();
            situationAnchorMock.Received().GetAspectsFromStoredElements();
            situationAnchorMock.Received().GetAspectsFromSlottedElements();
            situationDetailsMock.ReceivedWithAnyArgs().DisplayAspects(null);
            situationDetailsMock.ReceivedWithAnyArgs().UpdateSituationDisplay("","","");

        }

        [Test]
        public void AllOutputsGone_ResetsStateMachine()
        {
            sc.AllOutputsGone();
            situationStateMachineMock.Received().AllOutputsGone();
        }

        [Test]
        public void SituationHasBeenReset_DisplaysStartingInfoInDetails()
        {
            sc.SituationHasBeenReset();
            situationDetailsMock.Received().DisplayStarting();
        }

    }
}
