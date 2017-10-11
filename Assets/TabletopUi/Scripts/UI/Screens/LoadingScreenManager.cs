// LoadingScreenManager
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
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour {

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

	public static int sceneToLoad = 2;
	// IMPORTANT! This is the build index of your loading scene. You need to change this to match your actual scene index
	static int loadingSceneIndex = 1;

	public static void LoadScene(int levelNum) {				
		Application.backgroundLoadingPriority = ThreadPriority.High;
		sceneToLoad = levelNum;
		SceneManager.LoadScene(loadingSceneIndex);
	}

	void Start() {
		if (sceneToLoad < 0)
			return;

		fadeOverlay.gameObject.SetActive(true); // Making sure it's on so that we can crossfade Alpha
		currentScene = SceneManager.GetActiveScene();
		StartCoroutine(LoadAsync(sceneToLoad));

	    if (GameSaveManager.DoesGameSaveExist())
	        beginGameButton.Text = "CONTINUE";
	    else
	        beginGameButton.Text = "BEGIN";
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