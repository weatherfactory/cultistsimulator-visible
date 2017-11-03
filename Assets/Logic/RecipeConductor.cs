using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Core
{
    public interface IRecipeConductor
    {
        Recipe GetLinkedRecipe(Recipe recipe);
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

        RecipePrediction GetRecipePrediction(Recipe currentRecipe);
    }

    public class RecipeConductor : IRecipeConductor
    {
        private ICompendium compendium;
        private IAspectsDictionary aspectsToConsider;
        private IDice dice;
        private Character currentCharacter;

        public RecipeConductor(ICompendium c,IAspectsDictionary a,IDice d,Character character)
        {
            compendium = c;
            aspectsToConsider = a;
            dice = d;
            currentCharacter = character;
        }

        /// <summary>
        /// If linked recipes exist for this recipe:
        /// - check its requirements are satisfied by the situation
        /// - check it is not exhausted
        /// - check the chance is satisfied
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public Recipe GetLinkedRecipe(Recipe recipe)
        {

            if (!recipe.LinkedRecipes.Any())
                return null;

            foreach (var ar in recipe.LinkedRecipes)
            {
                if (ar.Additional)
                    throw new NotImplementedException(
                        ar.Id +
                        " is marked as an additional linked recipe, but we haven't worked out what to do with additional linked recipes yet");

                Recipe candidateRecipe = compendium.GetRecipeById(ar.Id);

                if (candidateRecipe == null)
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Tried to link to a nonexistent recipe with id " + ar.Id);
                }
                else if (!candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider))
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Couldn't satisfy requirements for " + ar.Id + " so won't link to it.");
                }
                else if (currentCharacter.HasExhaustedRecipe(candidateRecipe))
                {
                    NoonUtility.Log(recipe.Id + " says: " + ar.Id + " has been exhausted, so won't execute");   
                }
            else
                {
                    int diceResult = dice.Rolld100();

                    if (diceResult > ar.Chance)
                    {
                        NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " + ar.Chance +
                                        " for linked recipe " + ar.Id + "; will try to execute next linked recipe");
                    }
                    else
                    {
                        NoonUtility.Log(recipe.Id + " says: " + ar.Id + " is a suitable linked recipe! Executing it next.");
                        return candidateRecipe;
                    }
                }
            }

            NoonUtility.Log(recipe.Id + " says: " + "No suitable linked recipe found");

            return null;
        }


        public RecipePrediction GetRecipePrediction(Recipe currentRecipe)
        {
            var rp=new RecipePrediction();
           
   //set this up to return if we pass through the list below without finding anything interesting.
                rp.Title = currentRecipe.Label;
                rp.DescriptiveText = currentRecipe.StartDescription;
                rp.Commentary = currentRecipe.Aside;
   
                
            foreach (var ar in currentRecipe.AlternativeRecipes)
            {
                Recipe candidateRecipe = compendium.GetRecipeById(ar.Id);

                if (candidateRecipe == null)
                { 
                    rp.Title = "Recipe predictor couldn't find recipe with id " + ar.Id;
                    return rp;
                }
                if (candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider) &&
                    !currentCharacter.HasExhaustedRecipe(candidateRecipe))

                {
                    if (!ar.Additional)
                    {
                        if (ar.Chance < 100)
                        {
                            //there is uncertainty! Leave the headline decision where it is, but tell the player.
                            rp.Commentary = "[The outcome here may vary.]";
                            return rp;
                        }
                        else
                        {
                            //we have a candidate which will execute instead. NB we don't recurse - we assume the first level
                            //alternative will have a useful description.
                            rp.Title = candidateRecipe.Label;
                            rp.DescriptiveText = candidateRecipe.StartDescription;
                            rp.Commentary = candidateRecipe.Aside;
                            rp.BurnImage = candidateRecipe.BurnImage;
                            return rp;
                        }
                    }
                }
            }


            return rp;
        }

        public IList<Recipe> GetActualRecipesToExecute(Recipe recipe)
        {
            IList<Recipe> actualRecipesToExecute = new List<Recipe>() { recipe }; ;
            if (recipe.AlternativeRecipes.Count == 0)
                return actualRecipesToExecute;


            foreach (var ar in recipe.AlternativeRecipes)
            {
                int diceResult = dice.Rolld100();
                if (diceResult > ar.Chance)
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " +
                                    ar.Chance +
                                    " for alternative recipe " + ar.Id +
                                    "; will try to execute next alternative recipe");
                }
                else
                {
                    Recipe candidateRecipe = compendium.GetRecipeById(ar.Id);

                    if (!candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider))
                    { 
                        NoonUtility.Log(recipe.Id + " says: couldn't satisfy requirements for " + ar.Id);
                        continue;
                    }
                    if (currentCharacter.HasExhaustedRecipe(candidateRecipe))
                    { 
                        NoonUtility.Log(recipe.Id + " says: already exhausted " + ar.Id);
                        continue;
                    }
                    if (ar.Additional)
                    {
                        actualRecipesToExecute.Add(candidateRecipe); //add the additional recipe, and keep going
                        NoonUtility.Log(recipe.Id + " says: Found additional recipe " + ar.Id +
                                        " to execute - adding it to executiion listand looking for more");
                    }
                    else
                    {
                        IList<Recipe>
                            recursiveRange =
                                GetActualRecipesToExecute(
                                    candidateRecipe); //check if this recipe has any substitutes in turn, and then

                        string logmessage =
                            recipe.Id + " says: reached the bottom of the execution list: returning ";
                        foreach (var r in recursiveRange)
                            logmessage += r.Id + "; ";
                        NoonUtility.Log(logmessage);

                        return
                            recursiveRange; //this recipe, or its further alternatives, supersede(s) everything else! return it.
                    }
                }
            }

            return actualRecipesToExecute; //we either found no matching candidates and are returning the original, or we added one or more additional recipes to the list
        }

    }
}
