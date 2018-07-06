#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using OrbCreationExtensions;
using TMPro;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{

    [SerializeField] Button ThisButton;
    [SerializeField] TextMeshProUGUI ButtonText;



    public void SetPausedText(bool isPaused)
    {
        if (isPaused)
        { 
			//ButtonText.text = "Unpause <size=60%><alpha=#99>[SPACE]";
			ButtonText.text = LanguageTable.Get("UI_UNPAUSE");
        }
        else
        { 
			//ButtonText.text = "Pause <size=60%><alpha=#99>[SPACE]";
			ButtonText.text = LanguageTable.Get("UI_PAUSE");
        }
    }

 
}
