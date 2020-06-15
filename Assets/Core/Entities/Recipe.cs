using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

public class RequirementValue
{

}

public enum PortalEffect
{
    None=0,
    Wood=10,
    WhiteDoor=20,
    StagDoor=30,
    SpiderDoor=40,
    PeacockDoor=50,
    TricuspidGate=60
}

/// <summary>
/// This is mostly a bundle of properties, but the Do method is core logic! - it's where element countss are actually changed
/// </summary>
[Serializable]
public class Recipe: IEntity
{
    private string _description="";
    private string _startDescription="";
    private string _label="";
    public string Id { get; set; }
    public string ActionId { get; set; }
    public Dictionary<string, string> Requirements { get; set; }
    public Dictionary<string, string> TableReqs { get; set; }
    public Dictionary<string, string> ExtantReqs { get; set; }
    public Dictionary<string, string> Effects { get; set; }
    public AspectsDictionary Aspects { get; set; } 
    public List<MutationEffect> MutationEffects { get; set; }
    /// <summary>
    /// Elements that should be purged from the board (including currently dragged card if any). Int value is max number elements to be purged. (Later might also purge from slots and even maybe situations.
    /// </summary>)
    public Dictionary<string,int> Purge { get; set; }
    public Dictionary<string, int> HaltVerb { get; set; }
    public Dictionary<string, int> DeleteVerb { get; set; }


    public EndingFlavour SignalEndingFlavour { get; set; } //separate from Ending, because some recipes may be menacing but route to another recipe that actually does the ending

    public Boolean Craftable { get; set; }
    /// <summary>
    /// If HintOnly is true and Craftable is false, the recipe will display as a hint, but *only if no craftable recipes are available*
    /// </summary>
    public Boolean HintOnly { get; set; }

    public string Label
    {
        get { return _label; }
        set
        {
            _label = value ?? "";
        }
    }

    public int Warmup { get; set; }

    /// <summary>
    /// displayed when we identify and when we are running a recipe; also appended to the Aside if predicted as an additional recipe
    /// </summary>
    public string StartDescription
    {
        get { return _startDescription; }
        set { _startDescription = value ?? ""; }
    }

    /// <summary>
    /// On completion, the recipe will draw
    ///from this deck and add the result to the outcome.
    /// </summary>
    public Dictionary<string, int> DeckEffects { get; set; }

    /// <summary>
    /// displayed in the results when the recipe is complete
    /// </summary>
    public string Description
    {
        get { return _description; }
        set { _description = value ?? ""; }
    }

    public List<LinkedRecipeDetails> AlternativeRecipes { get; set; }
    public List<LinkedRecipeDetails> LinkedRecipes { get; set; }
    public string EndingFlag { get; set; }


    
    /// <summary>
    /// 0 means any number of executions; otherwise, this recipe may only be executed this many times by a given character.
    /// </summary>
    public int MaxExecutions { get; set; }
    public string BurnImage { get; set; }

    public bool HasInfiniteExecutions()
    {
        return MaxExecutions == 0;
    }

    public PortalEffect PortalEffect { get; set; }

    public List<SlotSpecification> SlotSpecifications { get; set; }

    //recipe to execute next; may be the loop recipe; this is null if no loop has been set

    public Recipe()
    {
        Requirements = new Dictionary<string, string>();
        TableReqs = new Dictionary<string, string>();
        ExtantReqs = new Dictionary<string, string>();
        Effects = new Dictionary<string, string>();
        AlternativeRecipes = new List<LinkedRecipeDetails>();
        LinkedRecipes=new List<LinkedRecipeDetails>();
        SlotSpecifications = new List<SlotSpecification>();
        Aspects=new AspectsDictionary();
        DeckEffects=new Dictionary<string,int>();
        Purge=new Dictionary<string, int>();
        HaltVerb = new Dictionary<string, int>();
        DeleteVerb = new Dictionary<string, int>();
        MutationEffects = new List<MutationEffect>();
        PortalEffect = PortalEffect.None;
    }



    public bool RequirementsSatisfiedBy(AspectsInContext aspectsinContext)
    {
        foreach (var req in Requirements)
        {
            if (!CheckRequirementsSatisfiedForContext(aspectsinContext.AspectsInSituation, req)) return false;
        }

        foreach (var treq in TableReqs)
        {
            if (!CheckRequirementsSatisfiedForContext(aspectsinContext.AspectsOnTable, treq)) return false;

        }

        foreach (var ereq in ExtantReqs)
        {
            if (!CheckRequirementsSatisfiedForContext(aspectsinContext.AspectsExtant, ereq)) return false;
        }

        return true;
    }

    private static bool CheckRequirementsSatisfiedForContext(IAspectsDictionary aspectsToCheck, KeyValuePair<string, string> req)
    {
        if (!int.TryParse(req.Value, out var reqValue))
        {
            //the value is not an int: it must be a reference to another aspect
            reqValue = aspectsToCheck.AspectValue(req.Value);
        }

        {
            if (reqValue <= -1) //this is a No More Than requirement
            {
                if (aspectsToCheck.AspectValue(req.Key) >= -reqValue)
                    return false;
            }
            else if (!(aspectsToCheck.AspectValue(req.Key) >= reqValue))
            {
                //req >0 means there must be >=req of the element
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// do something grander like a bong when we loop this recipe
    /// </summary>
    public bool SignalImportantLoop { get; set; }

}

public class Expulsion
{
 [FucineAspectsDictionary]
    public AspectsDictionary Filter { get; set; }
    [FucineValue(1)]
    public int Limit { get; set; }

    public Expulsion()
    {
        Limit = 0;
        Filter=new AspectsDictionary();
    }
}

public class LinkedRecipeDetails:IEntity
{
    private Expulsion _expulsion;

    [FucineId]
    public string Id { get; set; }

    [FucineValue(0)]
    public int Chance { get; set; }

    [FucineValue(false)]
    public bool Additional { get; set; }


    [FucineDictStringString]
    public Dictionary<string, string> Challenges { get; set; }

    [FucineSubEntity(typeof(Expulsion))]
    public Expulsion Expulsion
    {
        get
        {
            return _expulsion;
        }

        set
        {
            _expulsion = value;
        }
    }

    public LinkedRecipeDetails()
    {


    }


    public LinkedRecipeDetails(string id, int chance, bool additional,Expulsion expulsion, Dictionary<string, string> challenges)
    {
        Additional = additional;
        Id = id;
        Chance = chance;
        Expulsion = expulsion;
        Challenges = challenges ?? new Dictionary<string, string>();

    }

  
}

