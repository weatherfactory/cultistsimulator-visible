using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Assets.CS.TabletopUI;
using JetBrains.Annotations;
using Noon;
using UnityEngine;

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

public class Character:MonoBehaviour
{
    private string _name="[unnamed]";

    public CharacterState State
    {
        get
        {
            if (EndingTriggered != null)
                return CharacterState.Extinct;
            if (ActiveLegacy != null)
                return CharacterState.Viable;

            return CharacterState.Unformed;
        }


    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Profession { get; set; }


    public Dictionary<string, string> GetInProgressHistoryRecords()
    {
        return new Dictionary<string, string>(_inProgressHistoryRecords);
    }

    public Dictionary<string, string> GetPreviousCharacterHistoryRecords()
    {
        return new Dictionary<string, string>(_previousCharacterHistoryRecords);
    }

    public List<IDeckInstance> DeckInstances { get; set; } 
    private Dictionary<string, string> _inProgressHistoryRecords;
    private Dictionary<string, string> _previousCharacterHistoryRecords;
    public Legacy ActiveLegacy { get; set; }
    public Ending EndingTriggered { get; set; }

    private Dictionary<string, int> recipeExecutions;




    public void Reset(Legacy activeLegacy,Ending endingTriggered)
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
        recipeExecutions= new Dictionary<string, int>();
        DeckInstances = new List<IDeckInstance>();
        
        //hmm re Compendium
        foreach (var ds in Registry.Get<ICompendium>().GetEntitiesAsList<DeckSpec>())
        {
                IDeckInstance di = new DeckInstance(ds);
                DeckInstances.Add(di);
                di.Reset();
        }

    }


    public void ClearExecutions()
    {
        recipeExecutions.Clear();
    }

    public Dictionary<string, int> GetAllExecutions()
    {
        return new Dictionary<string, int>(recipeExecutions);
    }

    public void AddExecutionsToHistory(string forRecipeId,int executions)
    {
        if (recipeExecutions.ContainsKey(forRecipeId))
            recipeExecutions[forRecipeId]+=executions;
        else
            recipeExecutions[forRecipeId] = executions;
    }

    public int GetExecutionsCount(string forRecipeId)
    {
        if (recipeExecutions.ContainsKey(forRecipeId))
            return recipeExecutions[forRecipeId];

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

    public IDeckInstance GetDeckInstanceById(string id)
    {
        try
        {

            return  DeckInstances.SingleOrDefault(d => d.Id == id);
        }
        catch (Exception e)
        {
            NoonUtility.Log(e.Message + " for deck instance id " + id,2);
            throw;
        }
    }

    public bool OverwriteDeckInstance(IDeckInstance newDeckInstance)
    {
        //TODO: this should be a dictionary, obv
        var existingDeckInstance = DeckInstances.SingleOrDefault(di => di.Id == newDeckInstance.Id);

        if(existingDeckInstance==null)
        {
            DeckInstances.Add(newDeckInstance);
            return false;
        }
        else
        {
            DeckInstances.Remove(existingDeckInstance);
            DeckInstances.Add(newDeckInstance);
            return true;
        }

    }

 


}

