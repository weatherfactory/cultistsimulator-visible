using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using JetBrains.Annotations;
using OrbCreationExtensions;
using UnityEditor;
using UnityEngine.Assertions;

public class ContentRepository : Singleton<ContentRepository>
{

    private const string CONST_CONTENTDIR = "content/";
    private const string CONST_ELEMENTS = "elements";
    private const string CONST_RECIPES = "recipes/recipes";
    private const string CONST_VERBS = "verbs";
    private const string CONST_ID = "id";
    private const string CONST_LABEL = "label";
    private const string CONST_DESCRIPTION = "description";

    private ArrayList recipesArrayList=new ArrayList();
    private ArrayList verbsArrayList=new ArrayList();
    private Dictionary<string, Element> elements=new Dictionary<string, Element>();
    public RecipeCompendium RecipeCompendium;

    public void ImportElements()
    {
        TextAsset[] elementTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_ELEMENTS);
        foreach (TextAsset ta in elementTextAssets)
        {
            string json = ta.text;
            Hashtable htElements = new Hashtable();
            htElements = SimpleJsonImporter.Import(json);
            PopulateElements(htElements);
        }

 
    }

    public void PopulateElements(Hashtable htElements)
    {
        Assert.IsNotNull(htElements, "Elements were never imported; PopulateElementForId failed");
        if (htElements == null)
            throw new ApplicationException("Elements were never imported; PopulateElementForId failed");

        ArrayList alElements = htElements.GetArrayList("elements");
        foreach (Hashtable htElement in alElements)
        {
            Hashtable htAspects = htElement.GetHashtable("aspects");
            Hashtable htSlots = htElement.GetHashtable("slots");

            Element element = new Element(htElement.GetString(CONST_ID),
               htElement.GetString(CONST_LABEL),
                htElement.GetString(CONST_DESCRIPTION));

            element.AddAspectsFromHashtable(htAspects);
            element.AddSlotsFromHashtable(htSlots);

            elements.Add(element.Id,element);
        }
       
    }



    public void ImportRecipes()
    {
TextAsset[] recipeTextAssets=Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_RECIPES);
        foreach (TextAsset ta in recipeTextAssets)
        {
            string json = ta.text;
            recipesArrayList = SimpleJsonImporter.Import(json).GetArrayList("recipes");
            RecipeCompendium = PopulateRecipeCompendium(recipesArrayList);
        }

    }

    public void ImportVerbs()
    {
        TextAsset[] verbTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_VERBS);
        foreach (TextAsset ta in verbTextAssets)
        {
            string json = ta.text;
            verbsArrayList.AddRange(SimpleJsonImporter.Import(json).GetArrayList("verbs"));
        }

    }

    public List<Verb> GetAllVerbs()
    {
        List<Verb> verbs = new List<Verb>();

        foreach (Hashtable h in verbsArrayList)
        {
            Verb v=new Verb(h["id"].ToString(),h["label"].ToString(),h["description"].ToString());
            verbs.Add(v);
        }
        return verbs;
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
        return elements.ContainsKey(elementId);

    }

    public Element GetElementById(string elementId)
    {
        return elements[elementId];

    }

    public Sprite GetSpriteForVerb(string verbId)
    {
        return Resources.Load<Sprite>("icons40/verbs/" + verbId);
    }

    public Sprite GetSpriteForElement(string elementId)
    {
        return Resources.Load<Sprite>("FlatIcons/png/32px/" + elementId);
    }
}
