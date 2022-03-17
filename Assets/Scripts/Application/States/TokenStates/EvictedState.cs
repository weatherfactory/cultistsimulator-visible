using System.Collections;
using UnityEngine;

namespace SecretHistories.States.TokenStates
{
    public class EvictedState : AbstractTokenState
    {
        public override bool Docked()
        {
            return false;
        }

        public override bool InPlayerDrivenMotion()
        {
            return false;
        }

        public override bool InSystemDrivenMotion()
        {
            return false;
        }

        public override bool CanDecay()
        {
            return true;
        }
    }
}