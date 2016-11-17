using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Core
{
    public interface IRecipeConductor
    {
        IList<Recipe> GetNextRecipes(Recipe recipe);
        /// <summary>
        ///Determines whether the original recipe, an alternative, or something else should actually be run.
        /// Alternative recipes which match requirements on elements possessed and % chance are run in place of the original recipe.
        /// Alternatives which match, but which specify additional are run after the original recipe.
        /// There may be multiple additional alternatives.
        /// However, if an alternative ever does *not* specify additional, it replaces the entire list (although it may have alternatives of its own)
        /// Alternatives are recursive, and may have additionals of their own.
        /// A non-additional alternative always takes precedence over everything earlier; if a recursive alternative has additionals of its own, they'll replace everything earlier in the execution sequence.
        /// </summary>

        /// <returns> this may be the original recipe, or it may be an alternative recipe, it may be any number of recipes possible including the original</returns>
        IList<Recipe> GetActualRecipesToExecute(Recipe recipe);
    }

    public class RecipeConductor : IRecipeConductor
    {
        private ICompendium compendium;
        private IElementStacksGateway stacksToConsider;
        private IDice dice;

        public RecipeConductor(ICompendium c,IElementStacksGateway g,IDice d)
        {
            compendium = c;
            stacksToConsider = g;
            dice = d;
        }

        public IList<Recipe> GetNextRecipes(Recipe recipe)
        {
            IList<Recipe> recipes=new List<Recipe>();
            if (recipe.Loop != null)
                recipes.Add(compendium.GetRecipeById(recipe.Loop));

            return recipes;
        }


        public IList<Recipe> GetActualRecipesToExecute(Recipe recipe)
        {
            IList<Recipe> actualRecipesToExecute = new List<Recipe>() { recipe }; ;
            if (recipe.AlternativeRecipes.Count == 0)
                return actualRecipesToExecute;


            foreach (var ar in recipe.AlternativeRecipes)
            {
                int diceResult = dice.Rolld100();
                if (diceResult <= ar.Chance)
                {
                    Recipe candidateRecipe = compendium.GetRecipeById(ar.Id);
                    if (candidateRecipeRequirementsAreSatisfied(candidateRecipe))
                    {
                        if (ar.Additional)
                            actualRecipesToExecute.Add(candidateRecipe); //add the additional recipe, and keep going
                        else
                        {
                            IList<Recipe> recursiveRange = GetActualRecipesToExecute(candidateRecipe);//check if this recipe has any substitutes in turn, and then

                            return recursiveRange;//this recipe, or its further alternatives, supersede(s) everything else! return it.
                        }
                    }
                }
            }

            return actualRecipesToExecute; //we either found no matching candidates and are returning the original, or we added one or more additional recipes to the list
        }


        private bool candidateRecipeRequirementsAreSatisfied(Recipe candidateRecipe)
        {
            //must be satisfied by concrete elements in possession, not by aspects (tho this may some day change)
            foreach (var req in candidateRecipe.Requirements)
            {
                if (req.Value == -1) //req -1 means there must be none of the element
                {
                    if (stacksToConsider.GetCurrentElementQuantity(req.Key) > 0)
                        return false;
                }
                else if (!(stacksToConsider.GetCurrentElementQuantity(req.Key) >= req.Value))
                {
                    //req >0 means there must be >=req of the element
                    return false;
                }
            }
            return true;
        }
    }
}
