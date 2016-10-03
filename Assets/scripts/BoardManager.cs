using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#pragma warning disable 649

public class BoardManager : MonoBehaviour {
    [SerializeField]private InputField inputAdjustElementNamed;
    [SerializeField]private Text txtStatus;
    [SerializeField]private GameObject pnlResources;
    [SerializeField] GameObject pnlElementSlot;


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
    }

    public void AddElementToBoard(string elementId)
    {
        GameObject newElementSlot= Instantiate(pnlElementSlot, pnlResources.transform) as GameObject;
        if (newElementSlot != null)
        { 
            newElementSlot.GetComponent<ElementSlot>().SetElementValues(elementId, ContentManager.Instance);
        }
        else
        throw new ApplicationException("couldn't create a new element slot from prefab");
    }
}
