using UnityEngine;
using System.Collections;
using TMPro;

public class PauseButton : MonoBehaviour {

[SerializeField] TextMeshProUGUI ButtonText;

    public void SetPausedState(bool isPaused)
    {
        if (isPaused)
            ButtonText.text = "UNPAUSE";
        else
            ButtonText.text = "PAUSE";
    }
}
