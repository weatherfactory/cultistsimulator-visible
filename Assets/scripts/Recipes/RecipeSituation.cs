using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Assets.scripts.Entities;
using Noon;
using UnityEngine.Assertions;

public class RecipeSituation
{
    private float _timeRemaining;
    private List<IRecipeSituationSubscriber> _subscribers=new List<IRecipeSituationSubscriber>();
    private Compendium _compendium;
    private Recipe currentRecipe;

    public string CurrentRecipeId
    {
        get
        {
            if (currentRecipe == null)
                return null;
            return (currentRecipe.Id);
        }
    }

    private readonly IElementsContainer AffectsElementsContainer;
    ///this is the id of the *originating* recipe, although the recipe inside may change later.
    ///the original recipe may be important for ongoing situations
    public string OriginalRecipeId { get; set; }

    public float TimeRemaining
    {
        get
        {
            if (_timeRemaining < 0)
                return 0;
            return _timeRemaining;
        }
        set { _timeRemaining = value; }
    }

    public RecipeTimerState TimerState
    {
        get
        {
            if(currentRecipe==null)
                return RecipeTimerState.Extinct;
            if (TimeRemaining > 0)
                return RecipeTimerState.Ongoing;
            else
                return RecipeTimerState.Complete;
        }
    }

    /// <summary>
    /// if the situation contains its own elements, return the quantity for this one
    /// </summary>
    public int GetInternalElementQuantity(string forElementId)
    {
        if(AffectsElementsContainer.IsInternal())
            return AffectsElementsContainer.GetCurrentElementQuantity(forElementId);

        return 0;
    }

    public RecipeSituation(Recipe recipe, float? timeremaining,IElementsContainer workspaceContainer,Compendium rc)
    {
        currentRecipe = recipe;
        OriginalRecipeId = currentRecipe.Id;
        _compendium = rc;

        if (timeremaining == null)
            TimeRemaining = currentRecipe.Warmup;
        else
            TimeRemaining = timeremaining.Value;

        if (currentRecipe.PersistsIngredients())
        { 
            AffectsElementsContainer= new SituationElementContainer();
            Dictionary<string, int> potentiallyPersistableIngredients = workspaceContainer.GetOutputElements();
            if(potentiallyPersistableIngredients!=null) //if we have any suitable ingredients
                AddIngredientsToInternalContainer(currentRecipe, potentiallyPersistableIngredients,AffectsElementsContainer);
        }
        else
            AffectsElementsContainer = workspaceContainer;
    }

    public void DoHeartbeat()
    {
        _timeRemaining--;
        if (_timeRemaining <= 0)
        
            Complete();
        else
            Continue();
    }

    private void Continue()
    {
        SituationInfo info = GetCurrentSituationInfo();
        publishUpdate(info);
    }


    public void Extinguish()
    {
        currentRecipe = null;
    }

 
    public void Subscribe(IRecipeSituationSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
        subscriber.ReceiveSituationUpdate(GetCurrentSituationInfo());
    }

    private void publishUpdate(SituationInfo info)
    {
        foreach (var s in _subscribers)
            s.ReceiveSituationUpdate(info);
    }

    private SituationInfo GetCurrentSituationInfo()
    {
        SituationInfo info=new SituationInfo();

        //report on current elements only if the current element container is internal
        if (AffectsElementsContainer.IsInternal())
        {
            Dictionary<string, int> inSituation = AffectsElementsContainer.GetAllCurrentElements();
            foreach (string k in inSituation.Keys)
                info.ElementsInSituation.Add(k,inSituation[k]);
        }
        info.CurrentRecipe = currentRecipe;
        info.TimeRemaining = TimeRemaining;
        info.State = TimerState;

        return info;
    }

    private void AddIngredientsToInternalContainer(Recipe recipe, Dictionary<string, int> possiblyPersisted, IElementsContainer container)
    {
        foreach (var k in possiblyPersisted.Keys)
        {
            if (shouldPersistInInternalContainer(k, recipe))
                container.ModifyElementQuantity(k, possiblyPersisted[k]);
        }
    }



    private void resetWithRecipe(Compendium compendium)
    {
        currentRecipe = compendium.GetRecipeById(currentRecipe.Loop);
        TimeRemaining = currentRecipe.Warmup;
        
    }

    private void Complete()
    {
        SituationInfo info = GetCurrentSituationInfo();
        //find alternative recipe(s) - there may be additional recipes
        List<Recipe> recipesToExecute =
            _compendium.GetActualRecipesToExecute(currentRecipe, AffectsElementsContainer);
        foreach (Recipe r in recipesToExecute)
        {
            r.Do(AffectsElementsContainer);
            if (r.RetrievesContents())
                info.RetrievedContents = NoonUtility.AspectMatchFilter(currentRecipe.RetrievesContentsWith, 
                    AffectsElementsContainer.GetAllCurrentElements(),_compendium);
        }

        //if the first recipe in the list of alternatives is a different recipe, switch it.
        //This can be important if the alternative recipe has a different loop.
        if (recipesToExecute[0].Id != currentRecipe.Id)
            currentRecipe = recipesToExecute[0];
        
        if (currentRecipe.Loop != null)
            resetWithRecipe(_compendium);
        else
            Extinguish();


        publishUpdate(info);

    }

    private bool shouldPersistInInternalContainer(string elementId, Recipe recipe)
    {

        Element eToCheck = _compendium.GetElementById(elementId);
        Assert.IsNotNull(eToCheck, "invalid element id " + " checked in isPermittedByAspectFilter");

        if (eToCheck.ChildSlotSpecifications.Count>0)
            return false;

        foreach (string aspectFilterId in recipe.PersistsIngredientsWith.Keys)
        {
            if (!eToCheck.Aspects.ContainsKey(aspectFilterId))
                return false;
        }

        return true;
    }

}
    