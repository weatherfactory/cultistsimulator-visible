using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;


public class World
    {
    private List<RecipeSituation> currentRecipeSituations;
        private RecipeCompendium recipeCompendium;

        public World(RecipeCompendium rc)
        {
        currentRecipeSituations = new List<RecipeSituation>();
            recipeCompendium = rc;
        }

      public RecipeSituation AddSituation(Recipe forRecipe, float? timeRemaining, IElementsContainer ec)
      {
        RecipeSituation newRecipeSituation = new RecipeSituation(forRecipe, timeRemaining, ec);

        if (currentRecipeSituations.Exists(rs => rs.OriginalRecipeId == forRecipe.Id))
              return null;

       currentRecipeSituations.Add(newRecipeSituation);
            return newRecipeSituation;
        }


        public void DoHeartbeat()
        {
        
        List<RecipeSituation> situationsToRun = new List<RecipeSituation>();
        situationsToRun.AddRange(currentRecipeSituations);

        foreach (var rs in situationsToRun)
        {
            rs.DoHeartbeat(recipeCompendium);
            if (rs.TimerState == RecipeTimerState.Extinct)
                currentRecipeSituations.Remove(rs);
        }

    }

        public void Clear()
        {
        foreach (RecipeSituation rs in currentRecipeSituations)
            rs.Extinguish();

        currentRecipeSituations.Clear();
        }

    public IEnumerable<RecipeSituation> GetCurrentRecipeSituations()
    {
        return currentRecipeSituations;
    }

    public void FastForward(int seconds)
    {
        for (int i = 1; i <= seconds; i++)
            DoHeartbeat();
    }
}

