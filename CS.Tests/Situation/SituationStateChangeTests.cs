using System;
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
   public class SituationStateChangeTests
    {
        private Recipe r1;
        private Recipe r2;
        private Recipe r3;
        private IRecipeConductor rc;
        private ISituationSubscriber subscriber;
        [SetUp]
        public void Setup()
        {
            r1 = TestObjectGenerator.GenerateRecipe(1);
            r2 = TestObjectGenerator.GenerateRecipe(2);
            r3 = TestObjectGenerator.GenerateRecipe(3);
            r3 = TestObjectGenerator.GenerateRecipe(4);
            r1.Warmup = 0;
           rc = Substitute.For<IRecipeConductor>();
             subscriber =
                Substitute.For<ISituationSubscriber>();

        }

        [Test]
        public void NewSituation_IsStateUnstarted()
        {
            Core.Entities.Situation s=new Core.Entities.Situation(subscriber);
            Assert.AreEqual(SituationState.Unstarted,s.State);
        }

        [Test]
        public void UnstartedSituation_MovesToFreshlyStarted_WhenStartedWithRecipe()
        {
            Core.Entities.Situation s=new Core.Entities.Situation(subscriber);
            s.Start(r1);
            Assert.AreEqual(SituationState.FreshlyStarted,s.State);
        }

        [Test]
        public void UnstartedSituation_DoesNotChangeStateWhenContinued()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(subscriber);
            s.Continue(rc,1);
            Assert.AreEqual(SituationState.Unstarted, s.State);
        }

        [Test]
        public void FreshlyStartedSituation_MovesToOngoingAtFirstContinue()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(subscriber);
            s.Start(r1);
            s.Continue(rc, 1);
            Assert.AreEqual(SituationState.Ongoing, s.State);
        }

        [Test]
        public void SituationMovesFromOngoingToRequiringExecution_WhenContinuingAtTimeBelowZero()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(0, SituationState.Ongoing, r1, subscriber);
            Assert.AreEqual(SituationState.Ongoing,s.State);
            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> { r1 });
            s.Continue(rc, 1);
            Assert.AreEqual(SituationState.RequiringExecution, s.State);
        }

        [Test]
        public void Situation_RequiresExecution_When_ContinuingAtTimeBelowZero()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(0,SituationState.Ongoing,r1, subscriber);

            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> { r1 });

            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<SituationEffectCommand>(ec => ec.Recipe == r1));

        }


        [Test]
        public void Situation_BeginsLoopRecipe_WhenRecipeConductorSpecifiesLoopRecipe()
        {
            
            rc.GetLoopedRecipe(null).ReturnsForAnyArgs(r2);
            Core.Entities.Situation s = new Core.Entities.Situation(0, SituationState.RequiringExecution, r1, subscriber);
            s.Continue(rc,1);
            Assert.AreEqual(r2.Id,s.RecipeId);
            Assert.AreEqual(SituationState.Ongoing,s.State);
        }

        [Test]
        public void Situation_ResetsTimer_WhenBeginningLoopRecipe()
        {

            rc.GetLoopedRecipe(null).ReturnsForAnyArgs(r2);
            Core.Entities.Situation s = new Core.Entities.Situation(0, SituationState.RequiringExecution, r1, subscriber);
            r2.Warmup = 100;
            s.Continue(rc, 1);
            Assert.AreEqual(r2.Warmup, s.TimeRemaining);
        }

        [Test]
        public void Situation_GoesComplete_WhenRecipeConductorSpecifiesNoLoopRecipe()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(0, SituationState.RequiringExecution, r1, subscriber);
            s.Continue(rc,1);
            Assert.AreEqual(SituationState.Complete, s.State);
        }

        [Test]
        public void Situation_AllOutputsGone_ReturnsToBlankState_IfSituationComplete()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(30, SituationState.Complete, r1, subscriber);
            s.ResetIfComplete();
            Assert.AreEqual(SituationState.Unstarted,s.State);
            Assert.AreEqual(null,s.RecipeId);
            Assert.AreEqual(0,s.TimeRemaining);
        }

        [Test]
        public void Situation_AllOutputsGone_DoesNotReturnToBlankState_IfSituationNotComplete()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(30, SituationState.Ongoing, r1, subscriber);
            s.ResetIfComplete();
            Assert.AreNotEqual(SituationState.Unstarted, s.State);
            Assert.AreNotEqual(null, s.RecipeId);
            Assert.AreNotEqual(0, s.TimeRemaining);
        }


        [Test]
        public void Situation_ExecutesAlternativesSpecifiedByRecipeConductor()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(0,SituationState.Ongoing, r1, subscriber);

            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> {r2, r3});

            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<SituationEffectCommand>(ec=>ec.Recipe==r2));
            subscriber.Received().SituationExecutingRecipe(Arg.Is<SituationEffectCommand>(ec => ec.Recipe == r3));

        }

        [Test]
        public void Situation_SpecifiesAdditional_ShouldRunAsNewSituation_IfVerbIsDifferent()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(0, SituationState.Ongoing, r1, subscriber);
            
            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> { r2, r3 });
            r3.ActionId = r2.ActionId + " a difference";
            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<SituationEffectCommand>(ec => ec.Recipe==r3 && ec.AsNewSituation));

        }

        [Test]
        public void Situation_SpecifiesAdditional_ShouldNotRunAsNewSituation_IfVerbIsTheSame()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(0, SituationState.Ongoing, r1, subscriber);
            
            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> { r2, r3 });
            r3.ActionId = r2.ActionId;
            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<SituationEffectCommand>(ec => ec.Recipe == r3 && !ec.AsNewSituation));

        }


        [Test]
        public void Situation_LoopsFromAlternative_NotOriginal_IfSpecified()
        {
            Core.Entities.Situation s = new Core.Entities.Situation(0, SituationState.Ongoing, r1, subscriber);
            

            Recipe loopedRecipe = TestObjectGenerator.GenerateRecipe(99);

            rc.GetActualRecipesToExecute(r1).Returns(new List<Recipe> {r2, r3});
            rc.GetLoopedRecipe(r2).Returns(loopedRecipe);

            s.Continue(rc, 1); //executes

            s.Continue(rc, 1); //ends

            Assert.AreEqual(loopedRecipe.Id, s.RecipeId);
        }


    }
}
