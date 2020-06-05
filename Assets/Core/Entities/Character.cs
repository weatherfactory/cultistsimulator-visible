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

public enum LegacyEventRecordId
{
    LastCharacterName,
    LastDesire,
    LastTool,
    LastBook,
    LastSignificantPainting,
    LastCult,
    LastHeadquarters,
    LastFollower,
    LastPersonKilled

}

public class Character:IGameEntityStorage
{
    private string _name="[unnamed]";
    
    public CharacterState State { get; set; }
    public List<IDeckInstance> DeckInstances { get; set; }
    private Dictionary<LegacyEventRecordId, string> _futureLegacyEventRecords;
    private Dictionary<LegacyEventRecordId, string> _pastLegacyEventRecords;
    public Legacy ActiveLegacy { get; set; }

    private Dictionary<string, int> recipeExecutions;
    private string _endingTriggeredId = null;

    public Character(Legacy activeLegacy) : this(activeLegacy,null)
    {

    }

    public Character(Legacy activeLegacy, Character previousCharacter)
    {
        State = CharacterState.Viable;
        recipeExecutions = new Dictionary<string, int>();
        DeckInstances = new List<IDeckInstance>();
        //if we have a previous character, base our past on their future
        if (previousCharacter != null)
        {
            _pastLegacyEventRecords = previousCharacter.GetAllFutureLegacyEventRecords(); //THEIR FUTURE IS OUR PAST
        }
        //otherwise, create a blank slate
        else 
            _pastLegacyEventRecords = new Dictionary<LegacyEventRecordId, string>();

        //the history builder will then provide a default value for any empty ones.
        HistoryBuilder hb = new HistoryBuilder();
        _pastLegacyEventRecords = hb.FillInDefaultPast(_pastLegacyEventRecords);

        //finally, set our starting future to be our present, ie our past.
        _futureLegacyEventRecords = new Dictionary<LegacyEventRecordId, string>(_pastLegacyEventRecords);

        ActiveLegacy = activeLegacy;

    }

    // Turns this character into a defunct character based on the past of the current, active character
    public static Character MakeDefunctCharacter(IGameEntityStorage currentCharacter)
    {
        return new Character(null)
        {
            recipeExecutions = new Dictionary<string, int>(),
            DeckInstances = new List<IDeckInstance>(),
            State = CharacterState.Extinct,
            _futureLegacyEventRecords = currentCharacter.GetAllPastLegacyEventRecords(),

            // Turn all past records back into future records, to simulate a character whose run ended
            _pastLegacyEventRecords = new HistoryBuilder().FillInDefaultPast(new Dictionary<LegacyEventRecordId, string>()),
            
            // Load in a default legacy, since it doesn't matter for the defunct character
            ActiveLegacy = Registry.Retrieve<ICompendium>().GetAllLegacies().First()
        };
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
        if (forRecipe.HasInfiniteExecutions())
            return false;

        return forRecipe.MaxExecutions <= GetExecutionsCount(forRecipe.Id);
    }

    public void SetOrOverwritePastLegacyEventRecord(LegacyEventRecordId id, string value)
    {
if(string.IsNullOrEmpty(value))
    throw new ApplicationException("Error in LegacyEventRecord overwrite: shouldn't overwrite with an empty value, trying to erase the past for " + id.ToString());
        if (_pastLegacyEventRecords.ContainsKey(id))
            _pastLegacyEventRecords[id] = value;
        else
            _pastLegacyEventRecords.Add(id, value);
    }


    public void SetFutureLegacyEventRecord(LegacyEventRecordId id, string value)
    {
        if (_futureLegacyEventRecords.ContainsKey(id))
            _futureLegacyEventRecords[id] = value;
        else
            _futureLegacyEventRecords.Add(id, value);
    }
    public string GetFutureLegacyEventRecord(LegacyEventRecordId forId)
    {
        if (_futureLegacyEventRecords.ContainsKey(forId))
            return _futureLegacyEventRecords[forId];
        else
            return null;
    }


    public string GetPastLegacyEventRecord(LegacyEventRecordId forId)
    {
        if (_pastLegacyEventRecords.ContainsKey(forId))
            return _pastLegacyEventRecords[forId];
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

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Profession { get ; set; }


    public string EndingTriggeredId
    {
        get { return _endingTriggeredId; }
    }




    public Dictionary<LegacyEventRecordId, string> GetAllFutureLegacyEventRecords()
    {
        return new Dictionary<LegacyEventRecordId, string>(_futureLegacyEventRecords);
    }

    public Dictionary<LegacyEventRecordId, string> GetAllPastLegacyEventRecords()
    {
        return new Dictionary<LegacyEventRecordId,string>(_pastLegacyEventRecords);
    }

    
}

