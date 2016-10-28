using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrbCreationExtensions;
using UnityEngine.UI;

#pragma warning disable 649

public class BoardManager : MonoBehaviour,INotifier,IElementQuantityDisplay,IRecipeSituationSubscriber
{
    [SerializeField] private InputField inputAdjustElementNamed;
    [SerializeField] private LogPanel pnlLog;
    [SerializeField] private VerbPanel pnlVerbs;
    [SerializeField] private GameObject pnlResources;
    [SerializeField] Workspace pnlWorkspace;
    [SerializeField] private WorldPanel pnlWorld;
    [SerializeField] AspectsDisplay pnlCurrentAspects;
    [SerializeField]RecipeDisplay pnlRecipeDisplay;
    [SerializeField] private KnownRecipeDisplay pnlKnownRecipeDisplay;
    [SerializeField] private GameObject objLimbo;
    [SerializeField]GameObject prefabElementToken;
    [SerializeField]GameObject prefabVerbToken;
    [SerializeField]GameObject prefabEmptyElementSlot;
    [SerializeField]GameObject prefabChildSlotsOrganiser;
    [SerializeField]private GameObject prefabNotificationPanel;
    [SerializeField] public CharacterNamePanel characterNamePanel;
    [SerializeField] private Heartbeat heartbeat;

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
    private DraggableElementToken getStoredElementTokenForId(string elementId)
    {
        DraggableElementToken[] existingElementTokens = GetAllStoredElementTokens();
        return
            existingElementTokens.SingleOrDefault(e => e.Element.Id == elementId);
    }



 


