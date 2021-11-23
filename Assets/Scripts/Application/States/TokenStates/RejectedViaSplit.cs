using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.States.TokenStates
{
    /// <summary>
    ///eg: we dropped a stack of >1 elements on a sphere which can only accept one of them. This state applies to the stack remainder that's returned
    /// </summary>
   public class RejectedViaSplit: AbstractTokenState
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
