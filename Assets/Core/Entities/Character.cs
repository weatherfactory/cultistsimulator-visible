using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using JetBrains.Annotations;
using Noon;

public class Character:IGameEntityStorage
{
    private string _name="[unnamed]";
    private string _previouscharactername = "[UNKNOWN]";
    
    public CharacterState State { get; set; }
    public List<IDeckInstance> DeckInstances { get; set; }

    private Dictionary<string, int> recipeExecutions;
    private string _endingTriggeredId = null;

    public Character()
    {
        State = CharacterState.Viable;
        recipeExecutions=new Dictionary<string, int>();
        DeckInstances=new List<IDeckInstance>();
    }

    public void AddExecutionToHistory(string forRecipeId)
    {
        if (recipeExecutions.ContainsKey(forRecipeId))
            recipeExecutions[forRecipeId]++;
        else
            recipeExecutions[forRecipeId] = 1;
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

}

