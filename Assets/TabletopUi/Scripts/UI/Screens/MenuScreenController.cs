// based on LoadingScreenManager
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
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine.SceneManagement;

public class MenuScreenController : MonoBehaviour {

	bool waitForInput = true;

	[Header("Buttons")]
	public BeginGameButton beginGameButton;
	public Button showOptions;
	public EventSystem eventSystem;

	[Header("Loading Visuals")]
	public LoadingIcon loadingIcon;
	public Image fadeOverlay;

	[Header("Timing Settings")]
	public float waitOnLoadEnd = 0.25f;
	public float fadeDuration = 0.25f;

	[Header("Loading Settings")]
	public LoadSceneMode loadSceneMode = LoadSceneMode.Single;
	public ThreadPriority loadThreadPriority;

	[Header("Other")]
	// If loading additive, link to the cameras audio listener, to avoid multiple active audio listeners
	public AudioListener audioListener;

	AsyncOperation operation;
	Scene currentScene;

	public static int sceneToLoad;
	// IMPORTANT! This is the build index of your loading scene. You need to change this to match your actual scene index
	static int loadingSceneIndex = 1;

	public static void LoadScene(int levelNum) {				
		Application.backgroundLoadingPriority = ThreadPriority.High;
		sceneToLoad = levelNum;
		SceneManager.LoadScene(loadingSceneIndex);
	}

	void Start() {
	    var registry = new Registry();
	    var compendium = new Compendium();
	    registry.Register<ICompendium>(compendium);
	    var contentImporter = new ContentImporter();
	    contentImporter.PopulateCompendium(compendium);


        var saveGameManager = new GameSaveManager(new GameDataImporter(compendium), new GameDataExporter());

        if (!saveGameManager.DoesGameSaveExist())
        { 
            beginGameButton.Text = "BEGIN";
	        sceneToLoad = SceneNumber.GameScene;
        }
        else
        { 
	        beginGameButton.Text = "CONTINUE";
            if (saveGameManager.IsSavedGameActive())
                //back into the game!
                sceneToLoad = SceneNumber.GameScene;
            else
            {
                //we left the game from the game over or legacy screen: go back to choose a legacy.
                var savedCrossSceneState = saveGameManager.RetrieveSavedCrossSceneState();

                //this is essential. If we earlier left the game in the legacy screen, we're about to go back there, 
                //and we need to retrieve the legacy ids and populate with compendium data
                if (savedCrossSceneState.AvailableLegacies.Count > 0)
                    CrossSceneState.SetAvailableLegacies(savedCrossSceneState.AvailableLegacies);
            //this is currently unnecessary: we don't go back to the game over screen. but it's very likely we might want to track / restore this information.
                if (savedCrossSceneState.CurrentEnding != null)
                    CrossSceneState.SetCurrentEnding(savedCrossSceneState.CurrentEnding);

                sceneToLoad = SceneNumber.NewGameScene;

            }
        }
        if (sceneToLoad < 0)
			return;

		fadeOverlay.gameObject.SetActive(true); // Making sure it's on so that we can crossfade Alpha
		currentScene = SceneManager.GetActiveScene();
		StartCoroutine(LoadAsync(sceneToLoad));

	}

	private IEnumerator LoadAsync(int levelNum) {
		ShowLoadingVisuals();

		yield return null; 

		FadeIn();
		StartOperation(levelNum);

		// operation does not auto-activate scene, so it's stuck at 0.9
		while (DoneLoading() == false) 
			yield return null;

		if (loadSceneMode == LoadSceneMode.Additive)
			audioListener.enabled = false;

		ShowCompletionVisuals();

		yield return new WaitForSeconds(waitOnLoadEnd);

		while (waitForInput)
			yield return null;

		FadeOut();

		yield return new WaitForSeconds(fadeDuration);

		if (loadSceneMode == LoadSceneMode.Additive)
			SceneManager.UnloadSceneAsync(currentScene.name);
		else
			operation.allowSceneActivation = true;
	}

	private void StartOperation(int levelNum) {
		Application.backgroundLoadingPriority = loadThreadPriority;
		operation = SceneManager.LoadSceneAsync(levelNum, loadSceneMode);

		if (loadSceneMode == LoadSceneMode.Single)
			operation.allowSceneActivation = false;
	}

	private bool DoneLoading() {
		return (loadSceneMode == LoadSceneMode.Additive && operation.isDone) || (loadSceneMode == LoadSceneMode.Single && operation.progress >= 0.9f); 
	}

	void FadeIn() {
		fadeOverlay.CrossFadeAlpha(0, fadeDuration, true);
	}

	void FadeOut() {
		fadeOverlay.CrossFadeAlpha(1, fadeDuration, true);
	}

	void ShowLoadingVisuals() {
		if (loadingIcon)
			loadingIcon.doRotation = true;
	}

	void ShowCompletionVisuals() {
		if (loadingIcon)
			loadingIcon.gameObject.SetActive(false);
	}

	public void StartGame() {
		if (!waitForInput)
			return;
		
		if (DoneLoading() == false) 
			eventSystem.SetSelectedGameObject(beginGameButton.gameObject);

		eventSystem.enabled = false;
		waitForInput = false;		
	}
}