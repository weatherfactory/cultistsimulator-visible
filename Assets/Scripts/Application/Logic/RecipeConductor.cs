using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using Assets.Logic;

using UnityEngine;

namespace SecretHistories.Core
{
    public class RecipeConductor
    {
        private readonly AspectsInContext _aspectsInContext;
        private readonly Character _character;

        
        public RecipeConductor(AspectsInContext aspectsInContext, Character character)
        {
            _aspectsInContext = aspectsInContext;
            _character = character;
        }

        /// <summary>
        /// If linked recipes exist for this recipe:
        /// - check its requirements are satisfied by the situation
        /// - check it is not exhausted
        /// - check the chance is satisfied
        /// </summary>
        /// <param name="currentRecipe"></param>
        /// <returns></returns>
        public Recipe GetLinkedRecipe(Recipe currentRecipe)
        {

            if (!currentRecipe.Linked.Any())
                return null;

            foreach (var lr in currentRecipe.Linked)
            {
                if (lr.Additional)
                    throw new NotImplementedException(
                        lr.Id +
                        " is marked as an additional linked recipe, but we haven't worked out what to do with additional linked recipes yet");


                Recipe candidateRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(lr.Id);

                if (!candidateRecipe.IsValid())
                {
                    NoonUtility.Log($"Candidate linked recipe ID {lr.Id} isn't valid, which might mean it's a nullrecipe returned when we couldn't find the linked ID. Logging and continuing.",1,VerbosityLevel.Significants);
                }

                if (candidateRecipe == null)
                {
                    NoonUtility.Log(currentRecipe.Id + " says: " + "Tried to link to a nonexistent recipe with id " + lr.Id);
                }
                else if (!candidateRecipe.RequirementsSatisfiedBy(_aspectsInContext))
                {
                    NoonUtility.Log(currentRecipe.Id + " says: " + "Couldn't satisfy requirements for " + lr.Id + " so won't link to it.");
                }
                else if (_character.HasExhaustedRecipe(candidateRecipe))
                {
                    NoonUtility.Log(currentRecipe.Id + " says: " + lr.Id + " has been exhausted, so won't execute");
                }
                else
                {

                    if (lr.ShouldAlwaysSucceed())
                    {
                        NoonUtility.Log(currentRecipe.Id + " says: " + lr.Id +
                                        " is a suitable linked recipe with no chance or challenges specified. Executing it next.");
                        return candidateRecipe;

                    }

                    ChallengeArbiter challengeArbiter=new ChallengeArbiter(_aspectsInContext,lr);
                    
                    int diceResult = Watchman.Get<IDice>().Rolld100(currentRecipe);
                    int arbitratedChance = challengeArbiter.GetArbitratedChance();
                    if (diceResult > arbitratedChance)
                    {
                        NoonUtility.Log(currentRecipe.Id + " says: " + "Dice result " + diceResult + ", against chance " + arbitratedChance +
                                        " for linked recipe " + lr.Id + "; will try to execute next linked recipe");
                    }
                    else
                    {
                        NoonUtility.Log(currentRecipe.Id + " says: " + lr.Id + " is a suitable linked recipe with dice result " + diceResult + ", against chance " + +challengeArbiter.GetArbitratedChance() + ". Executing it next.");
                        return candidateRecipe;
                    }
                }
            }

            NoonUtility.Log(currentRecipe.Id + " says: " + "No suitable linked recipe found");

            return null;
        }

        /// <summary>
        /// Returns information on the recipe that's actually going to execute, based on current recipe and aspect context
        /// This might be an alt or linked recipe depending on state context
        public RecipePrediction GetPredictionForFollowupRecipe(Recipe currentlyPredictedRecipe, Situation situation)
        {
            _aspectsInContext.ThrowErrorIfNotPopulated(situation.Verb.Id);


            var candidateRecipes = situation.State.PotentiallyPredictableRecipesForState(situation);
            List<Recipe> nonExhaustedCandidateRecipes =
                candidateRecipes.Where(r => !_character.HasExhaustedRecipe(r)).ToList();


            //returns, in order: craftable non-hint recipes; hint recipes / verb-null recipe
            var orderedCandidateRecipes = nonExhaustedCandidateRecipes. OrderByDescending(r => r.Priority);
            
            foreach (var candidateRecipe in orderedCandidateRecipes)
            {
                if (candidateRecipe.RequirementsSatisfiedBy(_aspectsInContext))
                    return new RecipePrediction(candidateRecipe, _aspectsInContext.AspectsInSituation,situation.Verb);
            }

   
            return new RecipePrediction(currentlyPredictedRecipe, _aspectsInContext.AspectsInSituation, situation.Verb);
            
    }

        public IList<AlternateRecipeExecution> GetAlternateRecipes(Recipe recipe)
        {
            //start with the execution command for the original recipe
            //This is because additional recipes will add to it. If we find a non-additional alternate recipe,
            //we will return that recipe instead of this list.
//Which is horrible, but I reckon I did it this way so we could mix additionals and non-additionals in the fallthrough list.
            IList<AlternateRecipeExecution> originalRecipePlusAdditionals = new List<AlternateRecipeExecution>() {new AlternateRecipeExecution(recipe,null,new FucinePath(String.Empty)) }; ;
            if (recipe.Alt.Count == 0)
                return originalRecipePlusAdditionals;


            foreach (var ar in recipe.Alt)
            {


                ChallengeArbiter challengeArbiter = new ChallengeArbiter(_aspectsInContext, ar);

                int diceResult = Watchman.Get<IDice>().Rolld100(recipe);
                if (diceResult > challengeArbiter.GetArbitratedChance()) //BUT NOTE: Challenges always seem to fail on alternative recipes at the mo - though they're working fine on linked recipes.
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " +
                                    challengeArbiter.GetArbitratedChance() +
                                    " for alternative recipe " + ar.Id +
                                    "; will try to execute next alternative recipe");
                }
                else
                {
                    Recipe candidateRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(ar.Id);

                    if (!candidateRecipe.RequirementsSatisfiedBy(_aspectsInContext))
                    {
                        NoonUtility.Log(recipe.Id + " says: couldn't satisfy requirements for " + ar.Id);
                        continue;
                    }
                    if (_character.HasExhaustedRecipe(candidateRecipe))
                    {
                        NoonUtility.Log(recipe.Id + " says: already exhausted " + ar.Id);
                        continue;
                    }
                    if (ar.Additional)
                    {
                        originalRecipePlusAdditionals.Add(new AlternateRecipeExecution(candidateRecipe,ar.Expulsion,ar.ToPath)); //add the additional recipe, and keep going
                        NoonUtility.Log(recipe.Id + " says: Found additional recipe with dice result " + diceResult + ", against chance " + +challengeArbiter.GetArbitratedChance()  + ar.Id +
                                        " to execute - adding it to execution list and looking for more");
                    }
                    else
                    {
                        IList<AlternateRecipeExecution>
                            recursiveRange =
                                GetAlternateRecipes(candidateRecipe); //check if this recipe has any substitutes in turn, and then

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

            return originalRecipePlusAdditionals; //we either found no matching candidates and are returning the original, or we added one or more additional recipes to the list
        }

    }
}
