using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrbCreationExtensions;
using UnityEngine.UI;

#pragma warning disable 649

public class BoardManager : MonoBehaviour,IElementsContainer,INotifier
{
    [SerializeField] private InputField inputAdjustElementNamed;
    [SerializeField] private LogPanel pnlLog;
    [SerializeField] private GameObject pnlVerbs;
    [SerializeField] private GameObject pnlResources;
    [SerializeField] Workspace pnlWorkspace;
    [SerializeField] private WorldPanel pnlWorld;
    [SerializeField] AspectsDisplay pnlCurrentAspects;
    [SerializeField]RecipeDisplay pnlRecipeDisplay;
    [SerializeField] private KnownRecipeDisplay pnlKnownRecipeDisplay;
    [SerializeField] private GameObject objLimbo;
    [SerializeField]GameObject prefabElementToken;
    [SerializeField]GameObject prefabVerbFrame;
    [SerializeField]GameObject prefabVerbToken;
    [SerializeField]GameObject prefabEmptyElementSlot;
    [SerializeField]GameObject prefabChildSlotsOrganiser;
    [SerializeField] private GameObject prefabNotificationPanel;

    public DraggableToken CurrentDragItem;
    private Dictionary<string,Recipe> knownRecipes= new Dictionary<string, Recipe>();

    public AspectsDisplay PnlAspects
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
            
            Log("Couldn't create element with id " + elementId, Style.Subtle);
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

    private IEnumerable<RecipeSituation> GetAllCurrentRecipeTimers()
    {
        return pnlWorld.GetCurrentRecipeTimers();
    }


    /// <summary>
    /// This sets our starting elements
    /// </summary>
    public void Start()
    {
        ContentRepository.Instance.ImportVerbs();
        ContentRepository.Instance.ImportElements();
        ContentRepository.Instance.ImportRecipes();
        ModifyElementQuantity("clique", 1);
        ModifyElementQuantity("ordinarylife", 1);
        ModifyElementQuantity("health", 3);
        ModifyElementQuantity("reason", 3);
        ModifyElementQuantity("occultscrap", 1);
        foreach(Verb v in ContentRepository.Instance.GetAllVerbs())
        {
            AddVerbToBoard(v);
        }
        //ModifyElementQuantityOnBoard("alockedmind", 1);
        //ModifyElementQuantityOnBoard("aninspectorcalls", 1);
        //ModifyElementQuantityOnBoard("order", 1);
        //ModifyElementQuantityOnBoard("suitablepremises", 1);
        //ModifyElementQuantityOnBoard("crypt", 1);
        //ModifyElementQuantityOnBoard("starshatteredfane", 1);
        //ModifyElementQuantityOnBoard("minorremaking", 1);
        //ModifyElementQuantityOnBoard("cryptexpedition", 1);
        //ModifyElementQuantityOnBoard("riteofslaking", 1);


    }

    private void AddVerbToBoard(Verb v)
    {
        GameObject verbFrame = Instantiate(prefabVerbFrame, pnlVerbs.transform) as GameObject;
        verbFrame.name = "Frame - " + v.Id;
        Image image = verbFrame.GetComponentsInChildren<Image>().Single(i => i.name == "VerbToken");
        Sprite sprite = ContentRepository.Instance.GetSpriteForVerb(v.Id);
        image.sprite = sprite;
        DraggableVerbToken token = verbFrame.GetComponentInChildren<DraggableVerbToken>();
        token.Verb = v;
    }

    public GameObject AddVerbTokenToParent(Verb v,Transform parentTransform)
    {
        GameObject verbToken=Instantiate(prefabVerbToken,parentTransform) as GameObject;
        Image image = verbToken.GetComponent<Image>();
        Sprite sprite = ContentRepository.Instance.GetSpriteForVerb(v.Id);
        DraggableVerbToken token = verbToken.GetComponent<DraggableVerbToken>();
        image.sprite = sprite;
        return verbToken;
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

    public void ModifyElementQuantity(string elementId, int quantity)
    {
        ModifyElementQuantity(elementId,quantity,null);
    }

    public void ModifyElementQuantity(string elementId,int quantity,int? siblingIndex)
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
        ModifyElementQuantity(elementId,elementQuantity);
        

        UpdateAspectDisplay();
    }

    public void AddChildSlots(SlotReceiveElement governingSlot, DraggableElementToken draggedElement)
    {
        
        float governingSlotHeight = governingSlot.GetComponent<RectTransform>().rect.height;
        Transform governingSlotTransform = governingSlot.transform;
        governingSlot.DependentChildSlotOrganiser = Instantiate(prefabChildSlotsOrganiser, pnlWorkspace.transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governingSlotTransform.localPosition.x, governingSlotTransform.localPosition.y - governingSlotHeight);
        governingSlot.DependentChildSlotOrganiser.transform.localPosition = newSlotPosition;

        governingSlot.DependentChildSlotOrganiser.GetComponent<ChildSlotOrganiser>().Populate(draggedElement);

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
        Log(pnlRecipeDisplay.CurrentRecipe.StartDescription,Style.Subtle);
        pnlWorld.AddTimer(currentRecipe,null);
        MarkRecipeAsKnown(currentRecipe);
    }

