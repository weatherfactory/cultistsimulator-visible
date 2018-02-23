#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Audio;
using TMPro;

public class OptionsPanel : MonoBehaviour {
    [Header("Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider inspectionTimeSlider;
    [SerializeField] private Slider birdWormSlider;
    
    [SerializeField] private TextMeshProUGUI musicSliderValue;
    [SerializeField] private TextMeshProUGUI soundSliderValue;
    [SerializeField] private TextMeshProUGUI inspectionTimeSliderValue;
    [SerializeField] private TextMeshProUGUI birdWormSliderValue;

    [Header("Settings")]
    [Tooltip("Music volume goes from 0 to 1")]
    public float musicVolMax = 0.25f;

    [Tooltip("Sound volume uses DB ranges ")]
    public float soundDbMin = -40f;
    public float soundDbMax = 10f;

    [Tooltip("Timer is baseTimer + notches * timePerNotch, with the second notch from the right being 0")]
    public float baseInfoTimer = 10f;
    public float baseTimePerNotch = 2f;

    [Header("External Ref")]
    [SerializeField]
    private AudioMixer audioMixer;

    BackgroundMusic backgroundMusic;
    SoundManager soundManager;
    BaseDetailsWindow[] infoWindows;

    public void InitAudioSettings() {
        gameObject.SetActive(false);

        backgroundMusic = FindObjectOfType<BackgroundMusic>();
        soundManager = FindObjectOfType<SoundManager>();
        infoWindows = FindObjectsOfType<BaseDetailsWindow>();

        float value;

        // Loading Music / Setting default

        if (PlayerPrefs.HasKey("MusicVolume")) 
            value = PlayerPrefs.GetFloat("MusicVolume");
        else
            value = 1f;

        SetMusicVolume(value); // this does nothing, since we're disabled but updates the value hint
        SetMusicVolumeInternal(value);
        musicSlider.value = value;

        // Loading Sound / Setting default

        if (PlayerPrefs.HasKey("SoundVolume")) 
            value = PlayerPrefs.GetFloat("SoundVolume");
        else 
            value = 1f;

        SetSoundVolume(value); // this does nothing, since we're disabled but updates the value hint
        SetSoundVolumeInternal(value);
        soundSlider.value = value;

        // Loading Timer / Setting default

        if (PlayerPrefs.HasKey("NotificationTime"))
            value = PlayerPrefs.GetFloat("NotificationTime");
        else 
            value = 0f;

        SetInspectionWindowTime(value); // this does nothing, since we're disabled but updates the value hint
        SetInspectionWindowTimeInternal(value);
        inspectionTimeSlider.value = value;

        // Loading Bird/Worm Slider

        if (PlayerPrefs.HasKey("BirdWormSlider"))
            value = PlayerPrefs.GetFloat("BirdWormSlider");
        else
            value = UnityEngine.Random.Range(0, 1);

        SetBirdWorm(value); // this does nothing, since we're disabled but updates the value hint
        birdWormSlider.value = value;
    }

    public void ToggleVisibility() {
        gameObject.SetActive(!gameObject.activeSelf);
        Registry.Retrieve<TabletopManager>().SetPausedState(gameObject.activeInHierarchy);
    }

    public void RestartGame() {
        Registry.Retrieve<TabletopManager>().RestartGame();
        ToggleVisibility();
    }

    public void LeaveGame() {
        var tabletopManager = Registry.Retrieve<TabletopManager>();
        tabletopManager.SetPausedState(true);
        tabletopManager.SaveGame(true);

        SceneManager.LoadScene(SceneNumber.MenuScene);
    }

    // public button events

    public void SetMusicVolume(float volume) {
        musicSliderValue.text = volume * 10 + "%";

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SoundManager.PlaySfx("TokenHover");
        SetMusicVolumeInternal(volume);
    }

    public void SetSoundVolume(float volume) {
        soundSliderValue.text = volume * 10 + "%";

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SoundManager.PlaySfx("TokenHover");
        SetSoundVolumeInternal(volume);
    }

    public void SetInspectionWindowTime(float timer) {
        inspectionTimeSliderValue.text = GetInspectionTimeForValue(timer) + "s";

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SoundManager.PlaySfx("TokenHover");
        SetInspectionWindowTimeInternal(timer);
    }

    public void SetBirdWorm(float value) {
        birdWormSliderValue.text = (value > 0.5f ? "Bird" : "Worm");

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SoundManager.PlaySfx("TokenHover");
    }

    // Actual audio setting

    void SetMusicVolumeInternal(float volume) {
        if (backgroundMusic == null)
            return;

        backgroundMusic.SetVolume(GetClampedVol(volume) * musicVolMax);
        PlayerPrefs.SetFloat("MusicVolume", volume);

        if (volume == 0f && backgroundMusic.GetMute() == false)
            backgroundMusic.SetMute(true);
        else if (volume > 0f && backgroundMusic.GetMute())
            backgroundMusic.SetMute(false);
    }

    void SetSoundVolumeInternal(float volume) {
        if (soundManager == null)
            return;

        float dbVol = Mathf.Lerp(soundDbMin, soundDbMax, 1f - GetClampedVol(10f - volume));

        audioMixer.SetFloat("masterVol", dbVol);
        PlayerPrefs.SetFloat("SoundVolume", volume);
        SoundManager.PlaySfx("TokenHover");

        if (volume == 0f && soundManager.IsOn())
            soundManager.SetVolume(0f);
        else if (volume > 0f && soundManager.IsOn() == false)
            soundManager.SetVolume(1f);
    }

    float GetClampedVol(float sliderValue) {
        return Mathf.Pow(sliderValue / 10f, 2f); // slider has whole numbers only and goes from 0 to 10
    }

    void SetInspectionWindowTimeInternal(float value) {
        if (infoWindows == null || infoWindows.Length == 0)
            return;

        // value ranges from -4 to 1
        float timer = GetInspectionTimeForValue(value);
        PlayerPrefs.SetFloat("NotificationTime", value);

        for (int i = 0; i < infoWindows.Length; i++) 
            infoWindows[i].SetTimer(timer);
    }

    float GetInspectionTimeForValue(float value) {
        return baseInfoTimer + value * baseTimePerNotch; ;
    }


}
