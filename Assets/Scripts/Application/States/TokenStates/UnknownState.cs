using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;

namespace SecretHistories.States
{
    public class UnknownState: AbstractTokenState
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
            return false;

        }
    }
}
