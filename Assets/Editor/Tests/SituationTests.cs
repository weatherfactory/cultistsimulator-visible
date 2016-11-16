using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
   public class SituationTests
    {
        private Recipe r1;
        private Recipe r2;
        [SetUp]
        public void Setup()
        {
            r1 = TestObjectGenerator.GenerateRecipe(1);
            r2 = TestObjectGenerator.GenerateRecipe(2);
            r1.Warmup = 1;
            
        }

        [Test]
        public void SituationMovesFromOngoingToComplete_WhenContinuingAtTimeBelowZero()
        {
            Situation s=new Situation(r1);
            Assert.AreEqual(SituationState.Ongoing,s.State);
            Assert.AreEqual(SituationState.Ongoing, s.Continue(1));
            Assert.AreEqual(SituationState.Complete, s.Continue(1));
            Assert.AreEqual(SituationState.Complete, s.State);

        }

        [Test]
        public void SituationMovesFromCompleteToExtinct_WhenContinuingAtComplete()
        {
            Situation s = new Situation(r1);
            s.Continue(1); //ongoing
            s.Continue(1); //completes
            Assert.AreEqual(SituationState.Extinct, s.Continue(1)); //now extinct
            Assert.AreEqual(SituationState.Extinct, s.State);
        }

        [Test]
        public void Situation_ObeysRecipeConductor_TryBeginNextRecipe()
        {
            Situation s=new Situation(r1);
            IRecipeConductor rc = Substitute.For<IRecipeConductor>();
            rc.GetNextRecipe(null).ReturnsForAnyArgs(r2);
            s.Continue(1); //ongoing
            s.Continue(1); //completes
            s.TryBeginNextRecipe(rc);
            Assert.AreEqual(r2.Id,s.RecipeId);
        }

    }
}
