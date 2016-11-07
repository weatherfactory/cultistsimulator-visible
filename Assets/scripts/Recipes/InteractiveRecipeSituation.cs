using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noon;
using UnityEngine.Assertions;

/// <summary>
/// InteractiveRecipeSituations: these are what we were calling 'Situations' in the early UI discussion.
/// A RecipeSituation is the biz object that handles an ongoing recipe with a timer.
/// An InteractiveRecipeSituation:
/// - can persist some elements consumed from the workspace when the recipe is queued: 'You've started an expedition! I'll hang on to all these Followers, we may get them back later'
/// - executes its recipes against *those* elements, not against stockpile elements: 'one of these followers has died, but they've found these Progress elements, but those are still in the recipe
/// situation, not in your stockpile'
/// - can retrieve some of those elements when it's done: 'The expedition has finished! Here are your followers back in your stockpile (but the Progress elements have just been thrown away now we've finished')
/// - has a slot which can accept input of additional elements while the recipes inside the InteractiveSituation are looping (this is not implemented! it's what I'm working on now)
/// </summary>
public class InteractiveRecipeSituation: BaseRecipeSituation
    {
    private  IElementsContainer InternalContainer;

    public InteractiveRecipeSituation(Recipe recipe, float? timeremaining, IElementsContainer inputContainer, Compendium rc) : base(recipe, timeremaining, inputContainer, rc)
        {
        InternalContainer = new SituationElementContainer();
        CharacterContainer = inputContainer;
        Dictionary<string, int> potentiallyPersistableIngredients = inputContainer.GetOutputElements();
        if (potentiallyPersistableIngredients != null) //if we have any suitable ingredients
            AddIngredientsToInternalContainer(CurrentRecipe, potentiallyPersistableIngredients);
    }

        public override int GetInternalElementQuantity(string forElementId)
        {
            return InternalContainer.GetCurrentElementQuantity(forElementId);
    }

        protected override Recipe ProcessRecipe()
        {
        //find alternative recipe(s) - there may be additional recipes
        //nb we check on the content elements of this situation, not the character's elements
        List<Recipe> recipesToExecute =
            _compendium.GetActualRecipesToExecute(CurrentRecipe, InternalContainer);
        
        foreach (Recipe executingRecipe in recipesToExecute)
        {
            executingRecipe.Do(InternalContainer);
            RetrieveContentsToCharacterAsPermitted(executingRecipe);
        }
        if (recipesToExecute[0].Id != CurrentRecipe.Id)
                return recipesToExecute[0];
            else
                return CurrentRecipe;
       
        }

        private void RetrieveContentsToCharacterAsPermitted(Recipe executingRecipe)
        {
            if (executingRecipe.RetrievesContents())
            {
                Dictionary<string, int> retrieved =
                    NoonUtility.AspectMatchFilter(executingRecipe.RetrievesContentsWith,
                        InternalContainer.GetAllCurrentElements(), _compendium);

                foreach (string k in retrieved.Keys)
                    CharacterContainer.ModifyElementQuantity(k, retrieved[k]);
            }
        }

        public override bool IsInteractive()
        {
            return true;
        }

        protected override void publishUpdate(SituationInfo info)
        {
            AddInteractiveElementsToSituationInfo(info);
               base.publishUpdate(info);
        }

    private void AddInteractiveElementsToSituationInfo(SituationInfo info)
    {
            Dictionary<string, int> inSituation = InternalContainer.GetAllCurrentElements();
            foreach (string k in inSituation.Keys)
                info.DisplayElementsInSituation.Add(k, inSituation[k]);

            info.DisplayChildSlotSpecifications.AddRange(CurrentRecipe.ChildSlotSpecifications);
    }

    private void AddIngredientsToInternalContainer(Recipe recipe, Dictionary<string, int> possiblyPersisted)
    {
        //dammit, can't just use the aspects match, because we also want to exclude elements with slots (which aren't consumed)
        foreach (var k in possiblyPersisted.Keys)
        {
            if (shouldPersistInInternalContainer(k, recipe))
                InternalContainer.ModifyElementQuantity(k, possiblyPersisted[k]);
        }
    }

    private bool shouldPersistInInternalContainer(string elementId, Recipe recipe)
    {

        Element eToCheck = _compendium.GetElementById(elementId);
        Assert.IsNotNull(eToCheck, "invalid element id " + " checked in isPermittedByAspectFilter");

        if (eToCheck.ChildSlotSpecifications.Count > 0)
            return false;

        foreach (string aspectFilterId in recipe.PersistsIngredientsWith.Keys)
        {
            if (!eToCheck.AspectsIncludingSelf.ContainsKey(aspectFilterId))
                return false;
        }

        return true;
    }


}

