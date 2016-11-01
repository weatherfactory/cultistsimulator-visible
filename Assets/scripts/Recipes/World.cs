using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;


public class World
    {
    private List<RecipeSituation> currentRecipeSituations;
        private Compendium _compendium;

        public World(Compendium rc)
        {
        currentRecipeSituations = new List<RecipeSituation>();
            _compendium = rc;
        }

      public RecipeSituation AddSituation(Recipe forRecipe, float? timeRemaining, IElementsContainer ec)
      {
        RecipeSituation newRecipeSituation = new RecipeSituation(forRecipe, timeRemaining, ec,_compendium);

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
            rs.DoHeartbeat();
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

