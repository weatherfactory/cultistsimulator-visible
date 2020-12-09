using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.States;

namespace Assets.Core.Commands
{


    public enum CommandCategory
    {
        Anchor,
        VerbSlots,
        RecipeSlots,
        Timer,
        Storage,
        Output,
        Notes
    }

    public interface ISituationCommand
    {
        CommandCategory CommandCategory { get; }
        bool Execute(Situation situation);
    }
}
