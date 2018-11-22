#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TabletopUi.Scripts.Interfaces;
using TMPro;
using UnityEngine.UI;
using UnityEngine.VR;

public class DebugTools : MonoBehaviour,IRollOverride
{
    private const int MaxAutoCompletionSuggestions = 50;

    [SerializeField] private TabletopTokenContainer tabletop;
    [SerializeField] private Heart heart;
    [SerializeField] private InputField input;
    [SerializeField] private ScrollRect autoCompletionBox;
    [SerializeField] private VerticalLayoutGroup autoCompletionSuggestions;
    [SerializeField] private Button btnPlusOne;
    [SerializeField] private Button btnMinusOne;
    [SerializeField] private Button btnBeginSituation;
    [SerializeField] private Button btnHaltVerb;
    [SerializeField] public Button btnTriggerAchievement;
    [SerializeField] private Button btnResetAchivement;
    [SerializeField] private Button btnFastForward;
    [SerializeField] private Button btnNextTrack;
    [SerializeField] private Button btnUpdateContent;
    [SerializeField] private Button btnEndGame;
    [SerializeField] private Button btnLoadGame;
    [SerializeField] private Button btnSaveGame;
    [SerializeField] private Button btnResetDecks;
    [SerializeField] private BackgroundMusic backgroundMusic;
    [SerializeField] private Button btnQueueRoll;
    [SerializeField] private TMP_InputField rollToQueue;
    [SerializeField] private TextMeshProUGUI rollsQueued;

    // Debug Load/Save/Delete buttons
    [SerializeField] private List<Button> saveButtons;
    [SerializeField] private List<Button> loadButtons;
    [SerializeField] private List<Button> delButtons;

    public string endingAnimFXName = "DramaticLightEvil";

    public List<int> QueuedRollsList;

    public Transform AutoCompletionSuggestion;

    public void Awake()
    {
        autoCompletionBox.gameObject.SetActive(false);
        input.onValueChanged.AddListener(AttemptAutoCompletion);
        btnPlusOne.onClick.AddListener(() => AddCard(input.text));
        btnMinusOne.onClick.AddListener(() => RemoveItem(input.text));
        btnFastForward.onClick.AddListener(() => FastForward(30));
        btnUpdateContent.onClick.AddListener(UpdateCompendiumContent);
        btnEndGame.onClick.AddListener(()=>EndGame(input.text));
        btnLoadGame.onClick.AddListener(LoadGame);
        btnSaveGame.onClick.AddListener(SaveGame);
        btnResetDecks.onClick.AddListener(ResetDecks);
        btnNextTrack.onClick.AddListener(NextTrack);
        btnBeginSituation.onClick.AddListener(()=>BeginSituation(input.text));
        btnHaltVerb.onClick.AddListener(() => HaltVerb(input.text));
        btnTriggerAchievement.onClick.AddListener(()=>TriggerAchievement(input.text));
        btnResetAchivement.onClick.AddListener(() => ResetAchievement(input.text));


        btnQueueRoll.onClick.AddListener(()=>QueueRoll(rollToQueue.text));

        QueuedRollsList=new List<int>();

        for (int i = 0; i < saveButtons.Count; i++)
        {
            var index = i;
            saveButtons[i].onClick.AddListener(() => SaveDebugSave(index));
        }
        for (int i = 0; i < loadButtons.Count; i++)
        {
            var index = i;
            loadButtons[i].onClick.AddListener(() => LoadDebugSave(index));
            if (!CheckDebugSaveExists(i))
                loadButtons[i].interactable = false;
        }
        for (int i = 0; i < delButtons.Count; i++)
        {
            var index = i;
            delButtons[i].onClick.AddListener(() => DeleteDebugSave(index));
            if (!CheckDebugSaveExists(i))
                delButtons[i].interactable = false;
        }

    }

