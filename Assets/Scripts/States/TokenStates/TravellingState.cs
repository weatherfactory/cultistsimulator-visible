using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.States;
using Assets.CS.TabletopUI;

namespace Assets.Scripts.States.TokenStates
{
   public class TravellingState: TokenState
    {
        public override bool Docked(Token token)
        {
            return false;
        }

        public override bool InPlayerDrivenMotion(Token token)
        {
            return false;
        }

        public override bool InSystemDrivenMotion(Token token)
        {
            return true;
        }
    }
}
