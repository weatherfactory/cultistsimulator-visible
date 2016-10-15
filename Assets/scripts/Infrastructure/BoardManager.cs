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
    [SerializeField] CurrentAspectsDisplay pnlCurrentAspects;
    [SerializeField]RecipeDisplay pnlRecipeDisplay;
    [SerializeField] private GameObject objLimbo;
    [SerializeField]GameObject prefabElementToken;
    [SerializeField]GameObject prefabEmptyElementSlot;
    [SerializeField]GameObject prefabChildSlotsOrganiser;

    public DraggableToken CurrentDragItem;

    private void addElementToBoard(string elementId, int quantity,int? sibingIndex)
    {
        GameObject elementTokenGameObject = Instantiate(prefabElementToken,pnlResources.transform) as GameObject;

        //in case we're replacing a token already on the board
        if (sibingIndex!=null)
         elementTokenGameObject.transform.SetSiblingIndex(sibingIndex.Value);

        elementTokenGameObject.name = "Element token for " + elementId;
        try
        {
            DraggableElementToken elementTokenScript = elementTokenGameObject.GetComponent<DraggableElementToken>();
            elementTokenScript.PopulateForElementId(elementId, quantity, ContentRepository.Instance);
            
        }
        catch (NullReferenceException)
        {
            
            Log("Couldn't create element with id " + elementId);
            GameObject.Destroy(elementTokenGameObject);
        }
            

    }


    private DraggableElementToken GetExistingElementForId(string elementId)
    {
        DraggableElementToken[] existingElementTokens = pnlResources.GetComponentsInChildren<DraggableElementToken>();
        return
            existingElementTokens.SingleOrDefault(e => e.Element.Id == elementId);
    }




    /// <summary>
    /// This sets our starting elements
    /// </summary>
    public void Start()
    {
        ContentRepository.Instance.ImportElements();
        ContentRepository.Instance.ImportRecipes();
        ModifyElementQuantityOnBoard("clique", 1);
        ModifyElementQuantityOnBoard("ordinarylife",1);
        ModifyElementQuantityOnBoard("health", 10);
        ModifyElementQuantityOnBoard("occultscrap", 1);
    }


    public void SendToLimbo(GameObject target)
    {
        target.transform.SetParent(objLimbo.transform);
    }

    public string GetDebugElementName()
    {
        return  inputAdjustElementNamed.textComponent.text;
    }

    public void ModifyElementQuantityOnBoard(string elementId, int quantity)
    {
        ModifyElementQuantityOnBoard(elementId,quantity,null);
    }

    public void ModifyElementQuantityOnBoard(string elementId,int quantity,int? siblingIndex)
    {
        DraggableElementToken existingElement = GetExistingElementForId(elementId);
        if(existingElement)
            existingElement.ModifyQuantity(quantity);
        else
        {
            addElementToBoard(elementId,quantity,siblingIndex);
        }
    }

    

    public void ClearWorkspaceElements()
    {
        DraggableElementToken[] elements = pnlWorkspace.GetComponentsInChildren<DraggableElementToken>();

        foreach (DraggableElementToken element in elements)

        element.ReturnToOrigin();
     pnlCurrentAspects.GetComponent<CurrentAspectsDisplay>().ResetAspects();
    }

    public void ReturnElementTokenToStorage(DraggableElementToken tokenToReturn)
    {

        SendToLimbo(tokenToReturn.gameObject); //to prevent possible double-counting
        ModifyElementQuantityOnBoard(tokenToReturn.Element.Id, tokenToReturn.Quantity);
        GameObject.Destroy(tokenToReturn.gameObject);

        UpdateAspectDisplay();
    }


    public void AddChildSlots(SlotReceiveElement governingSlot, DraggableElementToken draggedElement)
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
            pnlCurrentAspects.UpdateAspects(pnlWorkspace);
    }

    public void DisplayCurrentRecipe(Recipe recipe)
    {
        pnlRecipeDisplay.GetComponent<RecipeDisplay>().DisplayRecipe(recipe);
    }

    public string GetCurrentVerbId()
    {
        return pnlWorkspace.GetCurrentVerbId();
    }

    public void QueueCurrentRecipe()
    {
        Recipe currentRecipe= pnlRecipeDisplay.CurrentRecipe;
        Log(pnlRecipeDisplay.CurrentRecipe.StartDescription);
        pnlWorld.AddTimer(currentRecipe);
    }

    public void Log(string message)
    {
       pnlLog.Write(message);
    }

    public void DoHeartbeat()
    {
    pnlWorld.DoHeartbeat();
    }

    public void ExecuteRecipe(Recipe recipe)
    {
        Log(recipe.Description);
        foreach (var e in recipe.Effects)
            ModifyElementQuantityOnBoard(e.Key, e.Value);


    }

    public void VerbAddedToSlot(Transform verbSlotTransform)
    {
        CurrentDragItem.transform.SetParent(verbSlotTransform);
        pnlWorkspace.MakeFirstSlotAvailable(verbSlotTransform.localPosition,prefabEmptyElementSlot);
        UpdateAspectDisplay();
    }
}
