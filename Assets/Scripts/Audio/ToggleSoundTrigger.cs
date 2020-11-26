using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleSoundTrigger : MonoBehaviour, IPointerEnterHandler {

    [SerializeField]
	string soundFXNameOn = "UIButtonClick";
	[SerializeField]
	string soundFXNameOff = "";
    [SerializeField]
    string hoverFXName = "TokenHover";

	Toggle button;

    void Start () {
		button = GetComponent<Toggle>();

        if (button != null)
			button.onValueChanged.AddListener( DoClickSound );
	}
	
	void DoClickSound(bool isOn) {
		if (isOn)
			SoundManager.PlaySfx(soundFXNameOn);
		else 
			SoundManager.PlaySfx(soundFXNameOff);
	}

    public void OnPointerEnter(PointerEventData eventData) {
        if (!button.interactable)
            return;

        SoundManager.PlaySfx(hoverFXName);
    }
}
