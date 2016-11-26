using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private IVerb verbMock;
            
        [SetUp]
        public void Setup()
        {
            
            situationAnchorMock = Substitute.For<ISituationAnchor>();
            situationDetailsMock = Substitute.For<ISituationDetails>();
            compendiumMock = Substitute.For<ICompendium>();

            sc = new SituationController(compendiumMock);
            sc.InitialiseToken(situationAnchorMock, verbMock);
            sc.InitialiseWindow(situationDetailsMock);
        }


        //item added to /removed from starting slot updates aspects display and recipe description with starting slot aspects
        [Test]
        public void ItemAddedToStartingSlot_UpdatesAspectsAndRecipeDescription_WithStartingSlotAspects()
        {
         sc.StartingSlotsUpdated();
           situationDetailsMock.Received().GetAspectsFromSlottedElements();
            situationDetailsMock.Received().DisplayRecipe(null);
        }

        //item added to / removed from ongoing slot updates aspects display and recipe *prediction* with stored aspects and ongoing slot aspects

        public void ItemAddedToOngoingSlot_UpdatesAspectsAndRecipePrediction_WithOngoingSlotAspects()
        { }

  

    }
}
