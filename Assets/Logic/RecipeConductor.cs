using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Logic;
using Noon;
using UnityEngine;

namespace Assets.Core
{
    public class RecipeConductor
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

            if (!recipe.Linked.Any())
                return null;

            foreach (var lr in recipe.Linked)
            {
                if (lr.Additional)
                    throw new NotImplementedException(
                        lr.Id +
                        " is marked as an additional linked recipe, but we haven't worked out what to do with additional linked recipes yet");

                Recipe candidateRecipe = compendium.GetEntityById<Recipe>(lr.Id);

                if (candidateRecipe == null)
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Tried to link to a nonexistent recipe with id " + lr.Id);
                }
                else if (!candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider))
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Couldn't satisfy requirements for " + lr.Id + " so won't link to it.");
                }
                else if (currentCharacter.HasExhaustedRecipe(candidateRecipe))
                {
                    NoonUtility.Log(recipe.Id + " says: " + lr.Id + " has been exhausted, so won't execute");
                }
                else
                {

                    if (lr.ShouldAlwaysSucceed())
                    {
                        NoonUtility.Log(recipe.Id + " says: " + lr.Id +
                                        " is a suitable linked recipe no chance or challenges specified. Executing it next.");
                        return candidateRecipe;

                    }

                    ChallengeArbiter challengeArbiter=new ChallengeArbiter(aspectsToConsider,lr);
                    
                    int diceResult = dice.Rolld100(recipe);

                    if (diceResult > challengeArbiter.GetArbitratedChance())
                    {
                        NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " + challengeArbiter.GetArbitratedChance() +
                                        " for linked recipe " + lr.Id + "; will try to execute next linked recipe");
                    }
                    else
                    {
                        NoonUtility.Log(recipe.Id + " says: " + lr.Id + " is a suitable linked recipe with dice result " + diceResult + ", against chance " + +challengeArbiter.GetArbitratedChance() + ". Executing it next.");
                        return candidateRecipe;
                    }
                }
            }

            NoonUtility.Log(recipe.Id + " says: " + "No suitable linked recipe found");

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
   //     public RecipePrediction GetPredictionForFollowupRecipe(Recipe currentRecipe)
   //     {
   //         var rp=new RecipePrediction();

   ////set this up to return if we pass through the list below without finding anything interesting.
   //             rp.Title = currentRecipe.Label;
   //             rp.DescriptiveText = currentRecipe.StartDescription;
   //         rp.SignalEndingFlavour = currentRecipe.SignalEndingFlavour;


   //         foreach (var ar in currentRecipe.Alt)
   //         {
   //             Recipe candidateRecipe = compendium.GetEntityById<Recipe>(ar.Id);

   //             if (candidateRecipe == null)
   //             {
   //                 rp.Title = "Recipe predictor couldn't find recipe with id " + ar.Id;
   //                 return rp;
   //             }
   //             if (candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider) &&
   //                 !currentCharacter.HasExhaustedRecipe(candidateRecipe))

   //             {
   //                 if (!ar.Additional)
   //                 {
   //                     if (ar.ShouldAlwaysSucceed())
   //                     {
   //                         //we have a candidate which will execute instead. NB we don't recurse - we assume the first level
   //                         //alternative will have a useful description.
   //                         rp.Title = candidateRecipe.Label;
   //                         rp.DescriptiveText = candidateRecipe.StartDescription;
   //                         rp.BurnImage = candidateRecipe.BurnImage;
   //                         rp.SignalEndingFlavour = candidateRecipe.SignalEndingFlavour;
   //                         //we are not in the additional branch, so just return this prediction.
   //                         return rp;
   //                     }
                        
   //                     // Print a warning when we encounter a non-certain alternative recipe with a start description
   //                     // and no description, since its text won't get displayed.
   //                     var arRecipe = compendium.GetEntityById<Recipe>(ar.Id);
   //                     if (!string.IsNullOrEmpty(arRecipe.StartDescription) && string.IsNullOrEmpty(arRecipe.Description))
   //                         Debug.LogWarning(
   //                             $"Recipe {ar.Id} should not be listed as an alternative recipe for {currentRecipe.Id}" +
   //                             " since it has a chance of failure, a start description and no final description");
   //                 }
   //             }
   //         }


   //         return rp;
   //     }

        public IList<RecipeExecutionCommand> GetActualRecipesToExecute(Recipe recipe)
        {
            IList<RecipeExecutionCommand> recipeExecutionCommands = new List<RecipeExecutionCommand>() {new RecipeExecutionCommand(recipe,null) }; ;
            if (recipe.Alt.Count == 0)
                return recipeExecutionCommands;


            foreach (var ar in recipe.Alt)
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
                    Recipe candidateRecipe = compendium.GetEntityById<Recipe>(ar.Id);

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
