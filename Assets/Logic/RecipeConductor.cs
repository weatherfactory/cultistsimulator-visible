using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.Logic;
using Noon;
using UnityEngine;

namespace Assets.Core
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

                Recipe candidateRecipe = Registry.Get<ICompendium>().GetEntityById<Recipe>(lr.Id);

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
                                        " is a suitable linked recipe no chance or challenges specified. Executing it next.");
                        return candidateRecipe;

                    }

                    ChallengeArbiter challengeArbiter=new ChallengeArbiter(_aspectsInContext,lr);
                    
                    int diceResult = Registry.Get<IDice>().Rolld100(currentRecipe);

                    if (diceResult > challengeArbiter.GetArbitratedChance())
                    {
                        NoonUtility.Log(currentRecipe.Id + " says: " + "Dice result " + diceResult + ", against chance " + challengeArbiter.GetArbitratedChance() +
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
        /// Returns information on the recipe that's going to execute, based on current recipe and aspect context
        public RecipePrediction GetPredictionForFollowupRecipe(Recipe currentRecipe, SituationState situationState, IVerb verb)
        {

            
            //returns, in order: craftable non-hint recipes; hint recipes; null recipe (which might be verb-description-based)
            _aspectsInContext.ThrowErrorIfNotPopulated(verb.Id);

            //note: we *either* get craftable recipes *or* if we're getting hint recipes we don't care if they're craftable
            var _recipes = Registry.Get<ICompendium>().GetEntitiesAsList<Recipe>();
            List<Recipe> candidateRecipes = _recipes.Where(r => r.CanExecuteInContext(currentRecipe, situationState)).ToList();
            List<Recipe> nonExhaustedCandidateRecipes =
                candidateRecipes.Where(r => !_character.HasExhaustedRecipe(r)).ToList();
            
            var orderedCandidateRecipes = nonExhaustedCandidateRecipes.OrderByDescending(r => r.Priority);
            
            foreach (var candidateRecipe in orderedCandidateRecipes)
            {
                if (candidateRecipe.RequirementsSatisfiedBy(_aspectsInContext))
                    return new RecipePrediction(candidateRecipe, _aspectsInContext.AspectsInSituation);
            }

         
            return new RecipePrediction(currentRecipe, _aspectsInContext.AspectsInSituation);
        }

        public IList<RecipeExecutionCommand> GetActualRecipesToExecute(Recipe recipe)
        {
            IList<RecipeExecutionCommand> recipeExecutionCommands = new List<RecipeExecutionCommand>() {new RecipeExecutionCommand(recipe,null) }; ;
            if (recipe.Alt.Count == 0)
                return recipeExecutionCommands;


            foreach (var ar in recipe.Alt)
            {


                ChallengeArbiter challengeArbiter = new ChallengeArbiter(_aspectsInContext, ar);

                int diceResult = Registry.Get<IDice>().Rolld100(recipe);
                if (diceResult > challengeArbiter.GetArbitratedChance()) //BUT NOTE: Challenges always seem to fail on alternative recipes at the mo - though they're working fine on linked recipes.
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " +
                                    challengeArbiter.GetArbitratedChance() +
                                    " for alternative recipe " + ar.Id +
                                    "; will try to execute next alternative recipe");
                }
                else
                {
                    Recipe candidateRecipe = Registry.Get<ICompendium>().GetEntityById<Recipe>(ar.Id);

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
