using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class WorldPanel : BoardMonoBehaviour
{
    [SerializeField] private GameObject prefabTimerPanel;
    private World World;



    public void RegisterSituation(RecipeSituation rs)
    {
        GameObject newTPobj = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTP = newTPobj.GetComponent<TimerPanel>();

  rs.Subscribe(newTP);
    }
    //public void AddSituation(Recipe forRecipe,float? timeRemaining,IElementsContainer ec)
    //{

        
    //    GameObject newTPobj = Instantiate(prefabTimerPanel, transform) as GameObject;
    //    TimerPanel newTP = newTPobj.GetComponent<TimerPanel>();

    //    RecipeSituation newRS = World.AddSituation(forRecipe, timeRemaining, ec);
    //    newRS.Subscribe(newTP);
    //}

    //public void DoHeartbeat()
    //{
    //    World.DoHeartbeat();
    //}




    //public void ClearWorld()
    //{
    //    World.Clear();
    //}

    //public void FastForward(int seconds,Character c)
    //{
    //    for(int i=1;i<=seconds;i++)
    //        DoHeartbeat();
    //}
}