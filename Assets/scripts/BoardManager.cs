using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {
    [SerializeField]private InputField inputAdjustElementNamed;
    [SerializeField]private Text txtStatus;
    

    public void SetFirstElementVisibility(bool visibility)
    {
        GameObject.Find("pnlFirstElementSlot").GetComponent<CanvasGroup>().alpha = Convert.ToInt32(visibility);
    }
    public void SetStatusText(string statusText)
    {

        txtStatus.GetComponent<Text>().text = statusText;
    }

    public string GetDebugElementName()
    {
        return  inputAdjustElementNamed.textComponent.text;
;

    }
}
