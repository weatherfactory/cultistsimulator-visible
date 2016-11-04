using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noon;
using UnityEngine.Assertions;


public class InteractiveRecipeSituation: BaseRecipeSituation
    {
    private  IElementsContainer InternalContainer;

    public InteractiveRecipeSituation(Recipe recipe, float? timeremaining, IElementsContainer inputContainer, Compendium rc) : base(recipe, timeremaining, inputContainer, rc)
        {
        InternalContainer = new SituationElementContainer();
        CharacterContainer = inputContainer;
        Dictionary<string, int> potentiallyPersistableIngredients = inputContainer.GetOutputElements();
        if (potentiallyPersistableIngredients != null) //if we have any suitable ingredients
            AddIngredientsToInternalContainer(currentRecipe, potentiallyPersistableIngredients);
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
            _compendium.GetActualRecipesToExecute(currentRecipe, InternalContainer);
        
        foreach (Recipe executingRecipe in recipesToExecute)
        {
            executingRecipe.Do(InternalContainer);
            RetrieveContentsToCharacterAsPermitted(executingRecipe);
        }
        if (recipesToExecute[0].Id != currentRecipe.Id)
                return recipesToExecute[0];
            else
                return currentRecipe;
       
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
            populateElementsInSituationInfo(info);
               base.publishUpdate(info);
        }

    private void populateElementsInSituationInfo(SituationInfo info)
    {
            Dictionary<string, int> inSituation = InternalContainer.GetAllCurrentElements();
            foreach (string k in inSituation.Keys)
                info.ElementsInSituation.Add(k, inSituation[k]);
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

