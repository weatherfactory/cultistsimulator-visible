using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;

namespace SecretHistories.States.TokenStates

{
    public  class PlacedAssertivelyBySystemState: AbstractTokenState
    {
        public override bool Docked()
        {
            return true;
        }

        public override bool InPlayerDrivenMotion(Token token)
        {
            return false;
        }

        public override bool InSystemDrivenMotion(Token token)
        {
            return false;
        }

        public override bool CanDecay(Token token)
        {
            return true;
        }
    }
}
