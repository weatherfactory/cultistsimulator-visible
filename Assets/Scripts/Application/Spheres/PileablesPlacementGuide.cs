using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Abstract;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    public class PileablesPlacementGuide : AbstractPlacementGuide
    {
        public override bool TokenAllowedHere(Token token)
        {
            if (token.OccupiesSpaceAs() == OccupiesSpaceAs.Someone)
                return false;

            return true;
        }
    }
}
