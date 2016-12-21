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
        public Dictionary<string,int> Effects { get; set; }
    ///specifies aspects which may import ingredients into the recipe situation. If any aspects are specified, then any ingredients matching
    /// *any* of these aspects (it's a greedy filter) will go into the recipesituation. (NB 'ingredients' means 'consumed')
        public Dictionary<string,int> PersistsIngredientsWith { get; set; }
    public Dictionary<string, int> RetrievesContentsWith { get; set; }
    public Boolean Craftable { get; set; }
        public string Label { get; set; }
        public int Warmup { get; set; }
    /// <summary>
    /// displayed when we identify and when we are running a recipe
    /// </summary>
        public string StartDescription { get; set; }
    /// <summary>
    /// often empty string; displayed as an aside/commentary wheile a recipe is running
    /// </summary>
        public string Aside { get; set; }
    /// <summary>
    /// displayed in the results when the recipe is complete
    /// </summary>
        public string Description { get; set; }
        public List<RecipeAlternative> AlternativeRecipes { get; set; }
        public string Loop { get; set; }
        public string Ending { get; set; }
    public List<SlotSpecification> SlotSpecifications { get; set; }

    //recipe to execute next; may be the loop recipe; this is null if no loop has been set

    public Recipe()
        {
            Requirements = new Dictionary<string, int>();
        Effects=new Dictionary<string, int>();
        AlternativeRecipes=new List<RecipeAlternative>();
        PersistsIngredientsWith=new Dictionary<string, int>();
        RetrievesContentsWith=new Dictionary<string, int>();
        SlotSpecifications=new List<SlotSpecification>();
        }


        public bool PersistsIngredients()
        {
            return PersistsIngredientsWith.Count > 0;
        }

    public bool RequirementsSatisfiedBy(IAspectsDictionary aspects)
    {
        //must be satisfied by concrete elements in possession, not by aspects (tho this may some day change)
        foreach (var req in Requirements)
        {
            if (req.Value == -1) //req -1 means there must be none of the element
            {
                if (aspects.AspectValue(req.Key)> 0)
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

    public bool RetrievesContents()
    {
        return RetrievesContentsWith.Count > 0;
    }
    }



public class RecipeAlternative
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


    public RecipeAlternative(string id, int chance, bool additional)
    {
        _additional = additional;
        _id = id;
        _chance = chance;
    }
}

