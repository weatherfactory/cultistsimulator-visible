using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.States;

namespace Assets.Core.Commands
{
    public interface ISituationCommand
    {
        bool ExecuteOnState(SituationState state);
    }
}
