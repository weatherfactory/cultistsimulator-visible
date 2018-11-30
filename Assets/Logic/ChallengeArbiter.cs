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
                if (kvp.Value != "base")
                    throw new ApplicationException("We don't know what to do with a non-base difficulty convention");
                if (_aspectsToConsider.ContainsKey(kvp.Key))
                    arbitratedChance = 30;


            }

            return arbitratedChance;
        }

    }
}
