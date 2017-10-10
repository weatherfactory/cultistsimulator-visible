using UnityEngine;
using System.Collections;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts;
using UnityEngine.UI;

public class DebugTools : MonoBehaviour
{
    
    [SerializeField] private TabletopContainer tabletopContainer;
    [SerializeField] private Heart heart;
    [SerializeField] private InputField input;
    [SerializeField] private Button btnPlusOne;
    [SerializeField] private Button btnMinusOne;
    [SerializeField]private Button btnBeginSituation;
    [SerializeField] private Button btnFastForward;
    [SerializeField]private Button btnNextTrack;
    [SerializeField] private Button btnUpdateContent;
    [SerializeField] private Button btnEndGame;
    [SerializeField] private BackgroundMusic backgroundMusic;


    public void Awake()
    {
        btnPlusOne.onClick.AddListener(() => AddCard(input.text));
        btnMinusOne.onClick.AddListener(() => DecrementElement(input.text));
        btnFastForward.onClick.AddListener(() => FastForward(30));
        btnUpdateContent.onClick.AddListener(UpdateCompendiumContent);
        btnEndGame.onClick.AddListener(EndGame);
        btnNextTrack.onClick.AddListener(NextTrack);
        btnBeginSituation.onClick.AddListener(()=>BeginSituation(input.text));

    }
    void AddCard(string elementId)
    {
        tabletopContainer.GetElementStacksManager().ModifyElementQuantity(elementId,1);
    }

    void DecrementElement(string elementId)
    {
        tabletopContainer.GetElementStacksManager().ModifyElementQuantity(elementId, -1);
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
            heart.Beat(interval);
    }

    void UpdateCompendiumContent()
    {
        Registry.Retrieve<TabletopManager>().UpdateCompendium(Registry.Retrieve<ICompendium>());
    }

    void NextTrack()
    {
        backgroundMusic.PlayNextClip();
    }

    void EndGame()
    {
        var compendium = Registry.Retrieve<ICompendium>();
        var ending = compendium.GetEndingForFlag("powerminor");
        Registry.Retrieve<TabletopManager>().EndGame(ending);
    }
}

