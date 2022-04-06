using System.Collections;
using SecretHistories.Assets.Scripts.Application.Abstract;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    public class WalkableLadder : WalkablePathObject
    {

        // Use this for initialization
        public override bool TokenAllowedHere(Token token)
        {

            if (token.OccupiesSpaceAs() == OccupiesSpaceAs.Someone)
                return true;

            return false;


        }
    }
    
}