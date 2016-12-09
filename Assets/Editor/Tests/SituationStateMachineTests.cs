﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
   public class SituationStateMachineTests
    {
        private Recipe r1;
        private Recipe r2;
        private Recipe r3;
        private IRecipeConductor rc;

        [SetUp]
        public void Setup()
        {
            r1 = TestObjectGenerator.GenerateRecipe(1);
            r2 = TestObjectGenerator.GenerateRecipe(2);
            r3 = TestObjectGenerator.GenerateRecipe(3);
            r3 = TestObjectGenerator.GenerateRecipe(4);
            r1.Warmup = 0;
           rc = Substitute.For<IRecipeConductor>();

        }

        [Test]
        public void NewSituation_IsStateUnstarted()
        {
            SituationStateMachine s=new SituationStateMachine(r1);
            Assert.AreEqual(SituationState.Fresh,s.State);
        }


        [Test]
        public void UnstartedSituation_MovesToOngoingAtFirstContinue()
        {
            SituationStateMachine s = new SituationStateMachine(r1);
            s.Continue(rc, 1);
            Assert.AreEqual(SituationState.Ongoing, s.State);
        }

        [Test]
        public void SituationMovesFromOngoingToRequiringExecution_WhenContinuingAtTimeBelowZero()
        {
            SituationStateMachine s = new SituationStateMachine(0, SituationState.Ongoing, r1);
            Assert.AreEqual(SituationState.Ongoing,s.State);
            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> { r1 });
            s.Continue(rc, 1);
            Assert.AreEqual(SituationState.RequiringExecution, s.State);
        }

        [Test]
        public void Situation_RequiresExecution_When_ContinuingAtTimeBelowZero()
        {
            SituationStateMachine s = new SituationStateMachine(0,SituationState.Ongoing,r1);
            ISituationStateMachineSituationSubscriber subscriber = Substitute.For<ISituationStateMachineSituationSubscriber>();
            s.Subscribe(subscriber);

            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> { r1 });

            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<EffectCommand>(ec => ec.Recipe == r1));

        }


        [Test]
        public void Situation_BeginsLoopRecipe_WhenRecipeConductorSpecifiesLoopRecipe()
        {
            
            rc.GetLoopedRecipe(null).ReturnsForAnyArgs(r2);
            SituationStateMachine s = new SituationStateMachine(0, SituationState.RequiringExecution, r1);
            s.Continue(rc,1);
            Assert.AreEqual(r2.Id,s.RecipeId);
            Assert.AreEqual(SituationState.Ongoing,s.State);
        }

        [Test]
        public void Situation_ResetsTimer_WhenBeginningLoopRecipe()
        {

            rc.GetLoopedRecipe(null).ReturnsForAnyArgs(r2);
            SituationStateMachine s = new SituationStateMachine(0, SituationState.RequiringExecution, r1);
            r2.Warmup = 100;
            s.Continue(rc, 1);
            Assert.AreEqual(r2.Warmup, s.TimeRemaining);
        }

        [Test]
        public void Situation_GoesExtinct_WhenRecipeConductorSpecifiesNoLoopRecipe()
        {
            SituationStateMachine s = new SituationStateMachine(0, SituationState.RequiringExecution, r1);
            s.Continue(rc,1);
            Assert.AreEqual(SituationState.Extinct, s.State);
        }



        [Test]
        public void Situation_ExecutesAlternativesSpecifiedByRecipeConductor()
        {
            SituationStateMachine s = new SituationStateMachine(0,SituationState.Ongoing, r1);
            ISituationStateMachineSituationSubscriber subscriber = Substitute.For<ISituationStateMachineSituationSubscriber>();
            s.Subscribe(subscriber);
            
            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> {r2, r3});

            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<EffectCommand>(ec=>ec.Recipe==r2));
            subscriber.Received().SituationExecutingRecipe(Arg.Is<EffectCommand>(ec => ec.Recipe == r3));

        }


        [Test]
        public void Situation_LoopsFromAlternative_NotOriginal_IfSpecified()
        {
            SituationStateMachine s = new SituationStateMachine(0, SituationState.Ongoing, r1);
            ISituationStateMachineSituationSubscriber subscriber = Substitute.For<ISituationStateMachineSituationSubscriber>();
            s.Subscribe(subscriber);


            Recipe loopedRecipe = TestObjectGenerator.GenerateRecipe(99);

            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> {r2, r3});
            rc.GetLoopedRecipe(r2).Returns(loopedRecipe);

            s.Continue(rc, 1); //executes

            s.Continue(rc, 1); //ends

            Assert.AreEqual(loopedRecipe.Id, s.RecipeId);
        }

    }
}
