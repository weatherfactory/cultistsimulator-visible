using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class WorldPanel : BoardMonoBehaviour
{
    [SerializeField] private GameObject prefabTimerPanel;
    private List<TimerPanel> CurrentTimerPanels = new List<TimerPanel>();

    public void AddTimer(Recipe forRecipe,float? timeRemaining)
    {
        GameObject newTimerPanelGameObject = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTimerPanel = newTimerPanelGameObject.GetComponent<TimerPanel>();
        newTimerPanel.StartTimer(forRecipe, timeRemaining);
        CurrentTimerPanels.Add(newTimerPanel);
    }

    public void DoHeartbeat()
    {
        foreach (var t in CurrentTimerPanels)
        {
            t.DoHeartbeat();
        }
        CurrentTimerPanels.RemoveAll(t => t.TimeRemaining <= 0);
    }

    public List<RecipeSituation> GetCurrentRecipeTimers()
    {
        List<RecipeSituation> l =new List<RecipeSituation>();
        foreach(TimerPanel tp in CurrentTimerPanels)
        {
            Assert.IsNotNull(tp,"somehow got a null timerpanel");
            Assert.IsNotNull(tp.RecipeSituation, "somehow got a null recipetimer");
            l.Add(tp.RecipeSituation);
        }

        return l;
    }

    public void ClearRecipeTimers()
    {
        foreach (TimerPanel tp in CurrentTimerPanels)
        {
            BM.ExileToLimboThenDestroy(tp.gameObject);
        }
        CurrentTimerPanels.Clear();
    }

    public void FastForward(int seconds)
    {
        for(int i=1;i<=seconds;i++)
        foreach(var t in CurrentTimerPanels)
                t.DoHeartbeat();
    }
}