using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using OrbCreationExtensions;


public class RecipeCompendium
{
    private List<Recipe> _recipes;

    public RecipeCompendium(List<Recipe> allRecipes )
    {
        _recipes = allRecipes;
    }

    public Recipe GetFirstRecipeForAspectsWithVerb(Dictionary<string, int> aspects,string verb)
    {
        //for each recipe,
        foreach (var recipe in _recipes.Where(r=>r.ActionId==verb))
        {
            //for each requirement in recipe, check if that aspect does *not* exist at that level in Aspects
            bool matches = true;
            foreach (string requirementId in recipe.Requirements.Keys)
            {
                if (!aspects.Any(a => a.Key == requirementId && a.Value >= recipe.Requirements[requirementId]))
                    matches = false;
            }
            //if none fail, return that recipe
            if (matches) return recipe;
            //if any fail, continue
        }

        return null;
    }

    public List<Recipe> GetAllRecipesAsList()
    {
        return _recipes;
    }

}
