using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
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
            Core.Entities.SituationClock s=new Core.Entities.SituationClock(subscriber);
            Assert.AreEqual(SituationState.Unstarted,s.State);
        }

        [Test]
        public void UnstartedSituation_MovesToFreshlyStarted_WhenStartedWithRecipe()
        {
            Core.Entities.SituationClock s=new Core.Entities.SituationClock(subscriber);
            s.Start(r1);
            Assert.AreEqual(SituationState.FreshlyStarted,s.State);
        }

        [Test]
        public void UnstartedSituation_DoesNotChangeStateWhenContinued()
        {
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(subscriber);
            s.Continue(rc,1);
            Assert.AreEqual(SituationState.Unstarted, s.State);
        }

        [Test]
        public void FreshlyStartedSituation_MovesToOngoingAtFirstContinue()
        {
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(subscriber);
            s.Start(r1);
            s.Continue(rc, 1);
            Assert.AreEqual(SituationState.Ongoing, s.State);
        }




        [Test]
        public void Situation_BeginsLoopRecipe_WhenRecipeConductorSpecifiesLoopRecipe()
        {
            
            rc.GetLinkedRecipe(null).ReturnsForAnyArgs(r2);
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(0, SituationState.RequiringExecution, r1, subscriber);
            s.Continue(rc,1);
            Assert.AreEqual(r2.Id,s.RecipeId);
            Assert.AreEqual(SituationState.Ongoing,s.State);
        }

 

   
        [Test]
        public void Situation_AllOutputsGone_ReturnsToBlankState_IfSituationComplete()
        {
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(30, SituationState.Complete, r1, subscriber);
            s.ResetIfComplete();
            Assert.AreEqual(SituationState.Unstarted,s.State);
            Assert.AreEqual(null,s.RecipeId);
            Assert.AreEqual(0,s.TimeRemaining);
        }

        [Test]
        public void Situation_AllOutputsGone_DoesNotReturnToBlankState_IfSituationNotComplete()
        {
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(30, SituationState.Ongoing, r1, subscriber);
            s.ResetIfComplete();
            Assert.AreNotEqual(SituationState.Unstarted, s.State);
            Assert.AreNotEqual(null, s.RecipeId);
            Assert.AreNotEqual(0, s.TimeRemaining);
        }


        [Test]
        public void Situation_SpecifiesAdditional_ShouldRunAsNewSituation_IfVerbIsDifferent()
        {
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(0, SituationState.Ongoing, r1, subscriber);
            
            rc.GetActualRecipesToExecute(r1).Returns(new List<RecipeExecutionCommand> { new RecipeExecutionCommand(r2,null), new RecipeExecutionCommand(r3,null) });
            r3.ActionId = r2.ActionId + " a difference";
            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<SituationEffectCommand>(ec => ec.Recipe==r3 && ec.AsNewSituation));

        }

        [Test]
        public void Situation_SpecifiesAdditional_ShouldNotRunAsNewSituation_IfVerbIsTheSame()
        {
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(0, SituationState.Ongoing, r1, subscriber);
            
            rc.GetActualRecipesToExecute(r1).Returns(new List<RecipeExecutionCommand> { new RecipeExecutionCommand(r2,null), new RecipeExecutionCommand(r3,null) });
            r3.ActionId = r2.ActionId;
            s.Continue(rc, 1);

            subscriber.Received().SituationExecutingRecipe(Arg.Is<SituationEffectCommand>(ec => ec.Recipe == r3 && !ec.AsNewSituation));

        }


        [Test]
        public void Situation_LoopsFromAlternative_NotOriginal_IfSpecified()
        {
            Core.Entities.SituationClock s = new Core.Entities.SituationClock(0, SituationState.Ongoing, r1, subscriber);
            

            Recipe loopedRecipe = TestObjectGenerator.GenerateRecipe(99);

            rc.GetActualRecipesToExecute(r1).Returns(new List<RecipeExecutionCommand> { new RecipeExecutionCommand(r2, null), new RecipeExecutionCommand(r3, null) });
            rc.GetLinkedRecipe(r2).Returns(loopedRecipe);

            s.Continue(rc, 1); //executes

            s.Continue(rc, 1); //ends

            Assert.AreEqual(loopedRecipe.Id, s.RecipeId);
        }


    }
}
