using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using JetBrains.Annotations;
using Noon;
using OrbCreationExtensions;
using UnityEditor;
using UnityEngine.Assertions;

public class ContentRepository : Singleton<ContentRepository>
{

    private const string CONST_CONTENTDIR = "content/";
    private const string CONST_ELEMENTS = "elements";
    private const string CONST_RECIPES = "recipes";
    private const string CONST_VERBS = "verbs";
    private const string CONST_ID = "id";
    private const string CONST_LABEL = "label";
    private const string CONST_DESCRIPTION = "description";


    private Dictionary<string,Verb> verbs=new Dictionary<string, Verb>();
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
        ArrayList recipesArrayList = new ArrayList();

        foreach (TextAsset ta in recipeTextAssets)
        {
            string json = ta.text;
            recipesArrayList.AddRange(SimpleJsonImporter.Import(json).GetArrayList("recipes"));
            
        }
        RecipeCompendium = PopulateRecipeCompendium(recipesArrayList);
    }

    public void ImportVerbs()
    {
        ArrayList verbsArrayList=new ArrayList();
        TextAsset[] verbTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_VERBS);
        foreach (TextAsset ta in verbTextAssets)
        {
            string json = ta.text;
            verbsArrayList.AddRange(SimpleJsonImporter.Import(json).GetArrayList("verbs"));
        }

        foreach (Hashtable h in verbsArrayList)
        {
            Verb v = new Verb(h["id"].ToString(), h["label"].ToString(), h["description"].ToString());
            verbs.Add(v.Id,v);
        }

    }

    public List<Verb> GetAllVerbs()
    {
        List<Verb> verbsList = new List<Verb>();

        foreach (KeyValuePair<string, Verb> keyValuePair in verbs)
        {
            verbsList.Add(keyValuePair.Value);
        }

        return verbsList;
    }

    public Verb GetVerbById(string verbId)
    {
        if(!verbs.ContainsKey(verbId))
            throw new ApplicationException("Couldn't find verb id " + verbId);
        
        return verbs[verbId];
    }

    public RecipeCompendium PopulateRecipeCompendium(ArrayList importedRecipes)
    {
        List<Recipe> recipesList = new List<Recipe>();
        for(int i=0; i< importedRecipes.Count;i++)
        {
            Hashtable htEachRecipe = importedRecipes.GetHashtable(i);
        
                Recipe r = new Recipe();
            try
            {
                r.Id = htEachRecipe[Constants.KID].ToString();
                r.Label = htEachRecipe[Constants.KLABEL].ToString();
                r.Craftable = Convert.ToBoolean(htEachRecipe[Constants.KCRAFTABLE]);
                r.ActionId = htEachRecipe[Constants.KACTIONID].ToString();
                r.StartDescription = htEachRecipe[Constants.KSTARTDESCRIPTION].ToString();
                r.Description = htEachRecipe[Constants.KDESCRIPTION].ToString();
                r.Warmup = Convert.ToInt32(htEachRecipe[Constants.KWARMUP]);
                if (htEachRecipe[Constants.KLOOP] == null)
                    r.Loop = null;
                else
                    r.Loop = htEachRecipe[Constants.KLOOP].ToString();
            }
            catch (Exception e)
            {
                if (htEachRecipe[Constants.KID] == null)
                    Debug.Log("Problem importing recipe with unknown id - " + e.Message);
                else
                    Debug.Log("Problem importing recipe '" + htEachRecipe[Constants.KID] + "' - " + e.Message);
            }

            Hashtable htReqs = htEachRecipe.GetHashtable(Constants.KREQUIREMENTS);
            foreach (string k in htReqs.Keys)
            {
                r.Requirements.Add(k,Convert.ToInt32(htReqs[k]));
            }

            Hashtable htEffects = htEachRecipe.GetHashtable(Constants.KEFFECTS);
            foreach (string k in htEffects.Keys)
            {
                r.Effects.Add(k,Convert.ToInt32(htEffects[k]));
            }

            ArrayList alRecipeAlternatives = htEachRecipe.GetArrayList(Constants.KALTERNATIVERECIPES);
            if(alRecipeAlternatives!=null)
            { 
                foreach (Hashtable ra in alRecipeAlternatives)
                {
                    string raID = ra[Constants.KID].ToString();
                    int raChance = Convert.ToInt32(ra[Constants.KCHANCE]);
                    bool raAdditional = Convert.ToBoolean(ra[Constants.KADDITIONAL] ?? false);

                    r.AlternativeRecipes.Add(new RecipeAlternative(raID,raChance,raAdditional));
                }
            }

            recipesList.Add(r);
           
        }

        return new RecipeCompendium(recipesList,new Dice());
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
