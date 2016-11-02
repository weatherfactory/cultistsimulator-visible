using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;


public class World
    {
    private List<BaseRecipeSituation> currentRecipeSituations;
        private Compendium _compendium;

        public World(Compendium rc)
        {
        currentRecipeSituations = new List<BaseRecipeSituation>();
            _compendium = rc;
        }

      public BaseRecipeSituation AddSituation(Recipe forRecipe, float? timeRemaining, IElementsContainer ec)
      {
        
        if (currentRecipeSituations.Exists(rs => rs.OriginalRecipeId == forRecipe.Id))
              return null;

          BaseRecipeSituation newRecipeSituation;
        if(forRecipe.PersistsIngredients())            
            newRecipeSituation = new InteractiveRecipeSituation(forRecipe, timeRemaining, ec, _compendium);
        else
            newRecipeSituation = new SealedRecipeSituation(forRecipe, timeRemaining, ec, _compendium);

        currentRecipeSituations.Add(newRecipeSituation);
            return newRecipeSituation;
        }


        public void DoHeartbeat()
        {
        
        List<BaseRecipeSituation> situationsToRun = new List<BaseRecipeSituation>();
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
        foreach (BaseRecipeSituation rs in currentRecipeSituations)
            rs.Extinguish();

        currentRecipeSituations.Clear();
        }

    public IEnumerable<BaseRecipeSituation> GetCurrentRecipeSituations()
    {
        return currentRecipeSituations;
    }

    public void FastForward(int seconds)
    {
        for (int i = 1; i <= seconds; i++)
            DoHeartbeat();
    }
}

