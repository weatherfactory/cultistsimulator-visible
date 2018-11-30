using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;

namespace Assets.Logic
{
    public class ChallengeArbiter
    {
        private readonly IAspectsDictionary _aspectsToConsider;
        private readonly LinkedRecipeDetails _link;
        private const string BASE_CONVENTION_ID = "base";

        public ChallengeArbiter(IAspectsDictionary aspectsToConsider,LinkedRecipeDetails link)
        {
            _aspectsToConsider = aspectsToConsider;
            _link = link;
        }


        public int GetArbitratedChance()
        {
            int arbitratedChance = _link.Chance;

            foreach (var kvp in _link.Challenges)
            {
                if (kvp.Value != BASE_CONVENTION_ID)
                    throw new ApplicationException("We don't know what to do with a non-base difficulty convention");
                    int chanceFromAspect = ChanceForBaseConvention(_aspectsToConsider.AspectValue(kvp.Key));
                if (chanceFromAspect > arbitratedChance)
                    arbitratedChance = chanceFromAspect;


            }

            return arbitratedChance;
        }

        private int ChanceForBaseConvention(int aspectLevel)
        {
            if (aspectLevel >= 10)
                return 90;
            if (aspectLevel >= 5)
                return 70;
            if (aspectLevel >= 1)
                return 30;

            return 0;
        }

    }
}
