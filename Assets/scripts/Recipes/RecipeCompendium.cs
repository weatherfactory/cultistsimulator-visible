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

    public List<Recipe> GetAllRecipesAsList()
    {
        return recipes;
    }

}
public class Recipe
{
    public string Id { get; set; }

   
}