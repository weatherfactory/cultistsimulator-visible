using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

public class RecipeSituation
{
    private float _timeRemaining;
    private List<IRecipeSituationSubscriber> _subscribers=new List<IRecipeSituationSubscriber>();
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

    public RecipeTimerState DoHeartbeat(INotifier notifier, IElementsContainer elementsContainer,
        RecipeCompendium compendium)
    {
        _timeRemaining--;
        if (_timeRemaining <= 0)
        {
            return Complete(notifier, elementsContainer, compendium);

        }
         return RecipeTimerState.Ongoing;
    }

    private RecipeTimerState Complete(INotifier notifier, IElementsContainer elementsContainer,
        RecipeCompendium compendium)
    {
        List<Recipe> recipesToExecute =
            compendium.GetActualRecipesToExecute(Recipe, elementsContainer);
        foreach (Recipe r in recipesToExecute)
            r.Do(notifier, elementsContainer);

        foreach(var s in _subscribers)
            s.SituationComplete(Recipe);
   
        if (Recipe.Loop != null)
        {
            Recipe = compendium.GetRecipeById(Recipe.Loop);
            TimeRemaining = Recipe.Warmup;
            return RecipeTimerState.Ongoing;
        }
        else
        {
            Recipe = null;
            return RecipeTimerState.Complete;
        }
    }

    public void AddSubscriber(IRecipeSituationSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
    }
}
    