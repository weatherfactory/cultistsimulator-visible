using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Services;
using Noon;


/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;
    private HashSet<AnchorAndSlot> outstandingSlotsToFill=new HashSet<AnchorAndSlot>();
    private int beatsTowardsAngelry = 0;
    //do major housekeeping every n beats
    private const int HOUSEKEEPING_CYCLE_BEATS = 20; //usually, a second
	// Autosave tracking is now done in TabletopManager.Update()
    private const float BEAT_INTERVAL_SECONDS = 0.05f;

    private const string METHODNAME_BEAT="Beat"; //so we don't get a tiny daft typo with the Invoke
    private GameSpeedState gameSpeedState=new GameSpeedState();

    private float timerBetweenBeats=0f;

    public void Update()
    {
        try
        {
            timerBetweenBeats += Time.deltaTime;

        if (timerBetweenBeats > BEAT_INTERVAL_SECONDS)
        {
            timerBetweenBeats -= BEAT_INTERVAL_SECONDS;
            if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Fast)
                Beat(BEAT_INTERVAL_SECONDS * 3);
            else if (gameSpeedState.GetEffectiveGameSpeed()==GameSpeed.Normal)
                Beat(BEAT_INTERVAL_SECONDS);
            else if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused)
                Beat(0f);
            else
               NoonUtility.Log("Unknown game speed state: " + gameSpeedState.GetEffectiveGameSpeed());
        }
        }
        catch (Exception e)
        {
            NoonUtility.LogException(e);
            throw;
        }

    }

    public void RespondToSpeedControlCommand(SpeedControlEventArgs args)
    {
        gameSpeedState.SetGameSpeedCommand(args.ControlPriorityLevel,args.GameSpeed);
    }


    public void Beat(float beatInterval)
    {
        beatsTowardsAngelry++;

        foreach (Situation sc in Registry.Get<SituationsCatalogue>().GetRegisteredSituations())
            sc.ExecuteHeartbeat(beatInterval);

        foreach(Sphere sphere in Registry.Get<SphereCatalogue>().GetSpheres())
            sphere.ExecuteHeartbeat(beatInterval);
   
        if (beatsTowardsAngelry >= HOUSEKEEPING_CYCLE_BEATS)
        {
            beatsTowardsAngelry = 0;
        }

    }
    private void DetermineOutstandingSlots(float beatInterval)
    {
        //TODO: execute the heartbeat, which should nudge angels
        //nudged angels (eg greedy angels) should then grab tokens
        var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

        foreach (var sc in situationControllers)
        {
          sc.ExecuteHeartbeat(beatInterval);

            //foreach (var tokenAndSlot in response.SlotsToFill)
            //{
            //    if (!OutstandingSlotAlreadySaved(tokenAndSlot))
            //        outstandingSlotsToFill.Add(tokenAndSlot);
            //}
        }
    }


}
