using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Assets.CS.TabletopUI;

public class SplashScreen : MonoBehaviour, IPointerClickHandler {

	bool isLoading = false;
	public string gameSceneName = "TabletopPrototype";
	[SerializeField] CanvasGroupFader fader;

	void Start() {
		fader.Hide();
	}

	public void OnPointerClick (PointerEventData eventData) {
		LoadGameScene();
	}

	void Update() {
		if (Input.anyKeyDown)
			LoadGameScene();
	}

	void LoadGameScene() {
		if (isLoading)
			return;

		fader.Show();
		isLoading = true;
		Invoke("DoLoad", fader.durationTurnOn);
	}

	void DoLoad() {
		SceneManager.LoadScene(gameSceneName);
	}

}
