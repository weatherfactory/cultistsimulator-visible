using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float minSecondsBetweenAmbientAnimations = 5f;
    [SerializeField] private float maxSecondsBetweenAmbientAnimations = 8f;
    [SerializeField] private float minSecondsBetweenAmbientSfx = 8f;
    [SerializeField] private float maxSecondsBetweenAmbientSfx = 20f;

    public const float BEAT_INTERVAL_SECONDS = 0.05f;

    public bool Metapaused { get; protected set; }

    private GameSpeedState gameSpeedState=new GameSpeedState();

    private float timerBetweenBeats=0f;
    private float nextAnimTime;
    private float nextAirSoundTime;
    private string idOfLastTokenAnimated; // to not animate the same twice. Keep player on their toes

    public void Awake()
    {
        var w=new Watchman();
        w.Register(this);
    }

    public void Start()
    {
        SetNextAnimTime();
        SetNextAirSoundTime();
        
    }

    public void Metapause()
    {
        Metapaused = true;
    }

    public void Unmetapause()
    {
        Metapaused = false;
    }

    void SetNextAnimTime()
    {
        nextAnimTime = Time.time + UnityEngine.Random.Range(minSecondsBetweenAmbientAnimations, maxSecondsBetweenAmbientAnimations);
    }

    void SetNextAirSoundTime()
    {
        nextAirSoundTime = Time.time + UnityEngine.Random.Range(minSecondsBetweenAmbientSfx, maxSecondsBetweenAmbientSfx);
    }

    public void Update()
    {
        if (Metapaused)
            return;

        ProcessBeatCounter();
        ProcessAmbientAnimationCounter();
        ProcessAmbientSoundCounter();
    }

    private void ProcessAmbientSoundCounter()
    {
        if (Time.time >= nextAirSoundTime)
        {
            DoAmbientSound();
            SetNextAirSoundTime();
        }
    }

    private void ProcessAmbientAnimationCounter()
    {
        try
        {
            
            if (Time.time >= nextAnimTime)
            {
                DoAmbientAnimation();
                SetNextAnimTime();
            }
        }


        catch (Exception e)
        {
            NoonUtility.Log("Problem in checking for card animations: " + e.Message);
        }
    }

    private void ProcessBeatCounter()
    {
        try
        {
            timerBetweenBeats += Time.deltaTime;

            if (timerBetweenBeats > BEAT_INTERVAL_SECONDS)
            {
                timerBetweenBeats -= BEAT_INTERVAL_SECONDS;
                if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Fast)
                    Beat(BEAT_INTERVAL_SECONDS * 3, BEAT_INTERVAL_SECONDS);
                else if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Normal)
                    Beat(BEAT_INTERVAL_SECONDS, BEAT_INTERVAL_SECONDS);
                else if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused)
                    Beat(0, BEAT_INTERVAL_SECONDS);

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


    public void Beat(float seconds,float metaseconds)
    {

        foreach(Sphere sphere in Watchman.Get<HornedAxe>().GetSpheres())
        {
            var secondsForSphere = seconds * sphere.TokenHeartbeatIntervalMultiplier;
            sphere.RequestFlockActions(secondsForSphere, metaseconds);
            sphere.RequestTokensSpendTime(secondsForSphere, metaseconds);

        }
    }


    public void DoAmbientAnimation()
    {
        var spheresWhichAllowAnimations = Watchman.Get<HornedAxe>().GetSpheresWhichAllowAmbientAnimations();

        var allAnimatableTokens = new List<Token>();

        foreach (var s in spheresWhichAllowAnimations)
        {
            var animatableTokens = s.GetTokensWhere(t => !t.Defunct && t.CanAnimateArt() && t.PayloadId != idOfLastTokenAnimated);
            allAnimatableTokens.AddRange(animatableTokens);
        }

        
        if (allAnimatableTokens.Any())
        {
            int index = UnityEngine.Random.Range(0, allAnimatableTokens.Count);

            allAnimatableTokens[index].StartArtAnimation();
            idOfLastTokenAnimated = allAnimatableTokens[index].Payload.Id;
        }
    }

    public void DoAmbientSound()
    {
        SoundManager.PlaySfx("TokenAnimAir");
        SetNextAirSoundTime();
        
    }

}
