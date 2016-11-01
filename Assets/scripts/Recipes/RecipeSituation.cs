﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Assets.scripts.Entities;
using UnityEngine.Assertions;

public class RecipeSituation
{
    private float _timeRemaining;
    private List<IRecipeSituationSubscriber> _subscribers=new List<IRecipeSituationSubscriber>();
    private Compendium _compendium;
    public Recipe Recipe { get; set; }
    private readonly IElementsContainer AffectsElements;
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
            if(Recipe==null)
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
        if(AffectsElements.IsInternal())
            return AffectsElements.GetCurrentElementQuantity(forElementId);

        return 0;
    }

    public RecipeSituation(Recipe recipe, float? timeremaining,IElementsContainer workspaceContainer,Compendium rc)
    {
        Recipe = recipe;
        OriginalRecipeId = recipe.Id;
        _compendium = rc;

        if (timeremaining == null)
            TimeRemaining = recipe.Warmup;
        else
            TimeRemaining = timeremaining.Value;

        if (recipe.PersistsIngredients())
        { 
            AffectsElements= new SituationElementContainer();
            Dictionary<string, int> potentiallyPersistableIngredients = workspaceContainer.GetOutputElements();
            if(potentiallyPersistableIngredients!=null) //if we have any suitable ingredients
                AddIngredientsToInternalContainer(recipe, potentiallyPersistableIngredients,AffectsElements);
        }
        else
            AffectsElements = workspaceContainer;


    }

    public void DoHeartbeat()
    {
        _timeRemaining--;
        if (_timeRemaining <= 0)
        {
            Complete();
        }
        publishUpdate();
    }


    public void Extinguish()
    {
        Recipe = null;
        publishUpdate();
    }

 
    public void Subscribe(IRecipeSituationSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
        subscriber.ReceiveSituationUpdate(Recipe,TimerState,TimeRemaining, GetCurrentSituationInfo());
    }

    private void publishUpdate()
    {
        foreach (var s in _subscribers)
            s.ReceiveSituationUpdate(Recipe, TimerState, TimeRemaining, GetCurrentSituationInfo());
    }

    private SituationInfo GetCurrentSituationInfo()
    {
        SituationInfo info=new SituationInfo();
        if (AffectsElements.IsInternal())
        {
            Dictionary<string, int> inSituation = AffectsElements.GetAllCurrentElements();
            foreach (string k in inSituation.Keys)
                info.Elements.Add(k,inSituation[k]);
        }

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
        Recipe = compendium.GetRecipeById(Recipe.Loop);
        TimeRemaining = Recipe.Warmup;
        publishUpdate();
    }

    private void Complete()
    {
        List<Recipe> recipesToExecute =
            _compendium.GetActualRecipesToExecute(Recipe, AffectsElements);
        foreach (Recipe r in recipesToExecute)
        {
            r.Do(AffectsElements);
        }

        if (Recipe.Loop != null)
        {
            resetWithRecipe(_compendium);
        }
        else
            Extinguish();

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
    