using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.States;
using Assets.CS.TabletopUI;

namespace Assets.Scripts.States.TokenStates
{
    /// <summary>
    ///eg: we dropped a stack of >1 elements on a sphere which can only accept one of them. This state applies to the stack remainder that's returned
    /// </summary>
   public class RejectedViaSplit: TokenState
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
            return false;
        }
    }
}
