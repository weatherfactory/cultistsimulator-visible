using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using OrbCreationExtensions;
using UnityEditor;
using UnityEngine.Assertions;


public class ContentRepository : Singleton<ContentRepository>
{

    private const string CONST_CONTENTDIR = "content/";
    private const string CONST_ELEMENTS = "elements";
    private const string CONST_RECIPES = "recipes";
    private const string CONST_ID = "id";
    private const string CONST_LABEL = "label";
    private const string CONST_DESCRIPTION = "description";

    private Hashtable htElements;
    private ArrayList RecipesArrayList;
    public RecipeCompendium RecipeCompendium;

    public void ImportElements()
    {
        string json = Resources.Load<TextAsset>(CONST_CONTENTDIR + CONST_ELEMENTS).text;
        htElements = SimpleJsonImporter.Import(json);
    }

    public void ImportRecipes()
    {
        string json = Resources.Load<TextAsset>(CONST_CONTENTDIR + CONST_RECIPES).text;
        RecipesArrayList = SimpleJsonImporter.Import(json).GetArrayList("recipes");
        RecipeCompendium = PopulateRecipeCompendium(RecipesArrayList);
    }

    public RecipeCompendium PopulateRecipeCompendium(ArrayList importedRecipes)
    {
        List<Recipe> recipesList = new List<Recipe>();
        for(int i=0; i< importedRecipes.Count;i++)
        {
            Hashtable htEach = importedRecipes.GetHashtable(i);
        
                Recipe r = new Recipe();
            try
            {
                r.Id = htEach["id"].ToString();
                r.Label = htEach["label"].ToString();
                r.Craftable = Convert.ToBoolean(htEach["craftable"]);
                r.ActionId = htEach["actionId"].ToString();
                r.StartDescription = htEach["startdescription"].ToString();
                r.Description = htEach["description"].ToString();
                r.Warmup = Convert.ToInt32(htEach["warmup"]);
            }
            catch (Exception e)
            {
                if (htEach["id"] == null)
                    Debug.Log("Problem importing recipe with unknown id - " + e.Message);
                else
                    Debug.Log("Problem importing recipe '" + htEach["id"] + "' - " + e.Message);
            }

            Hashtable htReqs = htEach.GetHashtable("requirements");
            foreach (string k in htReqs.Keys)
            {
                r.Requirements.Add(k,Convert.ToInt32(htReqs[k]));
            }

            Hashtable htEffects = htEach.GetHashtable("effects");
            foreach (string k in htEffects.Keys)
            {
                r.Effects.Add(k,Convert.ToInt32(htEffects[k]));
            }

               recipesList.Add(r);
           
        }

        return new RecipeCompendium(recipesList);
    }

    public Boolean IsKnownElement(string elementId)
    {
        Assert.IsNotNull(htElements, "Elements were never imported; IsKnownElement failed");

        if (htElements==null)
            throw new ApplicationException("Elements were never imported; IsKnownElement failed");
        bool y= htElements.GetNodeWithProperty(CONST_ID, elementId) != null;
        return y;

    }

    public Element PopulateElementForId(string elementId)
    {
        Assert.IsNotNull(htElements, "Elements were never imported; PopulateElementForId failed");
        if (htElements == null)
            throw new ApplicationException("Elements were never imported; PopulateElementForId failed");

        if (!IsKnownElement(elementId))
            return null;

        Hashtable htElement = htElements.GetNodeWithProperty(CONST_ID, elementId);
        Hashtable htAspects = htElement.GetHashtable("aspects");
        Hashtable htSlots = htElement.GetHashtable("slots");

        Element element = new Element(elementId,
           htElement.GetString(CONST_LABEL),
            htElement.GetString(CONST_DESCRIPTION));

        element.AddAspectsFromHashtable(htAspects);
        element.AddSlotsFromHashtable(htSlots);

        return element;
    }
}
