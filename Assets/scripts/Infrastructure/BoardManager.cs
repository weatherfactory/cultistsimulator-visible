using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
#pragma warning disable 649

public class BoardManager : MonoBehaviour {
    [SerializeField]private InputField inputAdjustElementNamed;
    [SerializeField]private Text txtStatus;
    [SerializeField]private GameObject pnlResources;
    [SerializeField] GameObject pnlElementSlot;
    [SerializeField] GameObject pnlCurrentAspects;
    [SerializeField]GameObject imgAspectDisplay;
    public GameObject itemBeingDragged;

    private void AddElementToBoard(string elementId, int quantity)
    {
        GameObject newElementSlot = Instantiate(pnlElementSlot, pnlResources.transform) as GameObject;
        if (newElementSlot != null)
        {
            ElementSlot slotScript = newElementSlot.GetComponent<ElementSlot>();
            slotScript.PopulateSlot(elementId, quantity, ContentManager.Instance);
        }
        else
            throw new ApplicationException("couldn't create a new Element slot from prefab");
    }
    private AspectDisplay GetAspectDisplayForId(string aspectId)
    {
        return
            pnlCurrentAspects.GetComponentsInChildren<AspectDisplay>().SingleOrDefault(a => a.AspectId == aspectId);
    }

    private ElementSlot GetElementSlotForId(string elementId)
    {
        return
            pnlResources.GetComponentsInChildren<ElementSlot>().SingleOrDefault(e => e.ElementId == elementId);
    }

    private void AddAspectToDisplay(string aspectId, int quantity)
    {
        GameObject newAspectDisplay = Instantiate(imgAspectDisplay, pnlCurrentAspects.transform) as GameObject;
        if (newAspectDisplay != null)
        {
            AspectDisplay adScript = newAspectDisplay.GetComponent<AspectDisplay>();
            adScript.PopulateDisplay(aspectId, quantity, ContentManager.Instance);
        }
    }

    public void Start()
    {
        ContentManager.Instance.ImportElements();
        ChangeElementQuantityOnBoard("ordinarylife",1);
        ChangeElementQuantityOnBoard("health", 10);
        ChangeElementQuantityOnBoard("occultscrap", 1);

    }

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

    public void ChangeElementQuantityOnBoard(string elementId,int quantity)
    {
        ElementSlot existingElement = GetElementSlotForId(elementId);
        if(existingElement)
            existingElement.ModifyQuantity(quantity);
        else
        {
            AddElementToBoard(elementId,quantity);
        }
    }

    public void ChangeAspectQuantityInDisplay(string aspectId, int quantity)
    {
        AspectDisplay existingAspect = GetAspectDisplayForId(aspectId);
        if (existingAspect)
            existingAspect.ModifyQuantity(quantity);
        else 
            AddAspectToDisplay(aspectId,quantity);
    }


  
}
