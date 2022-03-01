#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Fucine;
using SecretHistories.Services;

using UnityEngine;

public class BackgroundMusic : MonoBehaviour, ISettingSubscriber
{

    protected IEnumerable<AudioClip> backgroundMusic;
    protected IEnumerable<AudioClip> impendingDoomMusic;
    protected IEnumerable<AudioClip> otherworldMusic;

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
        otherworldMusic = ResourcesManager.GetOtherworldMusic();
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

    public void PlayOtherworldClip(string trackName)
    {
        var clip = otherworldMusic.SingleOrDefault(c => c.name == trackName);
        if (clip == null)
        {
            NoonUtility.Log($"Couldn't find track name {trackName} in otherworld clips");
            return;
        }
        audioSource.Stop();
        audioSource.PlayOneShot(clip);
        NoonUtility.Log("Playing" + currentClip.name, 0, VerbosityLevel.Trivia);
    }



    public void FadeToSilence(float overSeconds)
    {
        StartCoroutine(ChangeVolumeOverTime(audioSource.volume, 0f, overSeconds));
    }

    private IEnumerator ChangeVolumeOverTime(float fromVol,float toVol, float changeOverSeconds)
    {
        float elapsedSeconds = 0f;

        while (elapsedSeconds < changeOverSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(fromVol, toVol, elapsedSeconds / changeOverSeconds);
            yield return null;
        }
    }

    public void SignalEndingFlavourHasChangedOnTimeshadow(EndingFlavour oldEndingFlavour,EndingFlavour newEndingFlavour) {
        if (newEndingFlavour == EndingFlavour.None)
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
        return Mathf.Pow(sliderValue / 10f, 2f); // slider has integers only and goes from 0 to 10
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
