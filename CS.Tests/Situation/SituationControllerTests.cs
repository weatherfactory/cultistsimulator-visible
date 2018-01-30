using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
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
        private Registry registry;
            
        [SetUp]
        public void Setup()
        {
            
            situationAnchorMock = Substitute.For<ISituationAnchor>();
            situationDetailsMock = Substitute.For<ISituationDetails>();
            compendiumMock = Substitute.For<ICompendium>();
            characterMock = Substitute.For<Character>();
            _situationMock = Substitute.For<ISituation>();
            basicVerb=new BasicVerb("id","label","description",false);

            var scat=new SituationsCatalogue();
            registry=new Registry();
            registry.Register<SituationsCatalogue>(scat);


            sc = new SituationController(compendiumMock,characterMock);
            
            var command=new SituationCreationCommand(basicVerb,null,SituationState.Unstarted);
            sc.Initialise(command, situationAnchorMock,situationDetailsMock);

            sc.Situation = _situationMock;

        }


       

        [Test]
        public void AllOutputsGone_ResetsStateMachine()
        {
            sc.ResetSituation();
            _situationMock.Received().ResetIfComplete();
        }

        [Test]
        public void SituationHasBeenReset_DisplaysStartingInfoInDetails()
        {
            sc.ResetSituation();
            situationDetailsMock.Received().SetUnstarted();
        }

    }
}
