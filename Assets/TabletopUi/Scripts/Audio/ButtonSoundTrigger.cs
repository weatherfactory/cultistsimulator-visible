using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSoundTrigger : MonoBehaviour, IPointerEnterHandler {

    [SerializeField]
    string soundFXName = "UIButtonClick";
    [SerializeField]
    string hoverFXName = "TokenHover";

    Button button;

    void Start () {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener( DoClickSound );
	}
	
	void DoClickSound() {
        SoundManager.PlaySfx(soundFXName);
	}

    public void OnPointerEnter(PointerEventData eventData) {
        if (!button.interactable)
            return;

        SoundManager.PlaySfx(hoverFXName);
    }
}
