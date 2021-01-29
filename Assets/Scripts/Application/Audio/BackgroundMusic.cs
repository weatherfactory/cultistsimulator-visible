#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using SecretHistories.Services;

using UnityEngine;

public class BackgroundMusic : MonoBehaviour, ISettingSubscriber
{

    protected IEnumerable<AudioClip> backgroundMusic;
    protected IEnumerable<AudioClip> impendingDoomMusic;
    protected IEnumerable<AudioClip> mansusMusic;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    public int currentTrackNumber;
    private System.Random random;
    private AudioClip currentClip;

    [Header("Settings")]
    [Tooltip("Music volume goes from 0 to 1")]
    public float musicVolMax = 0.25f;

    // Use this for initialization
    void Awake() {
        var w=new Watchman();
        w.Register(this);
        backgroundMusic = ResourcesManager.GetBackgroundMusic();
        impendingDoomMusic = ResourcesManager.GetImpendingDoomMusic();
        mansusMusic = ResourcesManager.GetMansusMusic();
        random = new System.Random();
    }

    void Start()
    {
        var musicVolumeSetting = Watchman.Get<Compendium>().GetEntityById<Setting>(NoonConstants.MUSICVOLUME);
        if (musicVolumeSetting != null)
        {
            musicVolumeSetting.AddSubscriber(this);
            WhenSettingUpdated(musicVolumeSetting.CurrentValue);
        }
        else
            NoonUtility.Log("Missing setting entity: " + NoonConstants.MUSICVOLUME);

        PlayNextClip();
    }

    public void WhenSettingUpdated(object newValue)
    {
        SetVolume(newValue is float ? (float) newValue : 0);
    }


    void Update() {
        if (!audioSource.isPlaying) {
            PlayNextClip();
        }
    }

    public virtual void PlayNextClip() {
        //currentTrackNumber++;
        //if (currentTrackNumber == backgroundMusic.Count())
        //    currentTrackNumber = 0;
        //PlayClip(currentTrackNumber);

        PlayClip(random.Next(1, backgroundMusic.Count()), backgroundMusic); //never plays the first clip

    }

    public void PlayClip(int trackNumber, IEnumerable<AudioClip> clips) {
        audioSource.Stop();
        currentClip = clips.ElementAt(trackNumber);
        audioSource.PlayOneShot(currentClip);
        NoonUtility.Log("Playing" + currentClip.name, 0,VerbosityLevel.Trivia);
    }

    public void PlayRandomClip() {
        PlayClip(random.Next(0, backgroundMusic.Count()), backgroundMusic);

    }

    public void PlayMansusClip() {
        PlayClip(0, mansusMusic);
    }

    public void SignalEndingFlavour(EndingFlavour endingFlavour) {
        if (endingFlavour == EndingFlavour.None)
        {
            if (currentClip == impendingDoomMusic.ElementAt(0))
                PlayNextClip();
        }
        else
        {
            //room here to play different doom flavour musics
        
        if(currentClip!=impendingDoomMusic.ElementAt(0))
            PlayClip(0,impendingDoomMusic);
        }
    }
    

    private void SetVolume(float volume)
    {
        if (audioSource == null)
            return;

        audioSource.volume = GetClampedVol(volume) * musicVolMax;

        if (volume == 0f && GetMute() == false)
            SetMute(true);
        else if (volume > 0f && GetMute())
            SetMute(false);
    }

    private float GetClampedVol(float sliderValue)
    {
        return Mathf.Pow(sliderValue / 10f, 2f); // slider has whole numbers only and goes from 0 to 10
    }

    public void SetMute(bool mute) {
        audioSource.mute = mute;
    }



    public bool GetMute() {
        return audioSource.mute;
    }

    public float GetVolume() {
        return audioSource.volume;
    }

}
