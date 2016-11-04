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

        public List<ChildSlotSpecification> ChildSlotSpecifications;

    public string Label;
    public int Warmup;
    /// <summary>
    ///the state of the situation: complete, extinct, ongoing
    /// </summary>
    public RecipeTimerState State;
    /// <summary>
    ///seconds remaining on the situation timer
    /// </summary>
    public float TimeRemaining;
    public string OriginalActionId;
    public string Message;


    }

