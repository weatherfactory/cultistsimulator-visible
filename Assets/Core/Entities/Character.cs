using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using JetBrains.Annotations;
using Noon;

public enum LegacyEventRecordId
{
    LastNotion,
    LastTool,
    LastBook,
    LastSignificantPainting,
    LastCult,
    LastHeadquarters,
    LastPersonKilledName
}

public class Character:IGameEntityStorage
{
    private string _name="[unnamed]";
    private string _previouscharactername = "[UNKNOWN]";
    
    public CharacterState State { get; set; }
    public List<IDeckInstance> DeckInstances { get; set; }
    public Dictionary<LegacyEventRecordId, string> _legacyEventRecords;

    private Dictionary<string, int> recipeExecutions;
    private string _endingTriggeredId = null;

    public Character() : this(null)
    {

    }

    public Character(Character previousCharacter)
    {
        State = CharacterState.Viable;
        recipeExecutions = new Dictionary<string, int>();
        DeckInstances = new List<IDeckInstance>();
        if (previousCharacter == null)
            _legacyEventRecords = new Dictionary<LegacyEventRecordId, string>();
        else
            _legacyEventRecords = previousCharacter.GetAllLegacyEventRecords();
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
        if (forRecipe.HasInfiniteExecutions())
            return false;

        return forRecipe.MaxExecutions <= GetExecutionsCount(forRecipe.Id);
    }

    public void SetLegacyEventRecord(LegacyEventRecordId id, string value)
    {
        _legacyEventRecords.Add(id,value);
    }

    public string GetLegacyEventRecord(LegacyEventRecordId forId)
    {
        if (_legacyEventRecords.ContainsKey(forId))
            return _legacyEventRecords[forId];
        else
            return null;
    }

    public IDeckInstance GetDeckInstanceById(string id)
    {
        return  DeckInstances.SingleOrDefault(d => d.Id == id);
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Profession { get ; set; }
    public string PreviousCharacterName
    {
        get { return _previouscharactername; }
        set { _previouscharactername = value; }
    }

    public string EndingTriggeredId
    {
        get { return _endingTriggeredId; }
    }


    public string ReplaceTextFor(string text)
    {
        if (text == null)
            return null; //huh. It really shouldn't be - I should be assigning empty string on load -  and yet sometimes it is. This is a guard clause to stop a basic nullreferenceexception
        var replaced = text.Replace(NoonConstants.TOKEN_PREVIOUS_CHARACTER_NAME, PreviousCharacterName);
        
        return replaced;
    }


    public Dictionary<LegacyEventRecordId, string> GetAllLegacyEventRecords()
    {
        return new Dictionary<LegacyEventRecordId,string>(_legacyEventRecords);
    }
}

