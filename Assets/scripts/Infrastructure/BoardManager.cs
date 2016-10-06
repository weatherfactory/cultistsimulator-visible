using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
#pragma warning disable 649

public class BoardManager : MonoBehaviour {
    [SerializeField]private InputField inputAdjustElementNamed;
    [SerializeField]private Text txtStatus;
    [SerializeField]private GameObject pnlResources;
    [SerializeField]GameObject pnlWorkspace;
    [SerializeField] GameObject prefabElementSlot;
    [SerializeField]GameObject prefabEmptyElementSlot;
    [SerializeField] GameObject pnlCurrentAspects;
    [SerializeField]GameObject imgAspectDisplay;
    public GameObject itemBeingDragged;

    private void addElementToBoard(string elementId, int quantity)
    {
        GameObject newElementSlot = Instantiate(prefabElementSlot, pnlResources.transform) as GameObject;

            ElementSlot slotScript = newElementSlot.GetComponent<ElementSlot>();
            slotScript.PopulateSlot(elementId, quantity, ContentManager.Instance);

    }

    private void addSlotToWorkspace()
    {
        GameObject newElementSlot = Instantiate(prefabEmptyElementSlot, pnlWorkspace.transform) as GameObject;
       newElementSlot.transform.localPosition=new Vector3(75,-30);

    }
    private AspectDisplay GetAspectDisplayForId(string aspectId)
    {
        return
            pnlCurrentAspects.GetComponentsInChildren<AspectDisplay>().SingleOrDefault(a => a.AspectId == aspectId);
    }

    private ElementSlot GetElementSlotForId(string elementId)
    {
        return
            pnlResources.GetComponentsInChildren<ElementSlot>().SingleOrDefault(e => e.Element.Id == elementId);
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

    private void ChangeAspectQuantityInDisplay(string aspectId, int quantity)
    {
        AspectDisplay existingAspect = GetAspectDisplayForId(aspectId);
        if (existingAspect)
            existingAspect.ModifyQuantity(quantity);
        else
            AddAspectToDisplay(aspectId, quantity);
    }
    private void ResetAspectDisplay()
    {
        foreach(Transform child in pnlCurrentAspects.transform)
    GameObject.Destroy(child.gameObject);
    }

    public void Start()
    {
        ContentManager.Instance.ImportElements();
        ChangeElementQuantityOnBoard("ordinarylife",1);
        ChangeElementQuantityOnBoard("health", 10);
        ChangeElementQuantityOnBoard("occultscrap", 1);

    }

    public void MakeFirstSlotAvailable(bool visibility)
    {
        addSlotToWorkspace();
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
            addElementToBoard(elementId,quantity);
        }
    }



    public void UpdateAspectDisplay()
    {
        ResetAspectDisplay();

        DraggableElementDisplay[] elements = pnlWorkspace.GetComponentsInChildren<DraggableElementDisplay>();

        foreach (DraggableElementDisplay element in elements)
        {

            foreach (KeyValuePair<string, int> kvp in element.Element.Aspects)
            {
                ChangeAspectQuantityInDisplay(kvp.Key, kvp.Value);
            }

        }

        
    }


    public void ClearWorkspaceElements()
    {
        DraggableElementDisplay[] elements = pnlWorkspace.GetComponentsInChildren<DraggableElementDisplay>();

        foreach (DraggableElementDisplay element in elements)

  element.ReturnToOrigin();


        ResetAspectDisplay();
    }
}
