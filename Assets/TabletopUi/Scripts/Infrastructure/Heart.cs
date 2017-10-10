using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;
    private HashSet<TokenAndSlot> outstandingSlotsToFill=new HashSet<TokenAndSlot>();
    private int beatCounter = 0;
    //do major housekeeping every n beats
    private const int HOUSEKEEPING_CYCLE_BEATS = 20;
    private const string METHODNAME_BEAT="Beat"; //so we don't get a tiny daft typo with the Invoke
    private float usualInterval;

    public bool IsPaused { get; private set; }


  public void StartBeating(float startingInterval)
  {
        CancelInvoke(METHODNAME_BEAT);
        usualInterval = startingInterval;
        InvokeRepeating(METHODNAME_BEAT,0, usualInterval);
        
        IsPaused = false;
  }

    public void StopBeating()
    {
        CancelInvoke(METHODNAME_BEAT);
        IsPaused = true;
    }

    public void ResumeBeating()
    {
        StartBeating(usualInterval);
        IsPaused = false;
    }

    public void Beat()
    {
 
            AdvanceTime(usualInterval);
            beatCounter++;
            if (beatCounter >= HOUSEKEEPING_CYCLE_BEATS)
            {
                beatCounter = 0;
                outstandingSlotsToFill = Registry.Retrieve<TabletopManager>()
                    .FillTheseSlotsWithFreeStacks(outstandingSlotsToFill);
            }
 
    }
    

    public void AdvanceTime(float interval)
    {
        //foreach existing active recipe window: run beat there
        //advance timer
        var tabletopManager = Registry.Retrieve<TabletopManager>();
        var situationTokens = tabletopManager.GetAllSituationTokens();
        foreach (var st in situationTokens)
        {
           HeartbeatResponse response=st.ExecuteHeartbeat(interval);
            foreach (var r in response.SlotsToFill)
                outstandingSlotsToFill.Add(r);
        }

       tabletopManager.DecayStacksOnTable(interval);
    }

    //remove any outstanding state when loading the game
    public void Clear()
    {
        outstandingSlotsToFill.Clear();
    }
}
