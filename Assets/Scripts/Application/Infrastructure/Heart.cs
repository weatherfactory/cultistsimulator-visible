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

    private GameSpeedState gameSpeedState=new GameSpeedState();

    private float timerBetweenBeats=0f;
    private float nextAnimTime;
    private float nextAirSoundTime;
    private string idOfLastTokenAnimated; // to not animate the same twice. Keep player on their toes
    


    public void Start()
    {
        SetNextAnimTime();
        SetNextAirSoundTime();
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
            if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused)
                return;

            timerBetweenBeats += Time.deltaTime;

            if (timerBetweenBeats > BEAT_INTERVAL_SECONDS)
            {
                timerBetweenBeats -= BEAT_INTERVAL_SECONDS;
                if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Fast)
                    Beat(BEAT_INTERVAL_SECONDS * 3);
                else if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Normal)
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

        foreach(Sphere sphere in Watchman.Get<HornedAxe>().GetSpheres())
        {
            sphere.RequestFlockActions(beatInterval);
            var tokenHeartbeatIntervalForThisSphere = beatInterval * sphere.TokenHeartbeatIntervalMultiplier;

            if (tokenHeartbeatIntervalForThisSphere > 0) //for many spheres, the multiplier is 0 and time won't pass for tokens.
                sphere.RequestTokensSpendTime(beatInterval);

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