    void AttemptAutoCompletion(string value)
    {
        // Don't show the suggestion box if the field is empty
        if (value.Length == 0)
        {
            autoCompletionBox.gameObject.SetActive(false);
            return;
        }
        autoCompletionBox.gameObject.SetActive(true);

        // Clear the list
        foreach (Transform child in autoCompletionSuggestions.transform)
            Destroy(child.gameObject);

        // Re-populate it with updated suggestions
        // Disable the suggestion box if there are no suggestions
        List<string> suggestedIds = GetAutoCompletionSuggestions(value);
        if (suggestedIds.Count == 0)
        {
            autoCompletionBox.gameObject.SetActive(false);
            return;
        }
        foreach (var suggestedId in suggestedIds)
        {
            Transform suggestion = Instantiate(AutoCompletionSuggestion);
            suggestion.GetComponentInChildren<Text>().text = suggestedId;
            suggestion.SetParent(autoCompletionSuggestions.transform, false);
            Button button = suggestion.GetComponent<Button>();
            var suggestionText = suggestedId;
            button.onClick.AddListener(() => ApplySuggestion(suggestionText));
        }
    }

    void ApplySuggestion(string suggestion)
    {
        // Temporarily disable suggestions so that this doesn't trigger a new auto-completion attempt
        input.onValueChanged.RemoveListener(AttemptAutoCompletion);
        input.text = suggestion;
        input.onValueChanged.AddListener(AttemptAutoCompletion);
        autoCompletionBox.gameObject.SetActive(false);
    }

    List<string> GetAutoCompletionSuggestions(string prompt)
    {
        ICompendium compendium = Registry.Retrieve<ICompendium>();
        List<string> candidates = compendium.GetAllElementsAsDictionary().Keys.
            Where(e => e.StartsWith(prompt)).ToList();
        candidates.AddRange(compendium.GetAllRecipesAsList().Where(r => r.Id.StartsWith(prompt)).Select(r => r.Id));
        return candidates.OrderBy(id => id).Take(MaxAutoCompletionSuggestions).ToList();
    }

    void AddCard(string elementId)
    {
        var stackManager = tabletop.GetElementStacksManager();
        var existingStacks = stackManager.GetStacks();

        var element = Registry.Retrieve<ICompendium>().GetElementById(elementId);

        if (element == null) {
            Debug.LogWarning("No Element with ID " + elementId + " found!");
            return;
        }

        //check if there's an existing stack of that type to increment
        if (!element.Unique) {
            foreach (var stack in existingStacks)
            {
                if(stack.EntityId==elementId)
                {
                    stack.ModifyQuantity(1);
                    return;
                }
            }
        }
        //if we didn't jump out of loop with return, above
        stackManager.ModifyElementQuantity(elementId,1, Source.Existing(), new Context(Context.ActionSource.Debug));
    }

    void RemoveItem(string itemId)
    {
        //do we have an inactive empty verb with this id?
       var possibleEmptyVerb= Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations().FirstOrDefault(s => s.situationToken.EntityId==itemId);
        if(possibleEmptyVerb!=null)
        { if(!possibleEmptyVerb.situationWindow.GetOutputStacks().Any() && !possibleEmptyVerb.IsOngoing)
            possibleEmptyVerb.Retire();
        }
        else
        tabletop.GetElementStacksManager().ModifyElementQuantity(itemId, -1, Source.Existing(), new Context(Context.ActionSource.Debug));
    }

    void BeginSituation(string recipeId)
    {
        var compendium = Registry.Retrieve<ICompendium>();
        var recipe = compendium.GetRecipeById(recipeId.Trim());
        if (recipe!=null)
        {
            var situationEffectCommand=new SituationEffectCommand(recipe,true,null);

        IVerb verbForNewSituation = compendium.GetOrCreateVerbForCommand(situationEffectCommand);
        SituationCreationCommand scc = new SituationCreationCommand(verbForNewSituation, recipe, SituationState.FreshlyStarted);
        Registry.Retrieve<ITabletopManager>().BeginNewSituation(scc,new List<IElementStack>());
        }
        else
        Debug.Log("couldn't find this recipe: " + recipeId);
    }

    void HaltVerb(string verbId)
    {
        var situationsCatalogue = Registry.Retrieve<SituationsCatalogue>();
        foreach (var s in situationsCatalogue.GetRegisteredSituations())
        {
            if(s.GetTokenId()==verbId.Trim())
                s.Halt();
        }

    }

