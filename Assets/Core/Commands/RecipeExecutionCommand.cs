using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Commands
{
    public class RecipeExecutionCommand
    {
        public Recipe Recipe { get; set; }
        public Expulsion Expulsion { get; set; }

        public RecipeExecutionCommand(Recipe recipe, Expulsion expulsion)
        {
            Recipe = recipe;
            Expulsion = expulsion;
        }
    }
}
