using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Logic;
using NUnit.Framework;

namespace CS.Tests
{

    [TestFixture]
    public class ChallengesTests
    {

        Dictionary<string, string> challenges;
        LinkedRecipeDetails lrd;
        AspectsDictionary aspectsAvailable;


        [SetUp]
        public void SetUp()
        {


            challenges = new Dictionary<string, string> {{"a", "base"}, {"b", "base"}};
            lrd = new LinkedRecipeDetails("a", 0, false, null, challenges);
            aspectsAvailable = new AspectsDictionary();


        }

        [Test]
        public void ArbitedChanceIs0_ForBaseChallengeWithNoRelevantAspects()
        {

            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(0, ca.GetArbitratedChance());

        }

        [Test]
        public void ArbitedChanceIs30_ForBaseChallengeWithRelevantAspectPresent()
        {
            aspectsAvailable.Add("a", 1);

            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(30, ca.GetArbitratedChance());

        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithRelevantAspectAt5()
        {
            aspectsAvailable.Add("a", 5);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs90_ForBaseChallengeWithRelevantAspectAt10()
        {
            aspectsAvailable.Add("a", 10);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(90, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithOneLowerRelevantAspect()
        {
            aspectsAvailable.Add("a", 5);
            aspectsAvailable.Add("b", 4);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithOneLowerRelevantAspect_ButTheOtherWayRound()
        {
            aspectsAvailable.Add("a", 4);
            aspectsAvailable.Add("b", 5);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs70_ForBaseChallengeWithTwoRelevantAspectsAt5()
        {
            aspectsAvailable.Add("a", 5);
            aspectsAvailable.Add("b", 5);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(70, ca.GetArbitratedChance());
        }

        [Test]
        public void ArbitedChanceIs30_ForBaseChallengeWithTwoRelevantAspectsAt3()
        {
            aspectsAvailable.Add("a", 3);
            ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

            Assert.AreEqual(30, ca.GetArbitratedChance());
        }

    }
}

