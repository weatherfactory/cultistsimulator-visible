using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class WorldPanel : BoardMonoBehaviour
{
    [SerializeField] private GameObject prefabTimerPanel;
    private List<RecipeSituation> CurrentRecipeSituations=new List<RecipeSituation>();

    public void AddSituation(Recipe forRecipe,float? timeRemaining,IRecipeSituationSubscriber s)
    {

        RecipeSituation rs = new RecipeSituation(forRecipe, timeRemaining);
        GameObject newTPobj = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTP = newTPobj.GetComponent<TimerPanel>();

        CurrentRecipeSituations.Add(rs);

        rs.Subscribe(newTP);
    }

    public void DoHeartbeat(Character c)
    {

        List<RecipeSituation> situationsToRun=new List<RecipeSituation>();
        situationsToRun.AddRange(CurrentRecipeSituations);

        foreach (var rs in situationsToRun)
        {
            rs.DoHeartbeat(c, ContentRepository.Instance.RecipeCompendium);
            if (rs.TimerState == RecipeTimerState.Extinct)
                CurrentRecipeSituations.Remove(rs);
        }

    }

    public IEnumerable<RecipeSituation> GetCurrentRecipeSituations()
    {
        return CurrentRecipeSituations;
    }


    public void ClearWorld()
    {
      foreach(RecipeSituation rs in CurrentRecipeSituations)
       rs.Extinguish();

      CurrentRecipeSituations.Clear();
    }

    public void FastForward(int seconds,Character c)
    {
        for(int i=1;i<=seconds;i++)
            DoHeartbeat(c);
    }
}