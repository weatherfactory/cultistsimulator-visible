using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Services;
using SecretHistories.Spheres;


/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;
    private HashSet<AnchorAndSlot> outstandingSlotsToFill=new HashSet<AnchorAndSlot>();



    private const float BEAT_INTERVAL_SECONDS = 0.05f;

    private GameSpeedState gameSpeedState=new GameSpeedState();

    private float timerBetweenBeats=0f;

    public void Update()
    {
        try
        {
            if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused)
                return;
            
            timerBetweenBeats += Time.deltaTime;

        if (timerBetweenBeats > BEAT_INTERVAL_SECONDS)
        {
            timerBetweenBeats -= BEAT_INTERVAL_SECONDS;
            if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Fast)
                Beat(BEAT_INTERVAL_SECONDS * 3);
            else if (gameSpeedState.GetEffectiveGameSpeed()==GameSpeed.Normal)
                Beat(BEAT_INTERVAL_SECONDS);

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
        foreach (Situation sc in Watchman.Get<SituationsCatalogue>().GetRegisteredSituations())
            sc.ExecuteHeartbeat(beatInterval);

        foreach(Sphere sphere in Watchman.Get<SphereCatalogue>().GetSpheres())
            sphere.ExecuteHeartbeat(beatInterval);
    }



}
