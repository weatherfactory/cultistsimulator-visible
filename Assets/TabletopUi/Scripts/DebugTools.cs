#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts;
using Noon;
using UnityEngine.UI;

public class DebugTools : MonoBehaviour
{
    
    [SerializeField] private Tabletop tabletop;
    [SerializeField] private Heart heart;
    [SerializeField] private InputField input;
    [SerializeField] private Button btnPlusOne;
    [SerializeField] private Button btnMinusOne;
    [SerializeField]private Button btnBeginSituation;
    [SerializeField] private Button btnFastForward;
    [SerializeField]private Button btnNextTrack;
    [SerializeField] private Button btnUpdateContent;
    [SerializeField] private Button btnEndGame;
    [SerializeField] private Button btnLoadGame;
    [SerializeField] private Button btnSaveGame;
    [SerializeField] private Button btnResetDecks;
    [SerializeField] private BackgroundMusic backgroundMusic;


    public void Awake()
    {
        btnPlusOne.onClick.AddListener(() => AddCard(input.text));
        btnMinusOne.onClick.AddListener(() => DecrementElement(input.text));
        btnFastForward.onClick.AddListener(() => FastForward(30));
        btnUpdateContent.onClick.AddListener(UpdateCompendiumContent);
        btnEndGame.onClick.AddListener(EndGame);
        btnLoadGame.onClick.AddListener(LoadGame);
        btnSaveGame.onClick.AddListener(SaveGame);
        btnResetDecks.onClick.AddListener(ResetDecks);
        btnNextTrack.onClick.AddListener(NextTrack);
        btnBeginSituation.onClick.AddListener(()=>BeginSituation(input.text));

    }
    void AddCard(string elementId)
    {
        var stackManager = tabletop.GetElementStacksManager();

        var existingStacks = stackManager.GetStacks();

        //check if there's an existing stack of that type to increment
        foreach (var stack in existingStacks)
        {
            if(stack.Id==elementId)
            { 
                stack.ModifyQuantity(1);
                return;
            }
        }
        //if we didn't jump out of loop with return, above
       stackManager.ModifyElementQuantity(elementId,1, Source.Existing());
    }

    void DecrementElement(string elementId)
    {
        tabletop.GetElementStacksManager().ModifyElementQuantity(elementId, -1, Source.Existing());
    }

    void BeginSituation(string recipeId)
    {
        var compendium = Registry.Retrieve<ICompendium>();
        var recipe = compendium.GetRecipeById(recipeId);
        if (recipe!=null)
        { 
            var situationEffectCommand=new SituationEffectCommand(recipe,true);

        IVerb verbForNewSituation = compendium.GetOrCreateVerbForCommand(situationEffectCommand);
        SituationCreationCommand scc = new SituationCreationCommand(verbForNewSituation, recipe, SituationState.FreshlyStarted);
        Registry.Retrieve<TabletopManager>().BeginNewSituation(scc);
        }
        else
        Debug.Log("couldn't find this recipe: " + recipeId);
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

    void EndGame()
    {
        var compendium = Registry.Retrieve<ICompendium>();
        var ending = compendium.GetEndingById("powerminor");
        Registry.Retrieve<TabletopManager>().EndGame(ending);
    }

    public void LoadGame()
    {
        Registry.Retrieve<TabletopManager>().LoadGame();
    }

    public void SaveGame()
    {
        Registry.Retrieve<TabletopManager>().SaveGame(true);
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

}

