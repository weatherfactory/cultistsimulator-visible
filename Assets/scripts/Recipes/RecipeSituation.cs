using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RecipeSituation
{
    public Recipe Recipe { get; set; }
    public float TimeRemaining { get; set; }

    public RecipeSituation(Recipe recipe,float? timeremaining)
    {
        Recipe = recipe;
        if (timeremaining == null)
            TimeRemaining = recipe.Warmup;
        else
            TimeRemaining = timeremaining.Value;
    }

    public RecipeTimerState DoHeartbeat()
    {
        TimeRemaining--;
        if (TimeRemaining <= 0)
            return RecipeTimerState.Complete;

        return RecipeTimerState.Ongoing;
    }

    public void Complete(INotifier notifier, IElementsContainer elementsContainer, RecipeCompendium compendium)
    {
        List<Recipe> recipesToExecute =
                 compendium.GetActualRecipesToExecute(Recipe, elementsContainer);
        foreach (Recipe r in recipesToExecute)
            r.Do(notifier, elementsContainer);

        if (Recipe.Loop != null)
            Recipe = compendium.GetRecipeById(Recipe.Loop);
        else
            Recipe = null;
    }
}