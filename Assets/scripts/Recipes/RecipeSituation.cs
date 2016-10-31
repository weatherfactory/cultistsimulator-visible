using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

public class RecipeSituation:IElementsContainer
{
    private float _timeRemaining;
    private List<IRecipeSituationSubscriber> _subscribers=new List<IRecipeSituationSubscriber>();
    private readonly Dictionary<string, int> _elements=new Dictionary<string, int>();
    public Recipe Recipe { get; set; }
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

    public RecipeSituation(Recipe recipe, float? timeremaining)
    {
        Recipe = recipe;
        OriginalRecipeId = recipe.Id;

        if (timeremaining == null)
            TimeRemaining = recipe.Warmup;
        else
            TimeRemaining = timeremaining.Value;
    }

    public void DoHeartbeat(IElementsContainer elementsContainer,
        RecipeCompendium compendium)
    {
        _timeRemaining--;
        if (_timeRemaining <= 0)
        {
            Complete(elementsContainer, compendium);
        }
        publishUpdate();
    }


    private void Complete(IElementsContainer elementsContainer,
        RecipeCompendium compendium)
    {
        List<Recipe> recipesToExecute =
            compendium.GetActualRecipesToExecute(Recipe, elementsContainer);
        foreach (Recipe r in recipesToExecute)
        {
            r.Do(elementsContainer);
        }

        if (Recipe.Loop != null)
        {
            resetWithRecipe(compendium);
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

    public void ModifyElementQuantity(string elementId, int quantityChange)
    {
        if (!_elements.ContainsKey(elementId))
        
            _elements.Add(elementId, quantityChange);
        else
        _elements[elementId]=_elements[elementId]+=quantityChange;
        }

    public int GetCurrentElementQuantity(string elementId)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, int> GetAllCurrentElements()
    {
        throw new NotImplementedException();
    }

    public void TriggerSpecialEvent(string endingId)
    {
        throw new NotImplementedException();
    }

}
    