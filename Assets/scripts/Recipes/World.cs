using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;


public class World
    {
    public List<RecipeSituation> CurrentRecipeSituations;

        public World()
        {
        CurrentRecipeSituations = new List<RecipeSituation>();
    }

      public RecipeSituation AddSituation(Recipe forRecipe, float? timeRemaining, IElementsContainer ec)
        {
        RecipeSituation rs = new RecipeSituation(forRecipe, timeRemaining, ec);
        CurrentRecipeSituations.Add(rs);
            return rs;
        }


        public void DoHeartbeat()
        {
        
        List<RecipeSituation> situationsToRun = new List<RecipeSituation>();
        situationsToRun.AddRange(CurrentRecipeSituations);

        foreach (var rs in situationsToRun)
        {
            rs.DoHeartbeat(ContentRepository.Instance.RecipeCompendium);
            if (rs.TimerState == RecipeTimerState.Extinct)
                CurrentRecipeSituations.Remove(rs);
        }

    }

        public void Clear()
        {
        foreach (RecipeSituation rs in CurrentRecipeSituations)
            rs.Extinguish();

        CurrentRecipeSituations.Clear();
        }

    public IEnumerable<RecipeSituation> GetCurrentRecipeSituations()
    {
        return CurrentRecipeSituations;
    }

    public void FastForward(int seconds, Character c)
    {
        for (int i = 1; i <= seconds; i++)
            DoHeartbeat();
    }
}

