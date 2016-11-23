using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.CS.TabletopUI;

/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;
    private HashSet<IRecipeSlot> SlotRequestsToFill=new HashSet<IRecipeSlot>();
    private int beatCounter = 0;
    private const int UpdateCycleBeats = 20;

    private const string METHODNAME_BEAT="Beat"; //so we don't get a tiny daft typo with the Invoke
    private float usualInterval;
  public void StartBeating(float startingInterval)
  {
        usualInterval = startingInterval;
        InvokeRepeating(METHODNAME_BEAT,0, usualInterval);
    }

    public void StopBeating()
    {
        CancelInvoke(METHODNAME_BEAT);
    }

    public void Beat()
    {
            Beat(usualInterval);
        beatCounter++;
        //if(beatCounter==20)
            
    }

    

    public void Beat(float interval)
    {
        //foreach existing active recipe window: run beat there
        //advance timer
        var situationTokens = allContent.GetComponentsInChildren<SituationToken>();
        foreach (var st in situationTokens)
        {
           HeartbeatResponse response=st.ExecuteHeartbeat(interval);
            foreach (var r in response.SlotsToFill)
                SlotRequestsToFill.Add(r);
        }
    }


}
