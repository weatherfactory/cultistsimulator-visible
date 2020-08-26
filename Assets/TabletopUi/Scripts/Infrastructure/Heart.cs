using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Services;
using Noon;


public enum GameSpeed
{ Paused=0,
    Normal=1,
    Fast=2}

/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;
    private HashSet<TokenAndSlot> outstandingSlotsToFill=new HashSet<TokenAndSlot>();
    private int beatCounter = 0;
    //do major housekeeping every n beats
    private const int HOUSEKEEPING_CYCLE_BEATS = 20; //usually, a second
	// Autosave tracking is now done in TabletopManager.Update()


    private const string METHODNAME_BEAT="Beat"; //so we don't get a tiny daft typo with the Invoke
    private float usualInterval;
    private GameSpeed CurrentGameSpeed=GameSpeed.Normal;


    public bool IsPaused { get; private set; }

    public void StartBeatingWithDefaultValue()
    {
        StartBeating(0.05f);
    }
	public void StartBeating(float startingInterval)
	{
        CancelInvoke(METHODNAME_BEAT);
        usualInterval = startingInterval;
        InvokeRepeating(METHODNAME_BEAT,0, usualInterval);
        IsPaused = false;
		beatCounter = HOUSEKEEPING_CYCLE_BEATS;	// Force immediate housekeeping check on resume - CP
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
		// Moved this outside AdvanceTime so that the interval parameter is respected (and I can call it with 0 reliably) - CP
		float beatInterval = usualInterval;
		if (CurrentGameSpeed == GameSpeed.Fast)
			beatInterval = usualInterval * 3;

        AdvanceTime(beatInterval);
        beatCounter++;

        if (beatCounter >= HOUSEKEEPING_CYCLE_BEATS)
        {
            beatCounter = 0;

            outstandingSlotsToFill = Registry.Get<TabletopManager>()
                .FillTheseSlotsWithFreeStacks(outstandingSlotsToFill);
        }
    }


    async void  OnApplicationQuit()
    {
        var saveTask = Registry.Get<TabletopManager>().SaveGameAsync(true,SourceForGameState.DefaultSave);
        await saveTask;
    }

    public void AdvanceTime(float intervalThisBeat)
    {
        //foreach existing active recipe window: run beat there
        //advance timer
        var tabletopManager = Registry.Get<TabletopManager>();
        var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

        foreach (var sc in situationControllers)
        {
            HeartbeatResponse response = sc.ExecuteHeartbeat(intervalThisBeat);

            foreach (var r in response.SlotsToFill) {
                if (!OutstandingSlotAlreadySaved(r))
                    outstandingSlotsToFill.Add(r);
            }
        }

        tabletopManager.DecayStacksOnTable(intervalThisBeat);
        tabletopManager.DecayStacksInResults(intervalThisBeat);
    }

    bool OutstandingSlotAlreadySaved(TokenAndSlot slot) {
        foreach (var item in outstandingSlotsToFill)
            if (item.Token == slot.Token && item.RecipeSlot == slot.RecipeSlot)
                return true;

        return false;
    }

    //remove any outstanding state when loading the game
    public void Clear()
    {
        outstandingSlotsToFill.Clear();
    }
}
