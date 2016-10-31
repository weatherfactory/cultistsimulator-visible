using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class WorldPanel : BoardMonoBehaviour
{
    [SerializeField] private GameObject prefabTimerPanel;
    private List<TimerPanel> CurrentTimerPanels = new List<TimerPanel>();
    private List<RecipeSituation> CurrentRecipeSituations=new List<RecipeSituation>();

    public void AddTimer(Recipe forRecipe,float? timeRemaining,IRecipeSituationSubscriber s)
    {
        GameObject newTimerPanelGameObject = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTimerPanel = newTimerPanelGameObject.GetComponent<TimerPanel>();
        newTimerPanel.StartTimer(forRecipe, timeRemaining);
        newTimerPanel.RecipeSituation.AddSubscriber(s);
        CurrentTimerPanels.Add(newTimerPanel);


        //RecipeSituation rs = new RecipeSituation(forRecipe, timeRemaining);
        //GameObject newTPobj = Instantiate(prefabTimerPanel, transform) as GameObject;
        //TimerPanel newTP = newTimerPanelGameObject.GetComponent<TimerPanel>();

        //rs.AddSubscriber();
    }

    public void DoHeartbeat(Character c)
    {
        List<TimerPanel> timerPanelsToRun = new List<TimerPanel>();
        timerPanelsToRun.AddRange(CurrentTimerPanels);
        foreach (var t in timerPanelsToRun)
        {
            RecipeTimerState timerState=t.DoHeartbeat(c);
            if (timerState == RecipeTimerState.Complete)
            { 
                CurrentTimerPanels.Remove(t);
            }
        }
        
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
        List<TimerPanel> timerPanelsToRemove = new List<TimerPanel>();
        timerPanelsToRemove.AddRange(CurrentTimerPanels);
        foreach (TimerPanel tp in timerPanelsToRemove)
        {
            CurrentTimerPanels.Clear();
            BM.ExileToLimboThenDestroy(tp.gameObject);
        }
        
    }

    public void FastForward(int seconds,Character c)
    {
        for(int i=1;i<=seconds;i++)
            DoHeartbeat(c);
    }
}