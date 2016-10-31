using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
    public class Recipe
    {
        public string Id { get; set; }
        public string ActionId { get; set; }
        public Dictionary<string, int> Requirements { get; set; }
        public Dictionary<string,int> Effects { get; set; }
    public Dictionary<string,int> PersistedIngredients { get; set; }
        public Boolean Craftable { get; set; }
        public string Label { get; set; }
        public int Warmup { get; set; }
        public string StartDescription { get; set; }
        public string Description { get; set; }
        public List<RecipeAlternative> AlternativeRecipes { get; set; }
        public string Loop { get; set; }
        public string Ending { get; set; }

//recipe to execute next; may be the loop recipe; this is null if no loop has been set

        public Recipe()
        {
            Requirements = new Dictionary<string, int>();
        Effects=new Dictionary<string, int>();
        AlternativeRecipes=new List<RecipeAlternative>();
        PersistedIngredients=new Dictionary<string, int>();
        }

        public void Do(IElementsContainer container)
        {
        foreach(var e in Effects)
            container.ModifyElementQuantity(e.Key,e.Value);
        if(Ending!=null)
            container.TriggerSpecialEvent(Ending);
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

