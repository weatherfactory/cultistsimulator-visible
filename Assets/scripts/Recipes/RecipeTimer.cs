using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RecipeTimer
{
    public Recipe Recipe { get; set; }
    public float TimeRemaining { get; set; }

    public RecipeTimer(Recipe recipe,float? timeremaining)
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
}