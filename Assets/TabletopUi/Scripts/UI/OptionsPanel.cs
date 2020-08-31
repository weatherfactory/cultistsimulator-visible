#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Noon;
using UnityEngine.Audio;
using TMPro;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.UI;
using UnityEngine.Analytics;

public class OptionsPanel : MonoBehaviour {



	[SerializeField] private GameObject windowGO;
    [SerializeField] private GameObject SettingControlPrefab;

    [Header("Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
	[SerializeField] private Slider inspectionTimeSlider;
	[SerializeField] private Slider screenCanvasSizeSlider;
    [SerializeField] private Slider autosaveSlider;
	[SerializeField] private Slider	snapGridSlider;
    [SerializeField] private Slider birdWormSlider;
    [SerializeField] private Slider contrastSlider;
    [SerializeField] private Slider accessibleCardsSlider;
    [SerializeField] private Slider resolutionSlider;
    [SerializeField] private Slider graphicsLevelSlider;
    [SerializeField] private Slider windowedSlider;

    [SerializeField] private TextMeshProUGUI musicSliderValue;
    [SerializeField] private TextMeshProUGUI soundSliderValue;
	[SerializeField] private TextMeshProUGUI inspectionTimeSliderValue;
	[SerializeField] private TextMeshProUGUI screenCanvasSizeSliderValue;
    [SerializeField] private TextMeshProUGUI autosaveSliderValue;
    [SerializeField] private TextMeshProUGUI birdWormSliderValue;
    [SerializeField] private TextMeshProUGUI snapGridSliderValue;
    [SerializeField] private TextMeshProUGUI contrastSliderValue;
    [SerializeField] private TextMeshProUGUI accessibleCardsSliderValue;

    [SerializeField] private TextMeshProUGUI resolutionValue;
    [SerializeField] private TextMeshProUGUI graphicsLevelValue;
    [SerializeField] private TextMeshProUGUI windowedValue;

    

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


    private bool _initialised = false;

    private List<GameObject> GameSettingsControls;
    private List<GameObject> SystemSettingsControls;
    private List<Resolution> availableResolutions;
    private int deferredResolutionChangeToIndex=-1;

    public void Update()
    {
        //eg: we don't want to change  resolution until the mouse button is released
        if (!Input.GetMouseButton(0))
            RunAnyDeferredCommands();
    }


    private bool IsInGame()
    {
        //This options panel class is used in both the main and the menu screen. IsInGame determines which version of behaviour. (You think this is iffy,
        //you shoulda seen the version where it was set with an editor tickbox and overwriting the prefab was a severity 1 error).

        return Registry.Get<StageHand>().SceneIsActive(SceneNumber.TabletopScene);
    }

    public void RunAnyDeferredCommands()
    {
        if(deferredResolutionChangeToIndex >=0)
        { 
            NoonUtility.Log("Res to " + getResolutionDescription(availableResolutions[deferredResolutionChangeToIndex]));
            Configuration.SetResolution(availableResolutions[deferredResolutionChangeToIndex]);
            deferredResolutionChangeToIndex = -1;
        }

    }

    public void Start()
    {
        var settings = Registry.Get<ICompendium>().GetEntitiesAsList<Setting>();


        foreach (var setting in settings)
        {
            var settingControl = Instantiate(SettingControlPrefab,gameObject.transform).GetComponent<SettingControl>();
            settingControl.Initialise(setting);
        }




      //  windowGO.SetActive(true); //so we can use tags. SO HACKY

  //      GameSettingsControls = new List<GameObject>(GameObject.FindGameObjectsWithTag("GameSetting")) ;
	 //   SystemSettingsControls = new List<GameObject>(GameObject.FindGameObjectsWithTag("SystemSetting")) ;
      

	 //   foreach(var c in GameSettingsControls)
	 //       c.SetActive(true);

	 //   foreach(var c in SystemSettingsControls)
	 //       c.SetActive(false);
        

		//windowGO.SetActive(false);



        if (!IsInGame())
        {
            //resolutions!
            int r;
            availableResolutions= new List<Resolution>(Screen.resolutions);
            resolutionSlider.minValue = 0;
            resolutionSlider.maxValue = availableResolutions.Count-1;
            resolutionSlider.wholeNumbers = true;
            if (PlayerPrefs.HasKey(NoonConstants.RESOLUTION))
                r = PlayerPrefs.GetInt(NoonConstants.RESOLUTION);
            else
            {
                r = availableResolutions.FindIndex(res =>
                    res.height == Screen.height && res.width == Screen.width);
              if(r==-1)
                  r = availableResolutions.Count / 2;
            }
            
            resolutionSlider.value = r;
            SetResolutionDeferred(r);
            
            //windowedness!
            float w;
            if (PlayerPrefs.HasKey(NoonConstants.WINDOWED))
                w = PlayerPrefs.GetFloat(NoonConstants.WINDOWED);
            else
            {
                w = Screen.fullScreen ? 0f : 1f;
            }

            SetWindowed(w);
            windowedSlider.value = w;

            //graphics quality!
            float g;
            if (PlayerPrefs.HasKey(NoonConstants.GRAPHICSLEVEL))
                g = PlayerPrefs.GetInt(NoonConstants.GRAPHICSLEVEL);
            else
                g = 3;
            
            SetGraphicsLevel(g);
            graphicsLevelSlider.value = g;
        }


        // Loading Music / Setting default

	    float value;


     //   if (PlayerPrefs.HasKey(NoonConstants.MUSICVOLUME))
     //       value = PlayerPrefs.GetFloat(NoonConstants.MUSICVOLUME);
     //   else
     //       value = 10f;

     //   //SetMusicVolume(value); // this does nothing, since we're disabled but updates the value hint
     ////   SetMusicVolumeInternal(value);
     //   musicSlider.value = value;

        // Loading Sound / Setting default

        if (PlayerPrefs.HasKey(NoonConstants.SOUNDVOLUME))
            value = PlayerPrefs.GetFloat(NoonConstants.SOUNDVOLUME);
        else
            value = 10f;

        SetSoundVolume(value); // this does nothing, since we're disabled but updates the value hint
        SetSoundVolumeInternal(value);
        soundSlider.value = value;

        // Loading Timer / Setting default

        if (PlayerPrefs.HasKey(NoonConstants.NOTIFICATIONTIME))
            value = PlayerPrefs.GetFloat(NoonConstants.NOTIFICATIONTIME);
        else
            value = 10f;	// Default to maximum (now 30s)

        SetInspectionWindowTime(value); // this does nothing, since we're disabled but updates the value hint
        SetInspectionWindowTimeInternal(value);
		inspectionTimeSlider.value = value;

		// Loading Timer / Setting default

		if (PlayerPrefs.HasKey(NoonConstants.SCREENCANVASSIZE))
			value = PlayerPrefs.GetFloat(NoonConstants.SCREENCANVASSIZE);
		else
			value = 1f; // Set default scale to 100%

		SetCanvasScaleSize(value); // this does nothing, since we're disabled but updates the value hint
		SetCanvasScaleSizeInternal(value);
		screenCanvasSizeSlider.value = value;

        // Loading Autosave interval default

        if (PlayerPrefs.HasKey(NoonConstants.AUTOSAVEINTERVAL))
            value = PlayerPrefs.GetFloat(NoonConstants.AUTOSAVEINTERVAL);
        else
            value = 5f;

        SetAutosaveInterval(value); // this does nothing, since we're disabled but updates the value hint
        SetAutosaveIntervalInternal(value);
        autosaveSlider.value = value;

        // Loading Grid Snap Size Slider
        if (PlayerPrefs.HasKey(NoonConstants.GRIDSNAPSIZE))
            value = PlayerPrefs.GetFloat(NoonConstants.GRIDSNAPSIZE);
        else
            value = 1f;

        SetSnapGrid(value);
		SetSnapGridInternal(value);
        snapGridSlider.value = value;

        // Loading Bird/Worm Slider
        if (PlayerPrefs.HasKey(NoonConstants.BIRDWORMSLIDER))
            value = PlayerPrefs.GetInt(NoonConstants.BIRDWORMSLIDER);
        else
            value = 0;

        SetBirdWorm(value);
        birdWormSlider.value = value;


        if (PlayerPrefs.HasKey(NoonConstants.HIGHCONTRAST))
            value = PlayerPrefs.GetFloat(NoonConstants.HIGHCONTRAST);
        else
            value = 0f;

        SetHighContrast(value);
        contrastSlider.value = value;
        
        
        if (PlayerPrefs.HasKey(NoonConstants.ACCESSIBLECARDS))
	        value = PlayerPrefs.GetFloat(NoonConstants.ACCESSIBLECARDS);
        else
	        value = 0f;

        SetAccessibleCards(value);
        accessibleCardsSlider.value = value;

	    _initialised = true;
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

        if (!IsInGame())
			return;

        //reset the state on the must-confirm Restart button. doesn't matter if this is closing or opening,
        //let's reset it either way
        restartButton.ResetState();

		// Simplified to use Martin's LockToPause code which handles everything nicely - CP
        if (windowGO.activeInHierarchy)
		
			// now lock the pause so players can't do it manually - this also pauses
			Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs{ControlPriorityLevel = 3,GameSpeed = GameSpeed.Paused,WithSFX = false});
		
		else
            Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Unspecified, WithSFX = false });

