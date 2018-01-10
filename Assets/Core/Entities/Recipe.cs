using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;

/// <summary>
/// This is mostly a bundle of properties, but the Do method is core logic! - it's where element countss are actually changed
/// </summary>
[Serializable]
public class Recipe
{
    public string Id { get; set; }
    public string ActionId { get; set; }
    public Dictionary<string, int> Requirements { get; set; }
    public Dictionary<string, int> Effects { get; set; }
    public AspectsDictionary Aspects { get; set; }

    public Boolean Craftable { get; set; }
    public Boolean HintOnly { get; set; }
    public string Label { get; set; }
    public int Warmup { get; set; }

    /// <summary>
    /// displayed when we identify and when we are running a recipe; also appended to the Aside if predicted as an additional recipe
    /// </summary>
    public string StartDescription { get; set; }

    /// <summary>
    /// often empty string; displayed as an aside/commentary wheile a recipe is running. NOTE: currently has no effect - it's been exiled by the situation update
    /// </summary>
    public string Aside { get; set; }
    /// <summary>
    /// On completion, the recipe will draw
    ///from this deck and add the result to the outcome.
    /// </summary>
    public string DeckEffect { get; set; }

    /// <summary>
    /// displayed in the results when the recipe is complete
    /// </summary>
    public string Description { get; set; }

    public List<LinkedRecipeDetails> AlternativeRecipes { get; set; }
    public List<LinkedRecipeDetails> LinkedRecipes { get; set; }
    public string EndingFlag { get; set; }

    public bool EndsGame()
    {
        return EndingFlag != null;
    }
    /// <summary>
    /// 0 means any number of executions; otherwise, this recipe may only be executed this many times by a given character.
    /// </summary>
    public int MaxExecutions { get; set; }
    public string BurnImage { get; set; }

    public bool HasInfiniteExecutions()
    {
        return MaxExecutions == 0;
    }

    public List<SlotSpecification> SlotSpecifications { get; set; }

    //recipe to execute next; may be the loop recipe; this is null if no loop has been set

    public Recipe()
    {
        Requirements = new Dictionary<string, int>();
        Effects = new Dictionary<string, int>();
        AlternativeRecipes = new List<LinkedRecipeDetails>();
        LinkedRecipes=new List<LinkedRecipeDetails>();
        SlotSpecifications = new List<SlotSpecification>();
        Aspects=new AspectsDictionary();
    }



    public bool RequirementsSatisfiedBy(IAspectsDictionary aspects)
    {
        //must be satisfied by concrete elements in possession, not by aspects (tho this may some day change)
        foreach (var req in Requirements)
        {
            if (req.Value == -1) //req -1 means there must be none of the element
            {
                if (aspects.AspectValue(req.Key) > 0)
                    return false;
            }
            else if (!(aspects.AspectValue(req.Key) >= req.Value))
            {
                //req >0 means there must be >=req of the element
                return false;
            }
        }
        return true;
    }


}


public class LinkedRecipeDetails
{
    private readonly bool _additional;
    private readonly string _id;
    private readonly int _chance;

    public string Id
    {
        get { return _id; }
    }

    public int Chance
    {
        get { return _chance; }
    }

    public bool Additional
    {
        get { return _additional; }
    }


    public LinkedRecipeDetails(string id, int chance, bool additional)
    {
        _additional = additional;
        _id = id;
        _chance = chance;
    }
}

