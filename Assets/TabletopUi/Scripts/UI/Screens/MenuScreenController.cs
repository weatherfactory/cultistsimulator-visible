﻿// based on LoadingScreenManager
// --------------------------------
// built by Martin Nerurkar (http://www.martin.nerurkar.de)
// for Nowhere Prophet (http://www.noprophet.com)
//
// Licensed under GNU General Public License v3.0
// http://www.gnu.org/licenses/gpl-3.0.txt

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuScreenController : MonoBehaviour {

    // can be used to disable interaction when we start loading into a scene
    public EventSystem eventSystem;

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueGameButton;
    public Button purgeButton;

    [Header("Overlays")]
    public CanvasGroupFader modal;
    public CanvasGroupFader purgeConfirm;
    public CanvasGroupFader credits;
    public CanvasGroupFader versionHints;

    [Header("Fade Visuals")]
    public Image fadeOverlay;
    public float fadeDuration = 0.25f;

    [Header("Hints")]
    public GameObject purgeSaveMessage;
    public TextMeshProUGUI VersionNumber;
    public Animation versionAnim;

    bool canTakeInput;
    int sceneToLoad;
    VersionNumber currentVersion;
    CanvasGroupFader currentOverlay;

    GameSaveManager saveGameManager;

    void Start() {
        // make sure the screen is black
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.canvasRenderer.SetAlpha(1f);

        InitManagers();
        canTakeInput = true;

        // We delay the showing to get a proper fade in
        Invoke("UpdateAndShowMenu", 0.1f); 
    }

    void InitManagers() {
        var registry = new Registry();

        var compendium = new Compendium();
        registry.Register<ICompendium>(compendium);

        var contentImporter = new ContentImporter();
        contentImporter.PopulateCompendium(compendium);

        var metaInfo = new MetaInfo(NoonUtility.VersionNumber);
        registry.Register<MetaInfo>(metaInfo);
        CrossSceneState.SetMetaInfo(metaInfo);

        saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

        currentVersion = metaInfo.VersionNumber;
    }

    void UpdateAndShowMenu() {
        bool hasSavegame = saveGameManager.DoesGameSaveExist();
        bool isLegalSaveGame = hasSavegame ? saveGameManager.SaveGameHasMatchingVersionNumber(currentVersion) : false;

        // Show the buttons as needed
        newGameButton.gameObject.SetActive(!hasSavegame);
        continueGameButton.gameObject.SetActive(hasSavegame);
        continueGameButton.interactable = isLegalSaveGame;
        purgeButton.gameObject.SetActive(hasSavegame);

        // Show the purge message if needed
        purgeSaveMessage.gameObject.SetActive(hasSavegame && !isLegalSaveGame);

        UpdateVersionNumber(!isLegalSaveGame);
        HideAllOverlays();
        FadeIn();
    }

    void UpdateVersionNumber(bool hasNews) {
        // Show the current version number
        VersionNumber.text = currentVersion.Version;

        if (hasNews)
            versionAnim.Play();
        else
            versionAnim.Stop();
    }

    void HideAllOverlays() {
        modal.gameObject.SetActive(false);
        purgeConfirm.gameObject.SetActive(false);
        credits.gameObject.SetActive(false);
        versionHints.gameObject.SetActive(false);
    }

    #region -- View Changes ------------------------

    void FadeIn() {
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.canvasRenderer.SetAlpha(1f);
        fadeOverlay.CrossFadeAlpha(0, fadeDuration, true);
    }

    void FadeOut() {
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.canvasRenderer.SetAlpha(0f);
        fadeOverlay.CrossFadeAlpha(1, fadeDuration, true);
    }

    void ShowOverlay(CanvasGroupFader overlay) {
        if (currentOverlay != null)
            return;

        currentOverlay = overlay;

        overlay.Show();
        modal.Show();
    }

    void HideCurrentOverlay() {
        if (currentOverlay == null)
            return;

        currentOverlay.Hide();
        modal.Hide();
        currentOverlay = null;
    }

    #endregion

    #region -- User Actions via Scene Buttons ------------------------

    public void StartGame() {
        if (!canTakeInput)
            return;

        // Set the legacy to the first in the list; this should be the starting legacy
        CrossSceneState.SetChosenLegacy(Registry.Retrieve<ICompendium>().GetAllLegacies().First());
        // Load directly into the game scene, no legacy select
        LoadScene(SceneNumber.GameScene);
    }

    public void ContinueGame() {
        if (!canTakeInput)
            return;

        if (saveGameManager.IsSavedGameActive()) {
            //back into the game!
            LoadScene(SceneNumber.GameScene);
            return;
        }

        //we left the game from the game over or legacy screen: go back to choose a legacy.
        //Retrieve the saved state, and then selectively populate properties in the current state...
        //this is ugly: it could use appropriate methods
        var savedCrossSceneState = saveGameManager.RetrieveSavedCrossSceneState();

        //this is essential. If we earlier left the game in the legacy screen, we're about to go back there, 
        //and we need to retrieve the legacy ids and populate with compendium data
        if (savedCrossSceneState.AvailableLegacies.Count > 0)
            CrossSceneState.SetAvailableLegacies(savedCrossSceneState.AvailableLegacies);

        //this is currently unnecessary: we don't go back to the game over screen. but it's very likely we might want to track / restore this information.
        if (savedCrossSceneState.CurrentEnding != null)
            CrossSceneState.SetCurrentEnding(savedCrossSceneState.CurrentEnding);

        if (savedCrossSceneState.DefunctCharacter != null)
            CrossSceneState.SetDefunctCharacter(savedCrossSceneState.DefunctCharacter);

        LoadScene(SceneNumber.NewGameScene);
    }

    void LoadScene(int sceneNr) {
        canTakeInput = false;
        sceneToLoad = sceneNr;
        FadeOut();
        Invoke("LoadSceneDelayed", fadeDuration);
    }

    void LoadSceneDelayed() {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void TryPurgeSave() {
        if (!canTakeInput)
            return;

        ShowOverlay(purgeConfirm);
    }

    public void PurgeSave() {
        if (!canTakeInput)
            return;

        saveGameManager.DeleteCurrentSave();
        currentOverlay = null; // to ensure we can re-open another overlay afterwards
        FadeOut();
        Invoke("UpdateAndShowMenu", fadeDuration);
    }

    public void ShowCredits() {
        if (!canTakeInput)
            return;

        ShowOverlay(credits);
    }

    public void ShowVersionHints() {
        if (!canTakeInput)
            return;

        ShowOverlay(versionHints);
        versionAnim.Stop(); // Ensure that the anim is no longer playing
    }

    public void CloseCurrentOverlay() {
        if (!canTakeInput)
            return;

        HideCurrentOverlay();
    }

    public void Exit() {
        if (!canTakeInput)
            return;

        Application.Quit();
    }

    #endregion

}