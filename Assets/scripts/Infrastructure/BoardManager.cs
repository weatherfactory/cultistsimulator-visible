using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

#pragma warning disable 649

public class BoardManager : MonoBehaviour
{
    [SerializeField] private InputField inputAdjustElementNamed;
    [SerializeField] private LogPanel pnlLog;
    [SerializeField] private GameObject pnlResources;
    [SerializeField] Workspace pnlWorkspace;
    [SerializeField] private WorldPanel pnlWorld;
    [SerializeField] GameObject pnlCurrentAspects;
    [SerializeField]RecipeDisplay pnlRecipeDisplay;
    [SerializeField] private GameObject objLimbo;
    [SerializeField]GameObject prefabElementSlot;
    [SerializeField]GameObject prefabEmptyElementSlot;
    [SerializeField]GameObject prefabChildSlotsOrganiser;

    public DraggableToken itemBeingDragged;

    private void addElementToBoard(string elementId, int quantity)
    {
        GameObject newElementSlot = Instantiate(prefabElementSlot,pnlResources.transform) as GameObject;
        try
        {
            StorageSlot slotScript = newElementSlot.GetComponent<StorageSlot>();
            slotScript.PopulateSlot(elementId, quantity, ContentRepository.Instance);
            
        }
        catch (NullReferenceException)
        {
            
            BoardLog("Couldn't create element with id " + elementId);
            GameObject.Destroy(newElementSlot);
        }
            

    }


    private StorageSlot GetElementSlotForId(string elementId)
    {
        return
            pnlResources.GetComponentsInChildren<StorageSlot>().SingleOrDefault(e => e.Element.Id == elementId);
    }



    /// <summary>
    /// This sets our starting elements
    /// </summary>
    public void Start()
    {
        ContentRepository.Instance.ImportElements();
        ContentRepository.Instance.ImportRecipes();
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

    public string GetDebugElementName()
    {
        return  inputAdjustElementNamed.textComponent.text;
    }

    public void ChangeElementQuantityOnBoard(string elementId,int quantity)
    {
        StorageSlot existingElement = GetElementSlotForId(elementId);
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
     pnlCurrentAspects.GetComponent<CurrentAspectsDisplay>().ResetAspects();
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
            pnlCurrentAspects.GetComponent<CurrentAspectsDisplay>().UpdateAspects(pnlWorkspace.GetComponentsInChildren<DraggableElementToken>());
    }

    public void DisplayCurrentRecipe(Recipe recipe)
    {
        pnlRecipeDisplay.GetComponent<RecipeDisplay>().DisplayRecipe(recipe);
    }

    public string GetCurrentVerbId()
    {
        return pnlWorkspace.GetCurrentVerbId();
    }

    public void ExecuteCurrentRecipe()
    {
        Recipe currentRecipe= pnlRecipeDisplay.CurrentRecipe;
        Log(pnlRecipeDisplay.CurrentRecipe.StartDescription);
        pnlWorld.AddTimer(currentRecipe);
    }

    public void Log(string message)
    {
       pnlLog.Write(message);
    }
}
