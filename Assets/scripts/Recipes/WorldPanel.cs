using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class WorldPanel : BoardMonoBehaviour
{
    [SerializeField] private GameObject prefabTimerPanel;

    public void RegisterSituation(BaseRecipeSituation rs)
    {
        GameObject newTPobj = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTP = newTPobj.GetComponent<TimerPanel>();

  rs.Subscribe(newTP);
    }
  
}