		RefreshOptionsText();
    }

    public void RestartGame() {
		if (!IsInGame())
			return;
        if(restartButton.AttemptRestart())
        {
            ToggleVisibility();
            Registry.Get<StageHand>().NewGameOnTabletop();

        }
    }

	public async void LeaveGame()
	{
		if (!IsInGame())
			return;

        var tabletopManager = Registry.Get<TabletopManager>();
        tabletopManager.SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });
        var saveTask = tabletopManager.SaveGameAsync(true, SourceForGameState.DefaultSave);

        var success = await saveTask;


	        if (success)
	        {
                Registry.Get<StageHand>().MenuScreen();
            }
	        else
	        {
		        // Save failed, need to let player know there's an issue
		        // Autosave would wait and retry in a few seconds, but player is expecting results NOW.
		        ToggleVisibility();
		        Registry.Get<Assets.Core.Interfaces.INotifier>().ShowSaveError(true);
	        }
        
	}

	// Leave game without saving
	public void AbandonGame()
	{
        Registry.Get<StageHand>().MenuScreen();
    }

    public async void BrowseFiles()
    {
	    // Check for the existence of save file before browsing to it, since behaviour is undefined in that case
	    string savePath = NoonUtility.GetGameSaveLocation();
	    if (!File.Exists(savePath))
	    {
		    if (IsInGame())
            {
                var saveTask = Registry.Get<TabletopManager>().SaveGameAsync(true, SourceForGameState.DefaultSave);

                var success = await saveTask;


				    // If a game is active, try to save it, showing an error if that fails
				    if (!success)
				    {
					    ToggleVisibility();
					    Registry.Get<Assets.Core.Interfaces.INotifier>().ShowSaveError(true);
					    OpenInFileBrowser.Open(savePath);
				    }

                    return;
		    }

		    // Otherwise, just show the directory where the save file would be located
		    savePath = Path.GetDirectoryName(savePath);
	    }
        OpenInFileBrowser.Open(savePath);
    }

    public void ManageSaves( bool open )
	{
		this.gameObject.SetActive( !open );
		manageSavesWindow.SetActive( open );
    }

    public void SwitchSettingsDisplay(bool showSystemSettings)
    {
        foreach(var c in GameSettingsControls)
            c.SetActive(!showSystemSettings);

        foreach(var c in SystemSettingsControls)
            c.SetActive(showSystemSettings);
    }

	public void SaveErrorContinue()
	{
		// Just close window and resume play
		Registry.Get<Assets.Core.Interfaces.INotifier>().ShowSaveError( false );
	}

	public void SaveErrorReload()
	{
		// Reload last good savegame
		Registry.Get<Assets.Core.Interfaces.INotifier>().ShowSaveError(false);

		Registry.Get<TabletopManager>().LoadGame(SourceForGameState.DefaultSave);
	}

    // public button events

  //  public void SetMusicVolume(float volume) {
  //      musicSliderValue.text = volume * 10 + "%";

  //      if (gameObject.activeInHierarchy == false)
  //          return; // don't update anything if we're not visible.

  //      SetMusicVolumeInternal(volume);
		//SoundManager.PlaySfx("UISliderMove");
  //  }

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

    public void SetBirdWorm(float value)
	{
        PlayerPrefs.SetInt(NoonConstants.BIRDWORMSLIDER, (int)value);
		RefreshOptionsText();

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

		SoundManager.PlaySfx("UISliderMove");
    }

    public void SetHighContrast(float value)
	{
		PlayerPrefs.SetFloat(NoonConstants.HIGHCONTRAST, value);
		SetHighContrastInternal(value);
		RefreshOptionsText();

		if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

		SoundManager.PlaySfx("UISliderMove");
    }

    public void SetAccessibleCards(float value)
    {
	    PlayerPrefs.SetFloat(NoonConstants.ACCESSIBLECARDS, value);
	    TabletopManager.SetAccessibleCards(value > 0.5f);
	    RefreshOptionsText();

	    if (gameObject.activeInHierarchy == false)
		    return; // don't update anything if we're not visible.

	    SoundManager.PlaySfx("UISliderMove");
    }


    public void SetResolutionDeferred(float value)
    {
        int r = Convert.ToInt32(value);
        PlayerPrefs.SetInt(NoonConstants.RESOLUTION,r );

        RefreshOptionsText();

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.
        else
        { 
                deferredResolutionChangeToIndex = r;
          SoundManager.PlaySfx("UISliderMove");
        }
    }

    
    public void SetWindowed(float value)
    {
        PlayerPrefs.SetFloat(NoonConstants.WINDOWED, value);

        Configuration.SetWindowed(value > 0.5f);
        RefreshOptionsText();

        if (gameObject.activeInHierarchy == false)
            return; // don't update anything if we're not visible.

        SoundManager.PlaySfx("UISliderMove");
    }

    public void SetGraphicsLevel(float value)
    {
        int g = Convert.ToInt32(value);
        PlayerPrefs.SetInt(NoonConstants.GRAPHICSLEVEL, g);
        Configuration.SetGraphicsLevel(g);
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
        if (!_initialised)
            return;
        int mins = (int) PlayerPrefs.GetFloat(NoonConstants.AUTOSAVEINTERVAL);


        // Inspect time
        inspectionTimeSliderValue.text = GetInspectionTimeForValue(PlayerPrefs.GetFloat(NoonConstants.NOTIFICATIONTIME)) +
                                         Registry.Get<ILanguageManager>().Get("UI_SECONDS_POSTFIX");

        // Autosave

        autosaveSliderValue.text = mins + Registry.Get<ILanguageManager>().Get("UI_MINUTES_POSTFIX");

        // Snap grid
        int snap = Mathf.RoundToInt(PlayerPrefs.GetFloat(NoonConstants.GRIDSNAPSIZE));
        switch (snap)
        {
            default:
            case 0:
                snapGridSliderValue.text = Registry.Get<ILanguageManager>().Get("UI_SNAP_0");
                break;
            case 1:
                snapGridSliderValue.text = Registry.Get<ILanguageManager>().Get("UI_SNAP_1");
                break;
            case 2:
                snapGridSliderValue.text = Registry.Get<ILanguageManager>().Get("UI_SNAP_2");
                break;
            case 3:
                snapGridSliderValue.text = Registry.Get<ILanguageManager>().Get("UI_SNAP_3");
                break;
        }

        // Bird/worm
        birdWormSliderValue.text =
            Registry.Get<ILanguageManager>().Get(PlayerPrefs.GetInt(NoonConstants.BIRDWORMSLIDER) > 0 ? "UI_WORM" : "UI_BIRD");

        // High Contrast
        contrastSliderValue.text =
            Registry.Get<ILanguageManager>().Get(PlayerPrefs.GetFloat(NoonConstants.HIGHCONTRAST) > 0.5f ? "UI_ON" : "UI_OFF");

        // Accessible Cards
        accessibleCardsSliderValue.text =
            Registry.Get<ILanguageManager>().Get(PlayerPrefs.GetFloat(NoonConstants.ACCESSIBLECARDS) > 0.5f ? "UI_ON" : "UI_OFF");

        if (!IsInGame())
        {
        

       resolutionValue.text = getResolutionDescription(availableResolutions[PlayerPrefs.GetInt(NoonConstants.RESOLUTION)]);
	    windowedValue.text = Registry.Get<ILanguageManager>().Get( PlayerPrefs.GetFloat(NoonConstants.WINDOWED) > 0.5f ? "UI_ON" : "UI_OFF");
	        int graphicsLevel =  PlayerPrefs.GetInt(NoonConstants.GRAPHICSLEVEL);
	        switch (graphicsLevel)
	        {
	            case 1:		graphicsLevelValue.text = Registry.Get<ILanguageManager>().Get( "GRAPHICS_LEVEL_1" ); break;
	            case 2:		graphicsLevelValue.text = Registry.Get<ILanguageManager>().Get( "GRAPHICS_LEVEL_2" ); break;
	            case 3:		graphicsLevelValue.text = Registry.Get<ILanguageManager>().Get( "GRAPHICS_LEVEL_3" ); break;
	        }

        }
        
	}



    // INTERNAL Setters

    //void SetMusicVolumeInternal(float volume) {
    //    if (backgroundMusic == null)
    //        return;

    //    backgroundMusic.SetVolume(GetClampedVol(volume) * musicVolMax);
    //    PlayerPrefs.SetFloat(NoonConstants.MUSICVOLUME, volume);

    //    if (volume == 0f && backgroundMusic.GetMute() == false)
    //        backgroundMusic.SetMute(true);
    //    else if (volume > 0f && backgroundMusic.GetMute())
    //        backgroundMusic.SetMute(false);
    //}

    void SetSoundVolumeInternal(float volume) {
        if (SoundManager.Instance == null)
            return;

        float dbVol = Mathf.Lerp(soundDbMin, soundDbMax, 1f - GetClampedVol(10f - volume));

        audioMixer.SetFloat("masterVol", dbVol);
        PlayerPrefs.SetFloat(NoonConstants.SOUNDVOLUME, volume);

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
		PlayerPrefs.SetFloat(NoonConstants.NOTIFICATIONTIME, value);

		if (!IsInGame())
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
		PlayerPrefs.SetFloat(NoonConstants.SCREENCANVASSIZE, value);

		if (!IsInGame())
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
        PlayerPrefs.SetFloat(NoonConstants.AUTOSAVEINTERVAL, value);

		if (!IsInGame())
			return;

		var tabletopManager = Registry.Get<TabletopManager>();
		tabletopManager.SetAutosaveInterval( value );
    }

    void SetSnapGridInternal(float value)
	{
        // value ranges from 0 to 10
        PlayerPrefs.SetFloat(NoonConstants.GRIDSNAPSIZE, value);

		if (!IsInGame())
			return;

		var tabletopManager = Registry.Get<TabletopManager>();
		tabletopManager.SetGridSnapSize( value );
    }

    void SetHighContrastInternal(float value)
	{
        // value ranges from 0 to 1
        PlayerPrefs.SetFloat(NoonConstants.HIGHCONTRAST, value);
		TabletopManager.SetHighContrast( value>0f );
    }

    private string getResolutionDescription(Resolution r)
    {
        string desc = r.width + "\n x \n" + r.height;
        return desc;
    }

}
