using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;

namespace Assets.Logic
{
    public class ChallengeArbiter
    {
        private readonly IAspectsDictionary _aspectsToConsider;
        private readonly LinkedRecipeDetails _link;
        private const string BASE_CONVENTION_ID = "base";
        private const string ADVANCED_CONVENTION_ID = "advanced";

        public ChallengeArbiter(AspectsInContext aspectsToConsider,LinkedRecipeDetails link)
        {
            _aspectsToConsider = aspectsToConsider.AspectsInSituation;
            _link = link;
        }


        public int GetArbitratedChance()
        {
            if (_link.Challenges.Count == 0)
                return _link.Chance;

            int arbitratedChance = 0; 

            foreach (var kvp in _link.Challenges)
            {
                int chanceFromAspect = 0;
                if (kvp.Value == BASE_CONVENTION_ID)
                     chanceFromAspect = ChanceForBaseConvention(_aspectsToConsider.AspectValue(kvp.Key));
                else if (kvp.Value == ADVANCED_CONVENTION_ID)
                    chanceFromAspect = ChanceForAdvancedConvention(_aspectsToConsider.AspectValue(kvp.Key));
                else
                    throw new ApplicationException("We don't know what to do with this difficulty convention: " + kvp.Value);
                 
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

        private int ChanceForAdvancedConvention(int aspectLevel)
        {
            if (aspectLevel >= 20)
                return 90;
            if (aspectLevel >= 15)
                return 70;
            if (aspectLevel >= 10)
                return 30;
            if (aspectLevel >= 5)
                return 10;

            return 0;
        }

    }
}