    public void AddVerbToBoard(Verb v)
    {
        pnlVerbs.AddVerbToPanel(v);
        
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

    /// <summary>
    ///WARNING: This does *not* currently address the workspace.
    /// </summary>
    public int GetCurrentElementQuantity(string elementId)
    {
        DraggableElementToken det = getStoredElementTokenForId(elementId);
        if (det == null)
            return 0;
        return det.Quantity;
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


    public void FastForward(int seconds)
    {
        pnlWorld.FastForward(seconds,heartbeat.Character);
    }

    public void UpdateForElementQuantity(string elementId, int quantity)
    {
        DraggableElementToken existingElement = getStoredElementTokenForId(elementId);
        if (existingElement)
            existingElement.SetQuantity(quantity);
        else
        {
            addElementToBoard(elementId, quantity, null);
        }
    }



    public void ClearWorkspace()
    {
        pnlWorkspace.ReturnAllElementsToOrigin();
        pnlCurrentAspects.ResetAspects();
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
        heartbeat.Character.ModifyElementQuantity(elementId,elementQuantity);
        

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

    public void ModifyElementQuantity(string e, int q)
    {
        heartbeat.Character.ModifyElementQuantity(e,q);
    }

    public void UpdateAspectDisplay()
    {
            pnlCurrentAspects.UpdateAspects(pnlWorkspace);
    }

    public void DisplayRecipe(Recipe recipe)
    {
        pnlRecipeDisplay.GetComponent<RecipeDisplay>().DisplayRecipe(recipe);
    }

    public string GetCurrentVerbId()
    {
        return pnlWorkspace.GetCurrentVerbId();
    }


    public void QueueRecipe(Recipe r)
    {
        pnlVerbs.BlockVerb(r.ActionId);
        pnlWorkspace.ConsumeElements();
        Log(pnlRecipeDisplay.CurrentRecipe.StartDescription, Style.Subtle);
        pnlWorld.AddTimer(r, null,this);
        MarkRecipeAsKnown(r);
     
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
        if (!r.Craftable)
            return false;

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


    public void DoHeartbeat(Character c)
    {
    pnlWorld.DoHeartbeat(c);
    }


    public void VerbAddedToSlot(Transform verbSlotTransform)
    {
        CurrentDragItem.transform.SetParent(verbSlotTransform);
        pnlWorkspace.MakeFirstSlotAvailable(verbSlotTransform.localPosition,prefabEmptyElementSlot);
        UpdateAspectDisplay();
    }

    public void ToggleKnownRecipesPanel()
    {
        pnlKnownRecipeDisplay.ToggleVisibility();
    }

    /// <summary>
    /// DANGEROUS removes all elements from storage and workspace
    /// </summary>
    public void ClearBoard()
    {

        ClearWorkspace();
        DraggableElementToken[]  allElementTokens=GetAllStoredElementTokens();
        foreach (DraggableElementToken e in allElementTokens)
        {
            e.SetQuantity(0);
        }
        ClearRecipeTimers();
        ClearKnownRecipes();
        heartbeat.Character = heartbeat.CreateBlankCharacter();
    }

    private void ClearKnownRecipes()
    {
        knownRecipes.Clear();
    }


    public void SaveCurrentBoard()
    {
        try
        {
            string exportJson;

            Hashtable htElementsPossessed = new Hashtable();
            foreach (KeyValuePair<string,int> e in heartbeat.Character.GetAllCurrentElements())
            {
                htElementsPossessed.Add(e.Key,e.Value);
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

            Hashtable htCharacter = new Hashtable()
            {
                {Noon.Constants.KCHARACTERTITLE, heartbeat.Character.Title},
                {Noon.Constants.KCHARACTERFIRSTNAME, heartbeat.Character.FirstName},
                {Noon.Constants.KCHARACTERLASTNAME, heartbeat.Character.LastName},
                {Noon.Constants.KCHARACTERSTATE,heartbeat.Character.State.ToString() }
            };

            Hashtable htAdvice = new Hashtable()
            {
                {"Hello_from_AK", "Cultist Simulator has one save file, which is overwritten on character death."},
                {"2", " If you want to keep multiple saves or reload after death, you can copy this file. "},
                {
                    "3",
                    "All the values are also in an easy human-readable format, so you can feel free to give yourself more resources."
                },
                {"4", " Only you will ever know."},
                {
                    "5",
                    "(You, and the White-Glass, in whom there is no concealment, and who will be observed at the end.)"
                }
            };


            Hashtable htSave = new Hashtable
            {
                {Noon.Constants.CONST_SAVE_ELEMENTSPOSSESSED, htElementsPossessed},
                {Noon.Constants.CONST_SAVE_RECIPETIMERS, htRecipeTimers},
                {Noon.Constants.CONST_SAVE_RECIPESKNOWN, htRecipesKnown},
                {Noon.Constants.CONST_SAVE_CHARACTER_DETAILS, htCharacter}
            };
        htSave.Add("_",htAdvice);


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
            Hashtable htCharacter = htSave.GetHashtable(Noon.Constants.CONST_SAVE_CHARACTER_DETAILS);

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

            heartbeat.Character.Title = htCharacter[Noon.Constants.KCHARACTERTITLE].ToString();
            heartbeat.Character.FirstName = htCharacter[Noon.Constants.KCHARACTERFIRSTNAME].ToString();
            heartbeat.Character.LastName = htCharacter[Noon.Constants.KCHARACTERLASTNAME].ToString();
            heartbeat.Character.State = (CharacterState)Enum.Parse(typeof(CharacterState), htCharacter[Noon.Constants.KCHARACTERSTATE].ToString());

            foreach (string k in htElementsPossessed.Keys)
            {
                heartbeat.Character.ModifyElementQuantity(k, Convert.ToInt32(htElementsPossessed[k]));
            }

            foreach (string k in htRecipeTimers.Keys)
            {
                Recipe r = ContentRepository.Instance.RecipeCompendium.GetRecipeById(k);
                pnlWorld.AddTimer(r, float.Parse(htRecipeTimers[k].ToString()), this);
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
            throw;
        }
    }

    public void BlockVerb(string recipeActionId)
    {
        throw new NotImplementedException();
    }
    public void UnblockVerb(string recipeActionId)
    {
        pnlVerbs.UnblockVerb(recipeActionId);
    }

    public void SituationComplete(Recipe recipe)
    {
        UnblockVerb(recipe.ActionId);
    }
}
