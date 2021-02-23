using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.States;

namespace SecretHistories.Commands
{


    public enum CommandCategory
    {
        All,
        Anchor,
        VerbThresholds,
        RecipeThresholds,
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
