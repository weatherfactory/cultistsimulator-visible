using System;
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
    private RecipeCompendium _recipeCompendium;
    public Recipe Recipe { get; set; }
    public readonly IElementsContainer ElementsContainerAffected;
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

    public RecipeSituation(Recipe recipe, float? timeremaining,IElementsContainer workspaceContainer,RecipeCompendium rc)
    {
        Recipe = recipe;
        OriginalRecipeId = recipe.Id;
        _recipeCompendium = rc;

        if (timeremaining == null)
            TimeRemaining = recipe.Warmup;
        else
            TimeRemaining = timeremaining.Value;

        if (recipe.PersistsIngredients())
        { 
            ElementsContainerAffected= new SituationElementContainer();
            Dictionary<string, int> potentiallyPersistableIngredients = workspaceContainer.GetOutputElements();
            if(potentiallyPersistableIngredients!=null) //if we have any suitable ingredients
                AddIngredientsToInternalContainer(recipe, potentiallyPersistableIngredients,ElementsContainerAffected);
        }
        else
            ElementsContainerAffected = workspaceContainer;


    }

    private void AddIngredientsToInternalContainer(Recipe recipe, Dictionary<string, int> possiblyPersisted,IElementsContainer container)
    {
        foreach (var k in possiblyPersisted.Keys)
        {
            if (shouldPersistInInternalContainer(k, recipe))
                container.ModifyElementQuantity(k, possiblyPersisted[k]);
        }
    }

    private bool shouldPersistInInternalContainer(string elementId, Recipe recipe)
    {

        Element eToCheck = _recipeCompendium.GetElementById(elementId);
        Assert.IsNotNull(eToCheck,"invalid element id " + " checked in isPermittedByAspectFilter");
        foreach(string aspectFilterId in recipe.PersistedIngredients.Keys)
        { 
            if (eToCheck.ChildSlotSpecifications.Count==0 && eToCheck.Aspects.ContainsKey(aspectFilterId))
                return true;
        }
        
        return false;
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


    private void Complete()
    {
        List<Recipe> recipesToExecute =
            _recipeCompendium.GetActualRecipesToExecute(Recipe, ElementsContainerAffected);
        foreach (Recipe r in recipesToExecute)
        {
            r.Do(ElementsContainerAffected);
        }

        if (Recipe.Loop != null)
        {
            resetWithRecipe(_recipeCompendium);
        }
        else
            Extinguish();

    }

    private void resetWithRecipe(RecipeCompendium compendium)
    {
        Recipe = compendium.GetRecipeById(Recipe.Loop);
        TimeRemaining = Recipe.Warmup;
        publishUpdate();
    }

    public void Extinguish()
    {
        Recipe = null;
        publishUpdate();
    }

    private void publishUpdate()
    {
        foreach (var s in _subscribers)
            s.ReceiveSituationUpdate(Recipe, TimerState, TimeRemaining);
    }
    public void Subscribe(IRecipeSituationSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
        subscriber.ReceiveSituationUpdate(Recipe,TimerState,TimeRemaining);
    }

}
    