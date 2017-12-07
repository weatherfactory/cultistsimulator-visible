using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Assets.Core.Commands;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Noon;


public enum GameSpeed
{ Normal,Fast}

/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;
    private HashSet<TokenAndSlot> outstandingSlotsToFill=new HashSet<TokenAndSlot>();
    private int beatCounter = 0;
    private int housekeepingCyclesCounter = 0;
    //do major housekeeping every n beats
    private const int HOUSEKEEPING_CYCLE_BEATS = 20; //usually, a second
    private const int AUTOSAVE_CYCLE_HOUSEKEEPINGS = 300; //ysually, five minutes; number of housekeeping events that should pass before we autosave
    
    private const string METHODNAME_BEAT="Beat"; //so we don't get a tiny daft typo with the Invoke
    private float usualInterval;
    private GameSpeed CurrentGameSpeed=GameSpeed.Normal;
    

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

    public void SetGameSpeed(GameSpeed speed)
    {
        CurrentGameSpeed = speed;
    }

    public GameSpeed GetGameSpeed()
    {
        return CurrentGameSpeed;
    }

    public void Beat()
    {
 
            AdvanceTime(usualInterval);
            beatCounter++;
            if (beatCounter >= HOUSEKEEPING_CYCLE_BEATS)
            {
                beatCounter = 0;
                housekeepingCyclesCounter++;
                outstandingSlotsToFill = Registry.Retrieve<TabletopManager>()
                    .FillTheseSlotsWithFreeStacks(outstandingSlotsToFill);
            }

        if (housekeepingCyclesCounter >= AUTOSAVE_CYCLE_HOUSEKEEPINGS)
        {
            housekeepingCyclesCounter = 0;
            Registry.Retrieve<TabletopManager>().SaveGame(false);
        }
    }
    

    public void AdvanceTime(float intervalThisBeat)
    {
        //foreach existing active recipe window: run beat there
        //advance timer
        var tabletopManager = Registry.Retrieve<TabletopManager>();
        var situationTokens = tabletopManager.GetAllSituationTokens();
        foreach (var st in situationTokens)
        {
            if (CurrentGameSpeed == GameSpeed.Fast)
                intervalThisBeat = usualInterval * 2;
           HeartbeatResponse response=st.ExecuteHeartbeat(intervalThisBeat);
            foreach (var r in response.SlotsToFill)
                outstandingSlotsToFill.Add(r);
        }

       tabletopManager.DecayStacksOnTable(intervalThisBeat);
    }

    //remove any outstanding state when loading the game
    public void Clear()
    {
        outstandingSlotsToFill.Clear();
    }
}
