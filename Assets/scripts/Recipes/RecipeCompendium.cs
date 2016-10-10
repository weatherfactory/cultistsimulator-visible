using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrbCreationExtensions;


public class RecipeCompendium
{
    private List<Recipe> recipes;

    public RecipeCompendium(List<Recipe> allRecipes )
    {
        recipes = allRecipes;
    }

    public Recipe GetFirstRecipeForAspects(Dictionary<string, int> Aspects)
    {
        return null;
    }

    public List<Recipe> GetAllRecipesAsList()
    {
        return recipes;
    }

}
public class Recipe
{
    public string Id { get; set; }
    public Dictionary<string,int> Requirements { get; set; }

    public Recipe()
    {
        Requirements=new Dictionary<string, int>();
    }
 
}