    void BeginLegacy(string legacyId)
    {
        var l = Registry.Retrieve<ICompendium>().GetAllLegacies().SingleOrDefault(leg => leg.Id == legacyId);
        if (l == null)
            return;


    }

    void TriggerAchievement(string achievementId)
    {
        var storefrontServicesProvider = Registry.Retrieve<StorefrontServicesProvider>();
        storefrontServicesProvider.SetAchievementForCurrentStorefronts(achievementId,true);
    }

    void ResetAchievement(string achievementId)
    {
        var storefrontServicesProvider = Registry.Retrieve<StorefrontServicesProvider>();
        storefrontServicesProvider.SetAchievementForCurrentStorefronts(achievementId, false);
    }

    void FastForward(float interval)
    {
            heart.AdvanceTime(interval);
    }

    void UpdateCompendiumContent()
    {
        var contentImporter=new ContentImporter();
        contentImporter.PopulateCompendium(Registry.Retrieve<ICompendium>());
    }

    void NextTrack()
    {
        backgroundMusic.PlayNextClip();
    }

    // to allow access from HotkeyWatcher
    public void EndGame(string endingId)
    {
        var compendium = Registry.Retrieve<ICompendium>();

        var ending = compendium.GetEndingById(endingId);

        ending.Anim = endingAnimFXName;

        // Get us a random situation that killed us!
        var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
        var deathSit = situationControllers[UnityEngine.Random.Range(0, situationControllers.Count)];

        Registry.Retrieve<ITabletopManager>().EndGame(ending, deathSit);
    }

    public void LoadGame()
    {
        Registry.Retrieve<ITabletopManager>().LoadGame();
    }

    public void SaveGame()
    {
        Registry.Retrieve<ITabletopManager>().SaveGame(true);
    }

    void ResetDecks()
    {
        var character= Registry.Retrieve<Character>();
        foreach(var di in character.DeckInstances)
        {di.Reset();
            NoonUtility.Log("Reset " + di.Id + " - now contains ");
            foreach (var card in di.GetCurrentCardsAsList())
            {
                NoonUtility.Log(card + "\n" );
            }
        }
    }

    void QueueRoll(string roll)
    {
        int rollValue;
        int.TryParse(roll, out rollValue);
        if(rollValue>=1 && rollValue<=100)
            QueuedRollsList.Add(rollValue);

        UpdatedQueuedRollsDisplay();

    }

    public void UpdatedQueuedRollsDisplay()
    {
        rollsQueued.text = string.Empty;
        foreach(var i in QueuedRollsList)
        {
            if (rollsQueued.text!="")
                rollsQueued.text += ", ";

            rollsQueued.text += i.ToString();
        }
    }

    public int PopNextOverrideValue(Recipe recipe = null)
    {
        if (!QueuedRollsList.Any())
            return 0;
        else
        {
        int result = QueuedRollsList.First();
            QueuedRollsList.RemoveAt(0);
            UpdatedQueuedRollsDisplay();
            return result;
        }
    }

    void SaveDebugSave(int index)
    {
        ITabletopManager tabletopManager = Registry.Retrieve<ITabletopManager>();
        bool wasSuccessful = tabletopManager.SaveGame(true, index + 1);
        loadButtons[index].interactable = wasSuccessful;
        delButtons[index].interactable = wasSuccessful;
    }

    void LoadDebugSave(int index)
    {
        if (!CheckDebugSaveExists(index))
            return;
        ITabletopManager tabletopManager = Registry.Retrieve<ITabletopManager>();
        tabletopManager.LoadGame(index + 1);
    }

    void DeleteDebugSave(int index)
    {
        if (!CheckDebugSaveExists(index))
            return;
        File.Delete(NoonUtility.GetGameSaveLocation(index + 1));
        loadButtons[index].interactable = false;
        delButtons[index].interactable = false;
    }

    private bool CheckDebugSaveExists(int index)
    {
        return File.Exists(NoonUtility.GetGameSaveLocation(index + 1));
    }
}

public interface IRollOverride
{
    //if at least one override is queued, pop it and return it
    //if none are queued, return 0
    // If a contextual recipe is passed, it can affect the dice roll
    int PopNextOverrideValue(Recipe recipe = null);

}
