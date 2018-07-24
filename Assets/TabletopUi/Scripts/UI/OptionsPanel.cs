#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Noon;
using UnityEngine.Audio;
using TMPro;
using Assets.TabletopUi.Scripts.Infrastructure;

public class OptionsPanel : MonoBehaviour {
    [Header("Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider inspectionTimeSlider;
    [SerializeField] private Slider autosaveSlider;
    [SerializeField] private Slider birdWormSlider;
    
    [SerializeField] private TextMeshProUGUI musicSliderValue;
    [SerializeField] private TextMeshProUGUI soundSliderValue;
    [SerializeField] private TextMeshProUGUI inspectionTimeSliderValue;
    [SerializeField] private TextMeshProUGUI autosaveSliderValue;
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

    [Header("Detail Windows")]
    [SerializeField]
    private AspectDetailsWindow aspectDetailsWindow;
    [SerializeField]
    private TokenDetailsWindow tokenDetailsWindow;

    SpeedController speedController;
    BackgroundMusic backgroundMusic;
    SoundManager soundManager;

    private const string MUSICVOLUME="MusicVolume";
    private const string SOUNDVOLUME = "SoundVolume";
    private const string NOTIFICATIONTIME = "NotificationTime";
    private const string AUTOSAVEINTERVAL = "AutosaveInterval";
    


    private bool pauseStateWhenOptionsRequested = false;

    public void InitPreferences( SpeedController spdctrl ) {
        gameObject.SetActive(false);

		speedController = spdctrl;
        backgroundMusic = FindObjectOfType<BackgroundMusic>();
        soundManager = FindObjectOfType<SoundManager>();
            //infoWindows = FindObjectsOfType<BaseDetailsWindow>(); //This code looks like it never worked. The windows were inactive when it ran. Was it never tested, or have I misunderstood something about the implementation? - AK


        float value;

        // Loading Music / Setting default

        if (PlayerPrefs.HasKey(MUSICVOLUME)) 
            value = PlayerPrefs.GetFloat(MUSICVOLUME);
        else
            value = 1f;

        SetMusicVolume(value); // this does nothing, since we're disabled but updates the value hint
        SetMusicVolumeInternal(value);
        musicSlider.value = value;

        // Loading Sound / Setting default

        if (PlayerPrefs.HasKey(SOUNDVOLUME)) 
            value = PlayerPrefs.GetFloat(SOUNDVOLUME);
        else 
            value = 1f;

        SetSoundVolume(value); // this does nothing, since we're disabled but updates the value hint
        SetSoundVolumeInternal(value);
        soundSlider.value = value;

        // Loading Timer / Setting default

        if (PlayerPrefs.HasKey(NOTIFICATIONTIME))
            value = PlayerPrefs.GetFloat(NOTIFICATIONTIME);
        else 
            value = 0f;

        SetInspectionWindowTime(value); // this does nothing, since we're disabled but updates the value hint
        SetInspectionWindowTimeInternal(value);
        inspectionTimeSlider.value = value;

        // Loading Autosave interval default

        if (PlayerPrefs.HasKey(AUTOSAVEINTERVAL))
            value = PlayerPrefs.GetFloat(AUTOSAVEINTERVAL);
        else 
            value = 5f;

        SetAutosaveInterval(value); // this does nothing, since we're disabled but updates the value hint
        SetAutosaveIntervalInternal(value);
        autosaveSlider.value = value;

        // Loading Bird/Worm Slider

        if (PlayerPrefs.HasKey(NoonConstants.BIRDWORMSLIDER))
            value = PlayerPrefs.GetFloat(NoonConstants.BIRDWORMSLIDER);
        else
            value = UnityEngine.Random.Range(0, 1);

        SetBirdWorm(value);
        birdWormSlider.value = value;
        NoonUtility.WormWar(value);
    }

	public bool GetVisibility()
	{
		return gameObject.activeSelf;
	}

    public void ToggleVisibility() {
        gameObject.SetActive(!gameObject.activeSelf);
		if (gameObject.activeInHierarchy)
		{
			pauseStateWhenOptionsRequested = Registry.Retrieve<TabletopManager>().GetPausedState();
			Registry.Retrieve<TabletopManager>().SetPausedState(true);
			speedController.LockToPause(true);
		}
		else
		{
			Registry.Retrieve<TabletopManager>().SetPausedState(pauseStateWhenOptionsRequested);
			speedController.LockToPause(false);
		}
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

    public void BrowseFiles()
	{
        OpenInFileBrowser.Open( NoonUtility.GetGameSaveLocation() );
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

    public void SetAutosaveInterval(float timer) {
		int mins = (int)timer;
        autosaveSliderValue.text = mins + " min";

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SoundManager.PlaySfx("TokenHover");
        SetAutosaveIntervalInternal(timer);
    }

    public void SetBirdWorm(float value) {
        birdWormSliderValue.text = (value > 0.5f ? "Bird" : "Worm");
        PlayerPrefs.SetFloat(NoonConstants.BIRDWORMSLIDER, value);

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SoundManager.PlaySfx("TokenHover");
    }

    // Actual audio setting

    void SetMusicVolumeInternal(float volume) {
        if (backgroundMusic == null)
            return;

        backgroundMusic.SetVolume(GetClampedVol(volume) * musicVolMax);
        PlayerPrefs.SetFloat(MUSICVOLUME, volume);

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
        PlayerPrefs.SetFloat(SOUNDVOLUME, volume);
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

        // value ranges from -4 to 1
        float timer = GetInspectionTimeForValue(value);
        PlayerPrefs.SetFloat(NOTIFICATIONTIME, value);
        aspectDetailsWindow.SetTimer(timer);
        tokenDetailsWindow.SetTimer(timer);

    }

    float GetInspectionTimeForValue(float value) {
        return baseInfoTimer + value * baseTimePerNotch;
    }

    void SetAutosaveIntervalInternal(float value)
	{
        // value ranges from 1 to 10 in mins
        PlayerPrefs.SetFloat(AUTOSAVEINTERVAL, value);

		var tabletopManager = Registry.Retrieve<TabletopManager>();
		tabletopManager.SetAutosaveInterval( value );
    }
}
