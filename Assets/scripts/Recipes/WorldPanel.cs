using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WorldPanel : MonoBehaviour {

    [SerializeField]
    private GameObject prefabTimerPanel;
    private List<TimerPanel> CurrentTimers=new List<TimerPanel>();

    public void AddTimer(Recipe forRecipe)
    {
        GameObject newTimerPanelGameObject = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTimerPanel = newTimerPanelGameObject.GetComponent<TimerPanel>();
        newTimerPanel.StartTimer(forRecipe);
        CurrentTimers.Add(newTimerPanel);
    }

    public void DoHeartbeat()
    {
        foreach (var t in CurrentTimers)
        {
           t.DoHeartbeat();
        }
        CurrentTimers.RemoveAll(t => t.TimeRemaining <= 0);
    }
}

