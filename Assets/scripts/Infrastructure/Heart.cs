using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour,ICharacterInfoSubscriber
{
    [SerializeField] private EndingPanel pnlEnding;
    //BoardManager is the previous top-level object, which I'm gradually pulling things out of.
    [SerializeField] private BoardManager BM;
    //Compendium contains all the config information on elements and recipes. It used to be just RecipeCompendium and may still be referred to that way
    public Compendium Compendium;

    private const string DO="Do"; //so we don't get a tiny daft typo with the Invoke
    public Character Character;
    public World World;
    public Character CreateBlankCharacter()
    {
        Character character = new Character();
        character.Subscribe(BM.characterNamePanel);
        character.Subscribe(this);
        character.SubscribeElementQuantityDisplay(BM);

        //hard-coded; ultimately this will go in a config file
        character.Title = "Mr";
        character.FirstName = "Vivian";
        character.LastName = "Keyes";


        //hard-coded; ultimately this will go in a config file
        character.ModifyElementQuantity("health", 3);
        character.ModifyElementQuantity("reason", 3);
        character.ModifyElementQuantity("clique", 1);
        character.ModifyElementQuantity("ordinarylife", 1);
        character.ModifyElementQuantity("suitablepremises", 1);
        // character.ModifyElementQuantity("health", 3);
        
        
        character.ModifyElementQuantity("occultscrap", 1);
      //  character.ModifyElementQuantity("shilling", 10);

        return character;
    }

  
    void Start () {
        //I don't have any dependency injection at the moand I want to unit-test Compendium, passing in Dice in the constructor so I can override it
        Compendium=new Compendium(new Dice());
        RefreshContent();
        
         BM = GameObject.Find("Board").GetComponent<BoardManager>();

        foreach (Verb v in Compendium.GetAllVerbs())
        {
            BM.AddVerbToBoard(v);
        }

        NewGame();

    }

    void BeginHeartbeat()
    {
        InvokeRepeating(DO, 0, 1);
    }

    void PauseHeartbeat()
    {
        CancelInvoke(DO);
    }

    void Do()
    {
        if(Character.State==CharacterState.Viable)
            World.DoHeartbeat();
    }

    public void ReceiveUpdate(Character character)
    {
        if (character.State == CharacterState.Extinct)
        {
      EndGame();
        }
    }

    public void EndGame()
    {
        PauseHeartbeat();
        BM.gameObject.SetActive(false);

        pnlEnding.gameObject.SetActive(true);
        pnlEnding.DetailText.text = Character.EndingTriggeredId;
    }

    public void NewGame()
    {
        pnlEnding.gameObject.SetActive(false);
        BM.gameObject.SetActive(true);

        World = new World(Compendium);

        BM.ClearBoard();
       

        BM.QueueRecipe(Compendium.GetRecipeById("starvation"));

        BeginHeartbeat();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void RefreshContent()
    {
        //ContentImporter goes through the JSON and does a (big, brittle, hard-coded) import to populate the compendium
        ContentImporter ContentImporter = new ContentImporter();
        ContentImporter.PopulateCompendium(Compendium);
    }
}
