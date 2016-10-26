using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using OrbCreationExtensions;
using UnityEngine.Rendering;


public class RecipeCompendium
{
    private List<Recipe> _recipes;
    private IDice _dice;

    public RecipeCompendium(List<Recipe> allRecipes,IDice dice)
    {
        _recipes = allRecipes;
        _dice = dice;
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

    public Recipe GetRecipeById(string recipeId)
    {
        return _recipes.SingleOrDefault(r => r.Id == recipeId);   
    }

    /// <summary>
    ///Determines whether the original recipe, a substitute, or something else should actually be run
    /// </summary>

    /// <returns> this may be the original recipe, or it may be a substitute recipe, it may be any number of recipes possible including the original</returns>
    public List<Recipe> GetActualRecipesToExecute(Recipe recipe, IElementsContainer elementsContainer)
    {
        List<Recipe> actualRecipesToExecute=new List<Recipe>();
        if (recipe.AlternativeRecipes.Count == 0)
        {
            actualRecipesToExecute.Add(recipe);
            return actualRecipesToExecute;
        }

        foreach (var ar in recipe.AlternativeRecipes)
        {
            int diceResult = _dice.Rolld100();
            if (diceResult<= ar.Chance)
            {
                Recipe candidateRecipe = GetRecipeById(ar.Id);
                if(candidateRecipeRequirementsAreSatisfied(candidateRecipe,elementsContainer))
                { 
                    actualRecipesToExecute.Add(candidateRecipe);
                    return actualRecipesToExecute;
                }
            }
        }

        return new List<Recipe>() {recipe};
    }


    private bool candidateRecipeRequirementsAreSatisfied(Recipe candidateRecipe, IElementsContainer elementsContainer)
    {
        foreach (var req in candidateRecipe.Requirements)
        {
            if (!(elementsContainer.GetCurrentElementQuantity(req.Key) >= req.Value))
                return false;
        }
        return true;
    }
}
