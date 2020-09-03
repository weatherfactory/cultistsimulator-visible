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
using UIWidgets;
using UnityEngine.Analytics;

public class OptionsPanel : MonoBehaviour {



    [SerializeField] private GameObject SettingControlPrefab;
    [SerializeField] private GameObject TabControlPrefab;
    [SerializeField] private Transform TabsHere;
    [SerializeField] private Transform SettingsHere;


	[Header("Controls")]
    
    [SerializeField] private RestartButton restartButton;
    [SerializeField] private GameObject manageSavesWindow;
    [SerializeField] private GameObject optionsWindow;

	


    private bool IsInGame()
    {
        //This options panel class is used in both the main and the menu screen. IsInGame determines which version of behaviour. (You think this is iffy,
        //you shoulda seen the version where it was set with an editor tickbox and overwriting the prefab was a severity 1 error).

        return Registry.Get<StageHand>().SceneIsActive(SceneNumber.TabletopScene);
    }



    public void Start()
    {
        var settings = Registry.Get<ICompendium>().GetEntitiesAsList<Setting>();


        foreach(Transform editTimeTab in TabsHere)
            Destroy(editTimeTab.gameObject);

        var tabs = settings.Select(s => s.TabId).Distinct();
        foreach (var tabName in tabs)
        {
            var tabComponent = Instantiate(TabControlPrefab, TabsHere).GetComponent<OptionsPanelTab>();
            tabComponent.Initialise(tabName);
        }


        foreach (Transform editTimeSetting in SettingsHere)
            Destroy(editTimeSetting.gameObject);



        foreach (var setting in settings)
        {
            var settingControl = Instantiate(SettingControlPrefab,SettingsHere).GetComponent<SettingControl>();
            settingControl.Initialise(setting);
        }

	}

    private void OnEnable()
    {
    
       //RefreshOptionsText();
    }


    public void ToggleVisibility() {
		gameObject.SetActive(!gameObject.activeSelf);

        if (!IsInGame())
			return;

        //reset the state on the must-confirm Restart button. doesn't matter if this is closing or opening,
        //let's reset it either way
        restartButton.ResetState();

		// Simplified to use Martin's LockToPause code which handles everything nicely - CP
        if (gameObject.activeInHierarchy)
		
			// now lock the pause so players can't do it manually - this also pauses
			Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs{ControlPriorityLevel = 3,GameSpeed = GameSpeed.Paused,WithSFX = false});
		
		else
            Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Unspecified, WithSFX = false });

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




}
