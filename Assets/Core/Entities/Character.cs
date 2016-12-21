using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

public class Character
{
    private string _title;
    private string _firstName;
    private string _lastName;
    public CharacterState State { get; set; }
    private Dictionary<string, int> recipeExecutions;
    private string _endingTriggeredId = null;

    public Character()
    {
        State = CharacterState.Viable;
        recipeExecutions=new Dictionary<string, int>();
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

    public string Title
    {
        get { return _title; }
        set { _title = value; }
    }

    public string FirstName
    {
        get { return _firstName; }
        set { _firstName = value; }
    }

    public string LastName
    {
        get { return _lastName; }
        set { _lastName = value; }
    }

    public string EndingTriggeredId
    {
        get { return _endingTriggeredId; }
    }



}

