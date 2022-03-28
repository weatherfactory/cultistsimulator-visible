using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.States;

namespace SecretHistories.Assets.Scripts.Application.States.TokenStates
{
    public class WalkingState: AbstractTokenState
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
            return true;
        }

        public override bool CanDecay()
        {
            return true;
        }

        public override bool ShouldObserveRangeLimits()
        {
            return false;
        }
    }
}
