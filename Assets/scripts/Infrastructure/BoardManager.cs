﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

#pragma warning disable 649

public class BoardManager : MonoBehaviour
{
    [SerializeField] private InputField inputAdjustElementNamed;
    [SerializeField] private Text txtStatus;
    [SerializeField] private GameObject pnlResources;
    [SerializeField] GameObject pnlWorkspace;
    [SerializeField] GameObject pnlCurrentAspects;
    [SerializeField] GameObject imgAspectDisplay;
    [SerializeField] private GameObject objLimbo;
    [SerializeField]GameObject prefabElementSlot;
    [SerializeField]GameObject prefabEmptyElementSlot;
    [SerializeField]GameObject prefabChildSlotsOrganiser;

    public GameObject itemBeingDragged;

    private void addElementToBoard(string elementId, int quantity)
    {
        GameObject newElementSlot = Instantiate(prefabElementSlot,pnlResources.transform) as GameObject;
        try
        {
            ElementSlot slotScript = newElementSlot.GetComponent<ElementSlot>();
            slotScript.PopulateSlot(elementId, quantity, ContentManager.Instance);
            
        }
        catch (NullReferenceException)
        {
            
            BoardLog("Couldn't create element with id " + elementId);
            GameObject.Destroy(newElementSlot);
        }
            

    }




    private ElementSlot GetElementSlotForId(string elementId)
    {
        return
            pnlResources.GetComponentsInChildren<ElementSlot>().SingleOrDefault(e => e.Element.Id == elementId);
    }



    /// <summary>
    /// This sets our starting elements
    /// </summary>
    public void Start()
    {
        ContentManager.Instance.ImportElements();
        ChangeElementQuantityOnBoard("clique", 1);
        ChangeElementQuantityOnBoard("ordinarylife",1);
        ChangeElementQuantityOnBoard("health", 10);
        ChangeElementQuantityOnBoard("occultscrap", 1);
        

    }

    public void BoardLog(string message)
    {
        Debug.Log(message);
    }

    public void MakeFirstSlotAvailable(Vector3 governorPosition)
    {
        int governedStepRight = 50;
        int nudgeDown = -10;
        GameObject newElementSlot = Instantiate(prefabEmptyElementSlot, pnlWorkspace.transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governorPosition.x + governedStepRight, governorPosition.y + nudgeDown);
        newElementSlot.transform.localPosition = newSlotPosition;
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

    

    public void ClearWorkspaceElements()
    {
        DraggableElementToken[] elements = pnlWorkspace.GetComponentsInChildren<DraggableElementToken>();

        foreach (DraggableElementToken element in elements)

        element.ReturnToOrigin();
     pnlCurrentAspects.GetComponent<CurrentAspects>().ResetAspects();
    }


    public void CreateChildSlotsOrganiser(SlotReceiveElement governingSlot, DraggableElementToken draggedElement)
    {
        
        float governingSlotHeight = governingSlot.GetComponent<RectTransform>().rect.height;
        Transform governingSlotTransform = governingSlot.transform;
        governingSlot.ChildSlotOrganiser = Instantiate(prefabChildSlotsOrganiser, pnlWorkspace.transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governingSlotTransform.localPosition.x, governingSlotTransform.localPosition.y - governingSlotHeight);
        governingSlot.ChildSlotOrganiser.transform.localPosition = newSlotPosition;

        governingSlot.ChildSlotOrganiser.GetComponent<ChildSlotOrganiser>().Populate(draggedElement);


    }

    public void UpdateAspectDisplay()
    {
        pnlCurrentAspects.GetComponent<CurrentAspects>().UpdateAspects(pnlWorkspace);
    }
}
