using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using JetBrains.Annotations;

public class Character:IGameEntityStorage
{
    private string _name="[unnamed]";
    
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


    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Profession { get; set; }


    public string EndingTriggeredId
    {
        get { return _endingTriggeredId; }
    }


}

