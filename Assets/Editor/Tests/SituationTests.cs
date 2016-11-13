using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
   public class SituationTests
    {
        private Recipe r;
        [SetUp]
        public void Setup()
        {
            r=new Recipe();
            r.Warmup = 1;
            
        }

        [Test]
        public void SituationMovesFromOngoingToComplete_WhenContinuingAtTimeBelowZero()
        {
            Situation s=new Situation(r);
            Assert.AreEqual(SituationState.Ongoing,s.State);
            Assert.AreEqual(SituationState.Ongoing, s.Continue(1));
            Assert.AreEqual(SituationState.Complete, s.Continue(1));
            Assert.AreEqual(SituationState.Complete, s.State);

        }

        [Test]
        public void SituationMovesFromCompleteToExtinct_WhenContinuingAtComplete()
        {
            Situation s = new Situation(r);
            s.Continue(1); //ongoing
            s.Continue(1); //completes
            Assert.AreEqual(SituationState.Extinct, s.Continue(1)); //now extinct
            Assert.AreEqual(SituationState.Extinct, s.State);
        }
    }
}
