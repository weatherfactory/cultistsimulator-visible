using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI ButtonText;

    private const string DEFAULT_MESSAGE = "RESTART";
    private bool clickedOnce = true;

    public void Awake()
    {
        ButtonText.text = DEFAULT_MESSAGE;
    }

    public bool AttemptRestart()
    {
        if (clickedOnce)
            return true;
        
        ButtonText.text = "DEFINITELY RESTART?";
        clickedOnce = true;
        StartCoroutine(BrieflyDisable());
        return false;
    }

    public void ResetState()
    {
        ButtonText.text = DEFAULT_MESSAGE;
        clickedOnce = false;

    }

    public IEnumerator BrieflyDisable()
    {
        gameObject.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(2);
        gameObject.GetComponent<Button>().interactable = true;
    }
}
