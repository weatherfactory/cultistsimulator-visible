using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class WorldPanel : BoardMonoBehaviour
{
    [SerializeField] private TimerPanel prefabTimerPanel;
    [SerializeField]
    private TimerPanel prefabInteractiveTimerPanel;

    public void RegisterSituation(IRecipeSituation rs)
    {
        TimerPanel newTPobj;
        if (rs.IsInteractive())
            newTPobj = Instantiate(prefabInteractiveTimerPanel, transform) as TimerPanel;
        else
            newTPobj = Instantiate(prefabTimerPanel, transform) as TimerPanel;

  rs.Subscribe(newTPobj);
    }
  
}