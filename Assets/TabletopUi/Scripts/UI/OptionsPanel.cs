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

    
	
	[SerializeField] private GameObject windowGO;

    [Header("Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
	[SerializeField] private Slider inspectionTimeSlider;
	[SerializeField] private Slider screenCanvasSizeSlider;
    [SerializeField] private Slider autosaveSlider;
	[SerializeField] private Slider	snapGridSlider;
    [SerializeField] private Slider birdWormSlider;
    
    [SerializeField] private TextMeshProUGUI musicSliderValue;
    [SerializeField] private TextMeshProUGUI soundSliderValue;
	[SerializeField] private TextMeshProUGUI inspectionTimeSliderValue;
	[SerializeField] private TextMeshProUGUI screenCanvasSizeSliderValue;
    [SerializeField] private TextMeshProUGUI autosaveSliderValue;
    [SerializeField] private TextMeshProUGUI birdWormSliderValue;
    [SerializeField] private TextMeshProUGUI snapGridSliderValue;

    [SerializeField] private RestartButton restartButton;

    [SerializeField] private GameObject manageSavesWindow;
    [SerializeField] private GameObject optionsWindow;

    [Header("Settings")]
    [Tooltip("Music volume goes from 0 to 1")]
    public float musicVolMax = 0.25f;

    [Tooltip("Sound volume uses DB ranges ")]
    public float soundDbMin = -40f;
    public float soundDbMax = 10f;

    [Tooltip("Timer is baseTimer + notches * timePerNotch, with the second notch from the right being 0")]
    public float baseInfoTimer = 10f;
    public float baseTimePerNotch = 2f;

	public float scaleSliderFactor = 0.5f;

    [Header("External Ref")]
    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    BackgroundMusic backgroundMusic;

    [Header("Detail Windows")]
    [SerializeField]
    private AspectDetailsWindow aspectDetailsWindow;
    [SerializeField]
    private TokenDetailsWindow tokenDetailsWindow;

	[Header("ScreenCanvas")]
	[SerializeField]
	private CanvasScaleManager screenCanvasScaler;

    
    SpeedController speedController;
    

    private const string MUSICVOLUME="MusicVolume";
    private const string SOUNDVOLUME = "SoundVolume";
    private const string NOTIFICATIONTIME = "NotificationTime";
	private const string SCREENCANVASSIZE = "ScreenCanvasSize";
    private const string AUTOSAVEINTERVAL = "AutosaveInterval";
    private const string GRIDSNAPSIZE = "GridSnapSize";

    //This options panel class is used in both the main and the menu screen. IsInGame determines which version of behaviour. (You think this is iffy,
    //you shoulda seen the version where it was set with an editor tickbox and overwriting the prefab was a severity 1 error).
    private bool _isInGame = true;

    public void InitPreferences( SpeedController spdctrl,bool isInGame)
	{	
		windowGO.SetActive(false);
        _isInGame = isInGame;

		if (_isInGame) {
			speedController = spdctrl;
		}

        float value;

        // Loading Music / Setting default

        if (PlayerPrefs.HasKey(MUSICVOLUME)) 
            value = PlayerPrefs.GetFloat(MUSICVOLUME);
        else
            value = 10f;

        SetMusicVolume(value); // this does nothing, since we're disabled but updates the value hint
        SetMusicVolumeInternal(value);
        musicSlider.value = value;

        // Loading Sound / Setting default

        if (PlayerPrefs.HasKey(SOUNDVOLUME)) 
            value = PlayerPrefs.GetFloat(SOUNDVOLUME);
        else 
            value = 10f;

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

		// Loading Timer / Setting default

		if (PlayerPrefs.HasKey(SCREENCANVASSIZE))
			value = PlayerPrefs.GetFloat(SCREENCANVASSIZE);
		else 
			value = 1f; // Set default scale to 100%

		SetCanvasScaleSize(value); // this does nothing, since we're disabled but updates the value hint
		SetCanvasScaleSizeInternal(value);
		screenCanvasSizeSlider.value = value;

        // Loading Autosave interval default

        if (PlayerPrefs.HasKey(AUTOSAVEINTERVAL))
            value = PlayerPrefs.GetFloat(AUTOSAVEINTERVAL);
        else 
            value = 5f;

        SetAutosaveInterval(value); // this does nothing, since we're disabled but updates the value hint
        SetAutosaveIntervalInternal(value);
        autosaveSlider.value = value;

        // Loading Grid Snap Size Slider
        if (PlayerPrefs.HasKey(GRIDSNAPSIZE))
            value = PlayerPrefs.GetFloat(GRIDSNAPSIZE);
        else
            value = 1f;

        SetSnapGrid(value);
		SetSnapGridInternal(value);
        snapGridSlider.value = value;

        // Loading Bird/Worm Slider

        if (PlayerPrefs.HasKey(NoonConstants.BIRDWORMSLIDER))
            value = PlayerPrefs.GetFloat(NoonConstants.BIRDWORMSLIDER);
        else
            value = UnityEngine.Random.Range(0, 1);

        SetBirdWorm(value);
        birdWormSlider.value = value;
        NoonUtility.WormWar(value);
    }

    private void OnEnable()
    {
        RefreshOptionsText();
    }

	public bool GetVisibility()
	{
		return windowGO.activeSelf;
	}

    public void ToggleVisibility() {
		windowGO.SetActive(!windowGO.activeSelf);
        
        if (!_isInGame)
			return;

        //reset the state on the must-confirm Restart button. doesn't matter if this is closing or opening,
        //let's reset it either way
        restartButton.ResetState();

		// Simplified to use Martin's LockToPause code which handles everything nicely - CP
        if (windowGO.activeInHierarchy)
		{
			// now lock the pause so players can't do it manually - this also pauses
			if (speedController != null)
				speedController.LockToPause(true);
		}
		else
		{
			if (speedController != null)
				speedController.LockToPause(false);
		}

		RefreshOptionsText();
    }

    public void RestartGame() {
		if (!_isInGame)
			return;
        if(restartButton.AttemptRestart())
        { 
		
        Registry.Retrieve<TabletopManager>().RestartGame();
        ToggleVisibility();
        }
    }

	public void LeaveGame()
	{
		if (!_isInGame)
			return;
		
        var tabletopManager = Registry.Retrieve<TabletopManager>();
        tabletopManager.SetPausedState(true);
        if (tabletopManager.SaveGame(true))
		{
			// Save was successful, OK to exit
	        SceneManager.LoadScene(SceneNumber.MenuScene);
		}
		else
		{
			// Save failed, need to let player know there's an issue
			// Autosave would wait and retry in a few seconds, but player is expecting results NOW.
			ToggleVisibility();
			Registry.Retrieve<Assets.Core.Interfaces.INotifier>().ShowSaveError( true );
		}
    }

	// Leave game without saving
	public void AbandonGame()
	{
	    SceneManager.LoadScene(SceneNumber.MenuScene);
    }

    public void BrowseFiles()
	{
        OpenInFileBrowser.Open( NoonUtility.GetGameSaveLocation() );
    }

    public void ManageSaves( bool open )
	{
		this.gameObject.SetActive( !open );
		manageSavesWindow.SetActive( open );  
    }

	public void SaveErrorContinue()
	{
		// Just close window and resume play
		Registry.Retrieve<Assets.Core.Interfaces.INotifier>().ShowSaveError( false );
	}

	public void SaveErrorReload()
	{
		// Reload last good savegame
		Registry.Retrieve<Assets.Core.Interfaces.INotifier>().ShowSaveError(false);

		Registry.Retrieve<TabletopManager>().LoadGame();
	}

    // public button events

    public void SetMusicVolume(float volume) {
        musicSliderValue.text = volume * 10 + "%";

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SetMusicVolumeInternal(volume);
		SoundManager.PlaySfx("UISliderMove");
    }

    public void SetSoundVolume(float volume) {
        soundSliderValue.text = volume * 10 + "%";

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SetSoundVolumeInternal(volume);
		SoundManager.PlaySfx("UISliderMove"); // After So we have the sound applied
    }

    public void SetInspectionWindowTime(float timer){

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SetInspectionWindowTimeInternal(timer);
		RefreshOptionsText();
		SoundManager.PlaySfx("UISliderMove");
	}

	public void SetCanvasScaleSize(float size) {
		screenCanvasSizeSliderValue.text = GetCanvasScaleForValue(size) * 100 + "%";

		if (gameObject.activeInHierarchy == false)
			return; // don't update anything if we're not visible.

		SoundManager.PlaySfx("UISliderMove");
		SetCanvasScaleSizeInternal(size);
	}

    public void SetAutosaveInterval(float timer) {

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SetAutosaveIntervalInternal(timer);
		RefreshOptionsText();
		SoundManager.PlaySfx("UISliderMove");
    }

    public void SetBirdWorm(float value) {
        PlayerPrefs.SetFloat(NoonConstants.BIRDWORMSLIDER, value);
		RefreshOptionsText();

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

		SoundManager.PlaySfx("UISliderMove");
    }

    public void SetSnapGrid(float value)
	{
        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.
		
		SetSnapGridInternal(value);
		RefreshOptionsText();
		SoundManager.PlaySfx("UISliderMove");
    }

	public void RefreshOptionsText()
	{

	    int mins = (int)PlayerPrefs.GetFloat(AUTOSAVEINTERVAL);

	    try
	    {

	    
        // Inspect time
        inspectionTimeSliderValue.text = GetInspectionTimeForValue( PlayerPrefs.GetFloat(NOTIFICATIONTIME) ) + LanguageTable.Get("UI_SECONDS_POSTFIX");

		// Autosave

        autosaveSliderValue.text = mins + LanguageTable.Get( "UI_MINUTES_POSTFIX" );

		// Snap grid
		int snap = Mathf.RoundToInt( PlayerPrefs.GetFloat(GRIDSNAPSIZE) );
		switch (snap)
		{
		default:
		case 0:		snapGridSliderValue.text = LanguageTable.Get( "UI_SNAP_0" ); break;
		case 1:		snapGridSliderValue.text = LanguageTable.Get( "UI_SNAP_1" ); break;		
		case 2:		snapGridSliderValue.text = LanguageTable.Get( "UI_SNAP_2" ); break;
		case 3:		snapGridSliderValue.text = LanguageTable.Get( "UI_SNAP_3" ); break;
		}

		// Bird/worm
        
        birdWormSliderValue.text = LanguageTable.Get( PlayerPrefs.GetFloat(NoonConstants.BIRDWORMSLIDER) > 0.5f ? "UI_BIRD" : "UI_WORM" );
	    }
	    catch (NullReferenceException)
	    {
	        NoonUtility.Log("Not currently using loc text for options panel controls - needs fix from loc branch for nullref when called early",10);
	    }
	}

    // INTERNAL Setters

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
        if (SoundManager.Instance == null)
            return;

        float dbVol = Mathf.Lerp(soundDbMin, soundDbMax, 1f - GetClampedVol(10f - volume));

        audioMixer.SetFloat("masterVol", dbVol);
        PlayerPrefs.SetFloat(SOUNDVOLUME, volume);

        if (volume == 0f && SoundManager.Instance.IsOn())
            SoundManager.Instance.SetVolume(0f);
        else if (volume > 0f && SoundManager.Instance.IsOn() == false)
            SoundManager.Instance.SetVolume(1f);
    }

    float GetClampedVol(float sliderValue) {
        return Mathf.Pow(sliderValue / 10f, 2f); // slider has whole numbers only and goes from 0 to 10
    }

	// 

	void SetInspectionWindowTimeInternal(float value) {
		// value ranges from -4 to 1
		float timer = GetInspectionTimeForValue(value);
		PlayerPrefs.SetFloat(NOTIFICATIONTIME, value);

		if (!_isInGame)
			return;

		if (aspectDetailsWindow != null)
			aspectDetailsWindow.SetTimer(timer);

		if (tokenDetailsWindow != null)
			tokenDetailsWindow.SetTimer(timer);
	}

	float GetInspectionTimeForValue(float value) {
		return baseInfoTimer + value * baseTimePerNotch;
	}

	//

	void SetCanvasScaleSizeInternal(float value) {
		// value ranges from 0.5 to 2
		float scale = GetCanvasScaleForValue(value);
		scale = Mathf.Max( scale, 0.75f );	// ensure we never get tiny menu scale - CP
		PlayerPrefs.SetFloat(SCREENCANVASSIZE, value);

		if (!_isInGame)
			return;

		screenCanvasScaler.SetTargetScaleFactor(scale);
	}

	float GetCanvasScaleForValue(float value) {
		return value * scaleSliderFactor;
	}

	// 

    void SetAutosaveIntervalInternal(float value)
	{
        // value ranges from 1 to 10 in mins
        PlayerPrefs.SetFloat(AUTOSAVEINTERVAL, value);

		if (!_isInGame)
			return;
		
		var tabletopManager = Registry.Retrieve<TabletopManager>();
		tabletopManager.SetAutosaveInterval( value );
    }

    void SetSnapGridInternal(float value)
	{
        // value ranges from 0 to 10
        PlayerPrefs.SetFloat(GRIDSNAPSIZE, value);

		if (!_isInGame)
			return;
		
		var tabletopManager = Registry.Retrieve<TabletopManager>();
		tabletopManager.SetGridSnapSize( value );
    }
}
