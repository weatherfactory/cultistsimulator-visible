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
        private Character characterMock;
        private ISituationAnchor situationAnchorMock;
        private ISituationDetails situationDetailsMock;
        private ISituation _situationMock;
        private IVerb basicVerb;
            
        [SetUp]
        public void Setup()
        {
            
            situationAnchorMock = Substitute.For<ISituationAnchor>();
            situationDetailsMock = Substitute.For<ISituationDetails>();
            compendiumMock = Substitute.For<ICompendium>();
            characterMock = Substitute.For<Character>();
            _situationMock = Substitute.For<ISituation>();
            basicVerb=new BasicVerb("id","label","description",false);


            sc = new SituationController(compendiumMock,characterMock);
            
            var command=new SituationCreationCommand(basicVerb,null);
            sc.Initialise(command, situationAnchorMock,situationDetailsMock);

            sc.Situation = _situationMock;

        }


        

        //item added to /removed from starting slot updates aspects display and recipe description with starting slot aspects
        [Test]
        public void ItemAddedToStartingSlot_UpdatesAspectsAndRecipeDescription_WithStartingSlotAspects()
        {
            IAspectsDictionary startingSlotAspects=new AspectsDictionary {{ "1",1}};
            var recipe = TestObjectGenerator.GenerateRecipe(1);
            situationDetailsMock.GetAspectsFromAllSlottedElements().Returns(startingSlotAspects);
            compendiumMock.GetFirstRecipeForAspectsWithVerb(null,"", new Character()).ReturnsForAnyArgs(recipe);
          sc.StartingSlotsUpdated();
            situationDetailsMock.Received(1).DisplayAspects(startingSlotAspects);
            situationDetailsMock.Received().DisplayStartingRecipeFound(recipe);
        }

        [Test]
        public void ElementInStartingSlot_IsMovedToSituationStoredElements_WhenSituationIsClosed()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void ElementInSituationStoredElements_IsMovedToStartingSlot_WhenSituationIsOpened()
        {
            throw new NotImplementedException();
        }

        


        //item added to / removed from ongoing slot updates aspects display and recipe *prediction* with stored aspects and ongoing slot aspects
        [Test]
        public void ItemAddedToOngoingSlot_UpdatesAspectsAndRecipePrediction_WithOngoingSlotAspects()
        {
            situationDetailsMock.GetAspectsFromStoredElements().Returns(new AspectsDictionary());
            situationDetailsMock.GetAspectsFromAllSlottedElements().Returns(new AspectsDictionary());

            sc.OngoingSlotsUpdated();

            situationDetailsMock.ReceivedWithAnyArgs().DisplayAspects(null);
            situationDetailsMock.ReceivedWithAnyArgs().UpdateTextForPrediction(new RecipePrediction {Commentary="",DescriptiveText = "",Title = ""});

        }

        [Test]
        public void AllOutputsGone_ResetsStateMachine()
        {
            sc.AllOutputsGone();
            _situationMock.Received().AllOutputsGone();
        }

        [Test]
        public void SituationHasBeenReset_DisplaysStartingInfoInDetails()
        {
            sc.SituationHasBeenReset();
            situationDetailsMock.Received().SetStarting();
        }

    }
}
