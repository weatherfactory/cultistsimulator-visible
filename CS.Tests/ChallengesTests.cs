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
        lrd = new LinkedRecipeDetails("a",0,false,null,challenges);
        aspectsAvailable = new AspectsDictionary();

        
        }

    [Test]
    public void ArbitedChanceIs0_ForStandardChallengeWithNoRelevantAspects()
    {
        
        ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable, lrd);

        Assert.AreEqual(0, ca.GetArbitratedChance());

    }

        [Test]
    public void ArbitedChanceIs30_ForStandardChallengeWithRelevantAspectA()
    {
        aspectsAvailable.Add("a",1);

        ChallengeArbiter ca = new ChallengeArbiter(aspectsAvailable,lrd);

            Assert.AreEqual(30,ca.GetArbitratedChance());

    }
}
}
