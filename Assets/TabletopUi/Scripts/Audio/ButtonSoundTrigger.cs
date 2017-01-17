using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSoundTrigger : MonoBehaviour {

    [SerializeField]
    string soundFXName = "UIButtonClick";

	void Start () {
        var button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener( DoClickSound );
	}
	
	void DoClickSound() {
        SoundManager.PlaySfx(soundFXName);
	}
}
