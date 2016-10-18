using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrbCreationExtensions;
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

    public CurrentAspectsDisplay PnlCurrentAspects
    {
        set { pnlCurrentAspects = value; }
        get { return pnlCurrentAspects; }
    }

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
            ExileToLimboThenDestroy(elementTokenGameObject);
        }
            

    }

    /// <summary>
    /// this is the element token in the relevant storage area, so it includes quantity. It doesn't address the workspace and it assumes only one panel holds the tokens.
    /// </summary>
    private DraggableElementToken GetStoredElementTokenForId(string elementId)
    {
        DraggableElementToken[] existingElementTokens = GetAllStoredElementTokens();
        return
            existingElementTokens.SingleOrDefault(e => e.Element.Id == elementId);
    }

    /// <summary>
    /// an array of all element tokens currently in the player's stockpile
    /// </summary>
    /// <returns></returns>
    public DraggableElementToken[] GetAllStoredElementTokens()
    {
        return pnlResources.GetComponentsInChildren<DraggableElementToken>();
    }

    private IEnumerable<RecipeTimer> GetAllCurrentRecipeTimers()
    {
        return pnlWorld.GetCurrentRecipeTimers();
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

    /// <summary>
    /// takes a gameobject out of the hierarchy before destroying it, in case we might otherwise interact with it in the current frame
    /// </summary>
    /// <param name="target"></param>
    public void ExileToLimboThenDestroy(GameObject target)
    {
        target.transform.SetParent(objLimbo.transform);
        GameObject.Destroy(target);
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
        DraggableElementToken existingElement = GetStoredElementTokenForId(elementId);
        if(existingElement)
            existingElement.ModifyQuantity(quantity);
        else
        {
            addElementToBoard(elementId,quantity,siblingIndex);
        }
    }

    
    public void ClearWorkspaceElements()
    {
        pnlWorkspace.ClearAllWorkspaceElements(this);
    }
    private void ClearRecipeTimers()
    {
        pnlWorld.ClearRecipeTimers();
    }

    public void ReturnElementTokenToStorage(DraggableElementToken tokenToReturn)
    {
        //removes the element token from whereever it is currently and adds it to any existing stacks in its home panel
        string elementId = tokenToReturn.Element.Id;
        int elementQuantity = tokenToReturn.Quantity;
        ExileToLimboThenDestroy(tokenToReturn.gameObject); //to prevent possible double-counting
        ModifyElementQuantityOnBoard(elementId,elementQuantity);
        

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
        pnlWorld.AddTimer(currentRecipe,null);
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
    /// <summary>
    /// DANGEROUS removes all elements from storage and workspace
    /// </summary>
    public void ClearBoard()
    {
        ClearWorkspaceElements();
        DraggableElementToken[]  allElementTokens=GetAllStoredElementTokens();
        foreach (DraggableElementToken e in allElementTokens)
        {
            e.SetQuantity(0);
        }
        ClearRecipeTimers();
    }

    

    public void SaveCurrentBoard()
    {
        try
        {
            string exportJson;

            Hashtable htElementsPossessed = new Hashtable();
            foreach (DraggableElementToken e in GetAllStoredElementTokens())
            {
                htElementsPossessed.Add(e.Element.Id, e.Quantity);
            }


            Hashtable htRecipeTimers = new Hashtable();
            foreach (RecipeTimer rt in GetAllCurrentRecipeTimers())
            {
                htRecipeTimers.Add(rt.Recipe.Id,rt.TimeRemaining);
            }

            Hashtable htSave=new Hashtable();

            htSave.Add(Noon.NoonUtilityConstants.CONST_SAVE_ELEMENTSPOSSESSED,htElementsPossessed);
            htSave.Add(Noon.NoonUtilityConstants.CONST_SAVE_RECIPETIMERS,htRecipeTimers);

            exportJson = htSave.JsonString();
            
            File.WriteAllText(Noon.NoonUtility.GetGameSavePath(), exportJson);
            Log("Saved the game; but not the world.");
        }
        catch (Exception)
        {
            Log("Something horrible has happened. We couldn't save the game.");
        }
    }

    

    public void LoadCurrentBoard()
    {
        try
        {
            string importJson = File.ReadAllText(Noon.NoonUtility.GetGameSavePath());
            Hashtable htSave = SimpleJsonImporter.Import(importJson);

            Hashtable htElementsPossessed = htSave.GetHashtable(Noon.NoonUtilityConstants.CONST_SAVE_ELEMENTSPOSSESSED);
            Hashtable htRecipeTimers = htSave.GetHashtable(Noon.NoonUtilityConstants.CONST_SAVE_RECIPETIMERS);
            //check if it's all valid first
            foreach (string k in htElementsPossessed.Keys)
            {
                if (!ContentRepository.Instance.IsKnownElement(k))
                {
                    Log("Unknown element id: " + k);
                    throw new ApplicationException("Unknown element id " + k + " in save file");
                }
            }

            ClearBoard();

            foreach (string k in htElementsPossessed.Keys)
            {
                ModifyElementQuantityOnBoard(k, Convert.ToInt32(htElementsPossessed[k]));
            }

            foreach (string k in htRecipeTimers.Keys)
            {
                Recipe r = ContentRepository.Instance.RecipeCompendium.GetRecipeById(k);
                pnlWorld.AddTimer(r, float.Parse(htRecipeTimers[k].ToString()));
            }

            Log("Once more, for you, we have loaded the game.");
        }
        catch (Exception exception)
        {
            Log("Missing or invalid save file. Sorry! (" + exception.Message + ")");
        }
    }
}
