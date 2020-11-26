using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Fucine;

namespace Assets.Core.Commands
{
    public class RecipeExecutionCommand
    {
        public Recipe Recipe { get; set; }
        public Expulsion Expulsion { get; set; }
        public SpherePath ToPath { get; set; }

        public RecipeExecutionCommand(Recipe recipe, Expulsion expulsion,SpherePath toPath)
        {
            Recipe = recipe;
            Expulsion = expulsion;
            ToPath = toPath;
        }
    }
}
