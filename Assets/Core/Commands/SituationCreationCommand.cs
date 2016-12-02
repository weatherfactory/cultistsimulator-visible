using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Commands
{
    public class SituationCreationCommand
    {
        public Recipe Recipe { get; set; }
        public SituationState? State { get; set; }
        public int? TimeRemaining { get; set; }

        public SituationCreationCommand(Recipe recipe)
        {
            Recipe = recipe;
        }
    }
}
