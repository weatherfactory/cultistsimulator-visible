using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class WorldPanel : BoardMonoBehaviour
{
    [SerializeField] private GameObject prefabTimerPanel;
    [SerializeField]
    private GameObject prefabInteractiveTimerPanel;

    public void RegisterSituation(IRecipeSituation rs)
    {
        GameObject newTPobj;
        if (rs.IsInteractive())
            newTPobj = Instantiate(prefabInteractiveTimerPanel, transform) as GameObject;
        else
            newTPobj = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTP = newTPobj.GetComponent<TimerPanel>();

  rs.Subscribe(newTP);
    }
  
}