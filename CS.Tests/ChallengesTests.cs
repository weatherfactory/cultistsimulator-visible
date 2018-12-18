using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Logic;
using NUnit.Framework;

namespace CS.Tests
{

    [TestFixture]
    public class ChallengesTests
    {

        Dictionary<string, string> challenges;
        LinkedRecipeDetails lrd;
        AspectsInContext aspectsInContext;


        [SetUp]
        public void SetUp()
        {


            challenges = new Dictionary<string, string> {{"a", "base"}, {"b", "base"}};
            lrd = new LinkedRecipeDetails("a", 0, false, null, challenges);
           

             aspectsInContext=new AspectsInContext(new AspectsDictionary(), new AspectsDictionary(), new AspectsDictionary());
        }

        [Test]
        public void ArbitedChanceIs0_ForBaseChallengeWithNoRelevantAspects()
        {

            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(0, ca.GetArbitratedChance());

        }

        [Test]
        public void ArbitedChanceIs30_ForBaseChallengeWithRelevantAspectPresent()
        {
            aspectsInContext.AspectsInSituation.Add("a", 1);

            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(30, ca.GetArbitratedChance());

        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithRelevantAspectAt5()
        {
            aspectsInContext.AspectsInSituation.Add("a", 5);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs90_ForBaseChallengeWithRelevantAspectAt10()
        {
            aspectsInContext.AspectsInSituation.Add("a", 10);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(90, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithOneLowerRelevantAspect()
        {
            aspectsInContext.AspectsInSituation.Add("a", 5);
            aspectsInContext.AspectsInSituation.Add("b", 4);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithOneLowerRelevantAspect_ButTheOtherWayRound()
        {
            aspectsInContext.AspectsInSituation.Add("a", 4);
            aspectsInContext.AspectsInSituation.Add("b", 5);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithTwoRelevantAspectsAt5()
        {
            aspectsInContext.AspectsInSituation.Add("a", 5);
            aspectsInContext.AspectsInSituation.Add("b", 5);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs30_ForBaseChallengeWithTwoRelevantAspectsAt3()
        {
            aspectsInContext.AspectsInSituation.Add("a", 3);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, lrd);

            Assert.AreEqual(30, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs0_ForBAdvancedChallenge_WithAspect9()
        {
            aspectsInContext.AspectsInSituation.Add("a", 9);

           var advancedChallenges = new Dictionary<string, string> { { "a", "advanced" }};
          var advancedLrd = new LinkedRecipeDetails("a", 0, false, null, advancedChallenges);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, advancedLrd);

            Assert.AreEqual(0, ca.GetArbitratedChance());
        }
        [Test]
        public void ArbitedChanceIs30_ForBAdvancedChallenge_WithAspect10()
        {
            aspectsInContext.AspectsInSituation.Add("a", 10);

            var advancedChallenges = new Dictionary<string, string> { { "a", "advanced" } };
            var advancedLrd = new LinkedRecipeDetails("a", 0, false, null, advancedChallenges);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, advancedLrd);

            Assert.AreEqual(30, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs70_ForBAdvancedChallenge_WithAspect15()
        {
            aspectsInContext.AspectsInSituation.Add("a", 15);

            var advancedChallenges = new Dictionary<string, string> { { "a", "advanced" }};
            var advancedLrd = new LinkedRecipeDetails("a", 0, false, null, advancedChallenges);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, advancedLrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs90_ForBAdvancedChallenge_WithAspect20()
        {
            aspectsInContext.AspectsInSituation.Add("a", 20);

            var advancedChallenges = new Dictionary<string, string> { { "a", "advanced" } };
            var advancedLrd = new LinkedRecipeDetails("a", 0, false, null, advancedChallenges);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsInContext, advancedLrd);

            Assert.AreEqual(90, ca.GetArbitratedChance());
        }

    }
}

