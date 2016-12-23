using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Assets.CS.TabletopUI;

public class LoadingInput : MonoBehaviour, IPointerClickHandler {

	[SerializeField] LoadingScreenManager manager;

	public void OnPointerClick (PointerEventData eventData) {
		SetInput();
	}

	void Update() {
		if (Input.anyKeyDown)
			SetInput();
	}

	void SetInput() {
		manager.waitForInput = false;
	}

}
