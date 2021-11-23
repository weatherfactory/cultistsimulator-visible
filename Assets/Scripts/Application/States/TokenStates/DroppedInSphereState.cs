using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.States.TokenStates
{
    public class DroppedInSphereState: AbstractTokenState
    {

        public override bool Docked()
        {
            return true;
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
