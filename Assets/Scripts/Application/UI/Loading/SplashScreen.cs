#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SecretHistories.UI;
using SecretHistories.Services;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class SplashScreen : MonoBehaviour, IPointerClickHandler {

	bool isLoading = false;
	public float waitUntilAutoComplete = 5f;
	[SerializeField] CanvasGroupFader fader;

	void Start() {
		fader.Hide();
		Invoke("LoadGameScene", waitUntilAutoComplete);
	}

	public void OnPointerClick (PointerEventData eventData) {
		LoadGameScene();
	}

	void Update() {
		if (Keyboard.current.anyKey.wasPressedThisFrame)
			LoadGameScene();
	}

	void LoadGameScene() {
		if (fader.gameObject.activeInHierarchy || isLoading)
			return;

		fader.Show();
		isLoading = true;
		Invoke("DoLoad", fader.durationTurnOn);
	}

	void DoLoad() {
        Registry.Get<StageHand>().MenuScreen();
	}

}
