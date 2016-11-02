using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noon;


public class SituationInfo
    {
    /// <summary>
    /// Elements currently inside the situation, to be displayed
    /// </summary>
   public Dictionary<string,int> ElementsInSituation=new Dictionary<string, int>();
    /// <summary>
    ///the recipe currently driving the situation
    /// </summary>
    public Recipe CurrentRecipe;
    /// <summary>
    ///the state of the situation: complete, extinct, ongoing
    /// </summary>
    public RecipeTimerState State;
    /// <summary>
    ///seconds remaining on the situation timer
    /// </summary>
    public float TimeRemaining;
    /// <summary>
    ///any elements which have been taken from an internal container and should be applied elsewhere (eg to the character)
    /// </summary>
    public Dictionary<string,int> RetrievedContents=new Dictionary<string, int>();

    }