    public void Notify(string aside, string message, INotifyLocator notifyingGameObject)
    {
            GameObject objNotificationPanel = Instantiate(prefabNotificationPanel, transform) as GameObject;
            
            objNotificationPanel.transform.position = notifyingGameObject.GetNotificationPosition(); 
            NotificationPanel np = objNotificationPanel.GetComponent<NotificationPanel>();
             np.SetAside(aside);
            np.Lifespan = 3;
            np.SetBodyText(message);

    }

    /// <summary>
    /// return true if it was *already* known, false if it wasn't already known
    /// </summary>
    public Boolean MarkRecipeAsKnown(Recipe r)
    {
        if (knownRecipes.ContainsKey(r.Id))
            return true;

            knownRecipes.Add(r.Id,r);
            return false;
    }
    /// <summary>
    /// list of known recipes matching criterion
    /// </summary>
    /// <param name="withText">Empty string returns all known recipes; non-empty returns recipes whose labels match that substring</param>
    /// <returns></returns>
    public List<Recipe> GetKnownRecipes(string withText)
    {
       List<Recipe>matchingRecipes=new List<Recipe>();


        matchingRecipes.AddRange(knownRecipes.Values.Where(v=>v.Label.ToLower().Contains(withText) || withText==""));

        return matchingRecipes;
    }

    public void Log(string message, Style style)
    {
        pnlLog.Write(message);
    }


    public void DoHeartbeat()
    {
    pnlWorld.DoHeartbeat();
    }


    public void VerbAddedToSlot(Transform verbSlotTransform)
    {
        CurrentDragItem.transform.SetParent(verbSlotTransform);
        pnlWorkspace.MakeFirstSlotAvailable(verbSlotTransform.localPosition,prefabEmptyElementSlot);
        UpdateAspectDisplay();
    }

    public void ToggleKnownRecipesPanel()
    {
        Log("foo",Style.Assertive);
        pnlKnownRecipeDisplay.ToggleVisibility();
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
            foreach (RecipeSituation rt in GetAllCurrentRecipeTimers())
            {
                htRecipeTimers.Add(rt.Recipe.Id,rt.TimeRemaining);
            }

            Hashtable htRecipesKnown=new Hashtable();
            foreach (KeyValuePair<string,Recipe> kvpr in knownRecipes)
            {
                htRecipesKnown.Add(kvpr.Key, kvpr.Value);
            }


            Hashtable htSave=new Hashtable();

            htSave.Add(Noon.Constants.CONST_SAVE_ELEMENTSPOSSESSED,htElementsPossessed);
            htSave.Add(Noon.Constants.CONST_SAVE_RECIPETIMERS,htRecipeTimers);
            htSave.Add(Noon.Constants.CONST_SAVE_RECIPESKNOWN,htRecipesKnown);

            exportJson = htSave.JsonString();
            
            File.WriteAllText(Noon.NoonUtility.GetGameSavePath(), exportJson);
            Log("Saved the game; but not the world.", Style.Subtle);
        }
        catch (Exception)
        {
            Log("Something horrible has happened. We couldn't save the game.", Style.Subtle);
        }
    }

    

    public void LoadCurrentBoard()
    {
        try
        {
            string importJson = File.ReadAllText(Noon.NoonUtility.GetGameSavePath());
            Hashtable htSave = SimpleJsonImporter.Import(importJson);

            Hashtable htElementsPossessed = htSave.GetHashtable(Noon.Constants.CONST_SAVE_ELEMENTSPOSSESSED);
            Hashtable htRecipeTimers = htSave.GetHashtable(Noon.Constants.CONST_SAVE_RECIPETIMERS);
            Hashtable htRecipesKnown = htSave.GetHashtable(Noon.Constants.CONST_SAVE_RECIPESKNOWN);

            //check if it's all valid first
            foreach (string k in htElementsPossessed.Keys)
            {
                if (!ContentRepository.Instance.IsKnownElement(k))
                {
                    Log("Unknown element id: " + k, Style.Subtle);
                    throw new ApplicationException("Unknown element id " + k + " in save file");
                }
            }

            ClearBoard();

            foreach (string k in htElementsPossessed.Keys)
            {
                ModifyElementQuantity(k, Convert.ToInt32(htElementsPossessed[k]));
            }

            foreach (string k in htRecipeTimers.Keys)
            {
                Recipe r = ContentRepository.Instance.RecipeCompendium.GetRecipeById(k);
                pnlWorld.AddTimer(r, float.Parse(htRecipeTimers[k].ToString()));
            }

            foreach (string k in htRecipesKnown.Keys)
            {
                Recipe r = ContentRepository.Instance.RecipeCompendium.GetRecipeById(k);
                knownRecipes.Add(k,r);
            }

            Log("This is the game; I will be a pawn, or I will be a player.", Style.Subtle);
        }
        catch (Exception exception)
        {
            Log("Missing or invalid save file. Sorry! (" + exception.Message + ")", Style.Subtle);
        }
    }
}
