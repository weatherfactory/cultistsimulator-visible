using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

namespace Assets.Core
{
    public interface IRecipeConductor
    {
        Recipe GetNextRecipe(Recipe recipe);
    }

    public class RecipeConductor : IRecipeConductor
    {
        private ICompendium compendium;

        public RecipeConductor(ICompendium c)
        {
            compendium = c;
        }

        public Recipe GetNextRecipe(Recipe recipe)
        {
            if (recipe.Loop != null)
                return compendium.GetRecipeById(recipe.Loop);
            else
                return null;
        }
    }
}
