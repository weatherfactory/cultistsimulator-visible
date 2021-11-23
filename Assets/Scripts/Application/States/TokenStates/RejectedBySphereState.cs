using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.States.TokenStates
{
   public class RejectedBySphereState: AbstractTokenState
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
