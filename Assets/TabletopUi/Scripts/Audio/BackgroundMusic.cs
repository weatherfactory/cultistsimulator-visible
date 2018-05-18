#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Noon;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour {

    protected IEnumerable<AudioClip> backgroundMusic;
    protected IEnumerable<AudioClip> impendingDoomMusic;
    protected IEnumerable<AudioClip> mansusMusic;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    public int currentTrackNumber;
    private System.Random random;
    private AudioClip currentClip;

    // Use this for initialization
    void Awake() {
        backgroundMusic = ResourcesManager.GetBackgroundMusic();
        impendingDoomMusic = ResourcesManager.GetImpendingDoomMusic();
        mansusMusic = ResourcesManager.GetMansusMusic();
        random = new System.Random();
    }

    void Start() {
        PlayNextClip();
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
        NoonUtility.Log("Playing" + currentClip.name, 8);
    }

    public void PlayRandomClip() {
        PlayClip(random.Next(0, backgroundMusic.Count()), backgroundMusic);

    }

    public void PlayMansusClip() {
        PlayClip(0, mansusMusic);
    }

    public void PlayImpendingDoom() {
        if(currentClip!=impendingDoomMusic.ElementAt(0))
            PlayClip(0,impendingDoomMusic);
    }

    public void NoMoreImpendingDoom()
    {
        if (currentClip == impendingDoomMusic.ElementAt(0))
            PlayNextClip();
    }

    // Options Control

    public void SetMute(bool mute) {
        audioSource.mute = mute;
    }

    public void SetVolume(float volume) {
        audioSource.volume = volume;
    }

    public bool GetMute() {
        return audioSource.mute;
    }

    public float GetVolume() {
        return audioSource.volume;
    }

}
