using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Commands;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using SecretHistories.UI;
using SecretHistories.Constants;
using JetBrains.Annotations;
using OrbCreationExtensions;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Enums;
using UnityEngine;
using UnityEngine.Events;

public enum LegacyEventRecordId
{
    lastcharactername,
    lastdesire,
    lasttool,
    lastbook,
    lastsignificantpainting,
    lastcult,
    lastheadquarters,
    lastfollower,
    lastpersonkilled

}

[IsEncaustableClass(typeof(CharacterCreationCommand))]
public class Character:MonoBehaviour,IEncaustable
    {
        [Encaust]
        public Legacy ActiveLegacy { get; set; }
        [Encaust]
        public Ending EndingTriggered { get; set; }

    [Encaust]
    public string Name
    {
        get { return _name; }
        set
        {

            _name = value;

            foreach (var s in new List<ICharacterSubscriber>(_subscribers))
                if (s.Equals(null))
                    Unsubscribe(s);
                else
                    s.CharacterNameUpdated(_name);
        }
    }

    [Encaust]
    public string Profession
    {
        get => _profession;
        set
        {

            foreach (var s in new List<ICharacterSubscriber>(_subscribers))
                if (s.Equals(null))
                    Unsubscribe(s);
                else
                    s.CharacterProfessionUpdated(_name);
        }
    }
    [Encaust]
    public Dictionary<string, int> RecipeExecutions => new Dictionary<string, int>(_recipeExecutions);

    [Encaust]
    public Dictionary<string, string> InProgressHistoryRecords=>new Dictionary<string, string>(_inProgressHistoryRecords);
    
    [Encaust]
    public Dictionary<string, string> PreviousCharacterHistoryRecords=> new Dictionary<string, string>(_previousCharacterHistoryRecords);


    [DontEncaust]
    public CharacterState State
    {
        get
        {
            if (EndingTriggered.IsValid())
                return CharacterState.Extinct;
            if (ActiveLegacy != null)
                return CharacterState.Viable;

            return CharacterState.Unformed;
        }
    }



    public Transform CurrentDecks;
    public DeckInstance DeckPrefab;


    private Dictionary<string, string> _inProgressHistoryRecords=new Dictionary<string, string>();
    private Dictionary<string, string> _previousCharacterHistoryRecords=new Dictionary<string, string>();

    public void Subscribe(ICharacterSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
    }

    public void Unsubscribe(ICharacterSubscriber subscriber)
    {
        _subscribers.Remove(subscriber);
    }




    private Dictionary<string, int> _recipeExecutions=new Dictionary<string, int>();
    private Dictionary<string, DeckInstance> _deckInstances=new Dictionary<string, DeckInstance>();
    private string _profession;
    private HashSet<ICharacterSubscriber> _subscribers = new HashSet<ICharacterSubscriber>();
    private string _name = "[unnamed]";

    
    public void Reincarnate(Legacy activeLegacy,Ending endingTriggered)
    {
    
            ActiveLegacy = activeLegacy;
            EndingTriggered = endingTriggered;

            var hb = new HistoryBuilder();
        if (EndingTriggered!=null)
            //The game has ended. The current character becomes the previous character.
            _previousCharacterHistoryRecords = hb.FillInDefaultPast(_inProgressHistoryRecords);
        else
        //the game hasn't ended yet. There may be existing previous
            _previousCharacterHistoryRecords = hb.FillInDefaultPast(_previousCharacterHistoryRecords);
        
        
        _inProgressHistoryRecords = new Dictionary<string, string>();
        _recipeExecutions= new Dictionary<string, int>();
        _deckInstances=new Dictionary<string, DeckInstance>();
        ResetStartingDecks();

        Name = Watchman.Get<ILocStringProvider>().Get("UI_CLICK_TO_NAME");
        // Registry.Retrieve<Chronicler>().CharacterNameChanged(NoonConstants.DEFAULT_CHARACTER_NAME);//so we never see a 'click to rename' in future history
        Profession = ActiveLegacy.Label;

    }


    public void ResetStartingDecks()
    {
        foreach (Transform deck in CurrentDecks)
            Destroy(deck.gameObject);

        
        var compendium = Watchman.Get<Compendium>();
        foreach (var ds in compendium.GetEntitiesAsAlphabetisedList<DeckSpec>())
        {
            DeckInstance di=Instantiate(DeckPrefab, CurrentDecks);
            di.SetSpec(ds);
            di.Shuffle();

            _deckInstances.Add(di.Id,di);
        }
    }



    public void AddExecutionsToHistory(string forRecipeId,int executions)
    {
        if (_recipeExecutions.ContainsKey(forRecipeId))
            _recipeExecutions[forRecipeId]+=executions;
        else
            _recipeExecutions[forRecipeId] = executions;
    }

    public void OverwriteExecutionsWith(Dictionary<string,int> newExecutions)
    {
        _recipeExecutions=new Dictionary<string, int>(newExecutions);
    }


    public int GetExecutionsCount(string forRecipeId)
    {
        if (_recipeExecutions.ContainsKey(forRecipeId))
            return _recipeExecutions[forRecipeId];

        return 0;
    }

    public bool HasExhaustedRecipe(Recipe forRecipe)
    {
        if (forRecipe.UnlimitedExecutionsPermitted())
            return false;

        return forRecipe.MaxExecutions <= GetExecutionsCount(forRecipe.Id);
    }

    public void SetOrOverwritePastLegacyEventRecord(string id, string value)
    {
if(string.IsNullOrEmpty(value))
    throw new ApplicationException("Error in LegacyEventRecord overwrite: shouldn't overwrite with an empty value, trying to erase the past for " + id.ToString());
if (_previousCharacterHistoryRecords.ContainsKey(id))
    _previousCharacterHistoryRecords[id] = value;
else
    _previousCharacterHistoryRecords.Add(id, value);
    }


    public void SetFutureLegacyEventRecord(string id, string value)
    {
        if (_inProgressHistoryRecords.ContainsKey(id))
            _inProgressHistoryRecords[id] = value;
        else
            _inProgressHistoryRecords.Add(id, value);
    }
    public string GetFutureLegacyEventRecord(string forId)
    {
        if (_inProgressHistoryRecords.ContainsKey(forId))
            return _inProgressHistoryRecords[forId];
        else
            return null;
    }


    public string GetPastLegacyEventRecord(string forId)
    {
        if (_previousCharacterHistoryRecords.ContainsKey(forId))
            return _previousCharacterHistoryRecords[forId];
        else
            return null;
    }

    public List<DeckInstance> GetAllDecks()
    {
        return _deckInstances.Values.ToList();
    }

    public DeckInstance GetDeckInstanceById(string id)
    {
        try
        {
            foreach (Transform d in CurrentDecks)
            {
                var deck = d.GetComponent<DeckInstance>();
                if (deck.Id == id)
                    return deck;
            }

            return null;
                
        }
        catch (Exception e)
        {
            NoonUtility.Log(e.Message + " for deck instance id " + id,2);
            throw;
        }
    }

    public void UpdateDeckInstanceFromSave(DeckSpec ds,Hashtable htEachDeck)
    {
        DeckInstance deckToUpdate;
            
        deckToUpdate= GetDeckInstanceById(ds.Id);
        if (deckToUpdate == null)
        {
            deckToUpdate = Instantiate(DeckPrefab, CurrentDecks);
            deckToUpdate.SetSpec(ds);
        }

  
        foreach (string key in htEachDeck.Keys)
        {
            if (key==SaveConstants.SAVE_ELIMINATEDCARDS)
            {
                ArrayList alEliminated = htEachDeck.GetArrayList(key);
                foreach (string eliminatedCard in alEliminated)
                    deckToUpdate.EliminateCardWithId(eliminatedCard);
            }
            else
            {
                deckToUpdate.Add(htEachDeck[key].ToString());
            }
        }


    }

 


}

