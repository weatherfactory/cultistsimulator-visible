using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

namespace Assets.Core.States
{
    public abstract class TokenState
    {
        public abstract bool Docked(Token token);
        public abstract bool InPlayerDrivenMotion(Token token);
        public abstract bool InSystemDrivenMotion(Token token);
        public abstract bool CanDecay(Token token);



    }
}
