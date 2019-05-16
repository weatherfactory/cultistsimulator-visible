using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Logic;
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
        IList<RecipeExecutionCommand> GetActualRecipesToExecute(Recipe recipe);

        RecipePrediction GetRecipePrediction(Recipe currentRecipe);
    }

    public class RecipeConductor : IRecipeConductor
    {
        private ICompendium compendium;
        private AspectsInContext aspectsToConsider;
        private IDice dice;
        private Character currentCharacter;

        public RecipeConductor(ICompendium c,AspectsInContext a,IDice d,Character character)
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

            foreach (var lr in recipe.LinkedRecipes)
            {
                if (lr.Additional)
                    throw new NotImplementedException(
                        lr.Id +
                        " is marked as an additional linked recipe, but we haven't worked out what to do with additional linked recipes yet");

                Recipe candidateRecipe = compendium.GetRecipeById(lr.Id);

                if (candidateRecipe == null)
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Tried to link to a nonexistent recipe with id " + lr.Id,1);
                }
                else if (!candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider))
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Couldn't satisfy requirements for " + lr.Id + " so won't link to it.",5);
                }
                else if (currentCharacter.HasExhaustedRecipe(candidateRecipe))
                {
                    NoonUtility.Log(recipe.Id + " says: " + lr.Id + " has been exhausted, so won't execute", 5);
                }
            else
                {

                    if (lr.Chance >= 100)
                    {
                        NoonUtility.Log(recipe.Id + " says: " + lr.Id + " is a suitable linked recipe with chance >=100! Executing it next.", 5);
                        return candidateRecipe;

                    }

                    ChallengeArbiter challengeArbiter=new ChallengeArbiter(aspectsToConsider,lr);
                    
                    int diceResult = dice.Rolld100(recipe);

                    if (diceResult > challengeArbiter.GetArbitratedChance())
                    {
                        NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " + challengeArbiter.GetArbitratedChance() +
                                        " for linked recipe " + lr.Id + "; will try to execute next linked recipe", 5);
                    }
                    else
                    {
                        NoonUtility.Log(recipe.Id + " says: " + lr.Id + " is a suitable linked recipe with dice result " + diceResult + ", against chance " + +challengeArbiter.GetArbitratedChance() + ". Executing it next.", 5);
                        return candidateRecipe;
                    }
                }
            }

            NoonUtility.Log(recipe.Id + " says: " + "No suitable linked recipe found", 5);

            return null;
        }

        /// <summary>
        /// Returns information on the recipe that's going to execute, based on current recipe and aspect context
        ///  If there are no alternate recipes, we return the current recipe
        /// If there is a 100% alternate recipe, we return that
        /// If there is a chance of an alternate recipe, we return *that* with basic chance description
        /// If there's more than one, we return all those
        /// </summary>
        /// <param name="currentRecipe"></param>
        /// <returns></returns>
        public RecipePrediction GetRecipePrediction(Recipe currentRecipe)
        {
            var rp=new RecipePrediction();

   //set this up to return if we pass through the list below without finding anything interesting.
                rp.Title = currentRecipe.Label;
                rp.DescriptiveText = currentRecipe.StartDescription;
            rp.SignalEndingFlavour = currentRecipe.SignalEndingFlavour;


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
                    if (!ar.Additional && ar.Chance>=100)
                    {
                            //we have a candidate which will execute instead. NB we don't recurse - we assume the first level
                            //alternative will have a useful description.
                            rp.Title = candidateRecipe.Label;
                            rp.DescriptiveText = candidateRecipe.StartDescription;
                            rp.BurnImage = candidateRecipe.BurnImage;
                        rp.SignalEndingFlavour = candidateRecipe.SignalEndingFlavour;
                        //we are not in the additional branch, so just return this predictioin.
                        return rp;
                    }
                }
            }


            return rp;
        }

        public IList<RecipeExecutionCommand> GetActualRecipesToExecute(Recipe recipe)
        {
            IList<RecipeExecutionCommand> recipeExecutionCommands = new List<RecipeExecutionCommand>() {new RecipeExecutionCommand(recipe,null) }; ;
            if (recipe.AlternativeRecipes.Count == 0)
                return recipeExecutionCommands;


            foreach (var ar in recipe.AlternativeRecipes)
            {


                ChallengeArbiter challengeArbiter = new ChallengeArbiter(aspectsToConsider, ar);

                int diceResult = dice.Rolld100(recipe);
                if (diceResult > challengeArbiter.GetArbitratedChance()) //BUT NOTE: Challenges always seem to fail on alternative recipes at the mo - though they're working fine on linked recipes.
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " +
                                    challengeArbiter.GetArbitratedChance() +
                                    " for alternative recipe " + ar.Id +
                                    "; will try to execute next alternative recipe");
                }
                else
                {
                    Recipe candidateRecipe = compendium.GetRecipeById(ar.Id);

                    if (!candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider))
                    {
                        NoonUtility.Log(recipe.Id + " says: couldn't satisfy requirements for " + ar.Id,5);
                        continue;
                    }
                    if (currentCharacter.HasExhaustedRecipe(candidateRecipe))
                    {
                        NoonUtility.Log(recipe.Id + " says: already exhausted " + ar.Id,5);
                        continue;
                    }
                    if (ar.Additional)
                    {
                        recipeExecutionCommands.Add(new RecipeExecutionCommand(candidateRecipe,ar.Expulsion)); //add the additional recipe, and keep going
                        NoonUtility.Log(recipe.Id + " says: Found additional recipe with dice result " + diceResult + ", against chance " + +challengeArbiter.GetArbitratedChance()  + ar.Id +
                                        " to execute - adding it to execution list and looking for more");
                    }
                    else
                    {
                        IList<RecipeExecutionCommand>
                            recursiveRange =
                                GetActualRecipesToExecute(candidateRecipe); //check if this recipe has any substitutes in turn, and then

                        string logmessage =
                            recipe.Id + " says: reached the bottom of the execution list: returning ";
                        foreach (var r in recursiveRange)
                            logmessage += r.Recipe.Id + "; ";
                        NoonUtility.Log(logmessage);

                        return
                            recursiveRange; //this recipe, or its further alternatives, supersede(s) everything else! return it.
                    }
                }
            }

            return recipeExecutionCommands; //we either found no matching candidates and are returning the original, or we added one or more additional recipes to the list
        }

    }
}
