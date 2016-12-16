using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Noon;
using OrbCreationExtensions;
using UnityEngine.Assertions;

public class ContentImporter
{
    private IList<ContentImportProblem> contentImportProblems;
    private const string CONST_CONTENTDIR = "content/";
    private const string CONST_ELEMENTS = "elements";
    private const string CONST_RECIPES = "recipes";
    private const string CONST_VERBS = "verbs";
    private const string CONST_ID = "id";
    private const string CONST_LABEL = "label";
    private const string CONST_DESCRIPTION = "description";
    public ICompendium _compendium { get; private set; }


    public Dictionary<string, IVerb> Verbs;
    public Dictionary<string, Element> Elements;
    public List<Recipe> Recipes;
    

    public ContentImporter()
    {
   contentImportProblems=new List<ContentImportProblem>();
   Verbs = new Dictionary<string, IVerb>();
   Elements = new Dictionary<string, Element>();
   Recipes = new List<Recipe>();
    }

    public IList<ContentImportProblem> GetContentImportProblems()
    {
        return contentImportProblems;
    }

    private void LogProblem(string problemDesc)
    {
        contentImportProblems.Add(new ContentImportProblem(problemDesc));
    }

    public List<SlotSpecification> AddSlotsFromHashtable(Hashtable htSlots)
    {
        List<SlotSpecification> cssList=new List<SlotSpecification>();

        if (htSlots != null)
        {

            foreach (string k in htSlots.Keys)
            {

                Hashtable htThisSlot = htSlots[k] as Hashtable;

                SlotSpecification slotSpecification=new SlotSpecification(k);

                if ((string) htThisSlot[NoonConstants.KGREEDY] == "true")
                    slotSpecification.Greedy = true;

                if ((string) htThisSlot[NoonConstants.KCONSUMES] == "true")
                    slotSpecification.Consumes = true;

                Hashtable htRequired = htThisSlot["required"] as Hashtable;
                if (htRequired != null)
                {
                    foreach (string rk in htRequired.Keys)
                        slotSpecification.Required.Add(rk, 1);
                }
                Hashtable htForbidden = htThisSlot["forbidden"] as Hashtable;
                if (htForbidden != null)
                {
                    foreach (string fk in htForbidden.Keys)
                        slotSpecification.Forbidden.Add(fk, 1);
                }

                cssList.Add(slotSpecification);
            }
        }

        return cssList;

    }

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
        
        if (htElements == null)
            LogProblem("Elements were never imported; PopulateElementForId failed");

        ArrayList alElements = htElements.GetArrayList("elements");
        foreach (Hashtable htElement in alElements)
        {
            Hashtable htAspects = htElement.GetHashtable("aspects");
            Hashtable htSlots = htElement.GetHashtable(NoonConstants.KSLOTS);

            Element element = new Element(htElement.GetString(CONST_ID),
               htElement.GetString(CONST_LABEL),
                htElement.GetString(CONST_DESCRIPTION));

            element.Aspects = NoonUtility.ReplaceConventionValues(htAspects);
            element.ChildSlotSpecifications=AddSlotsFromHashtable(htSlots);

            Elements.Add(element.Id,element);
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
        PopulateRecipeList(recipesArrayList);
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
            IVerb v = new BasicVerb(h["id"].ToString(), h["label"].ToString(), h["description"].ToString());
            Verbs.Add(v.Id,v);
        }

    }


    public void PopulateRecipeList(ArrayList importedRecipes)
    {
        for(int i=0; i< importedRecipes.Count;i++)
        {
            Hashtable htEachRecipe = importedRecipes.GetHashtable(i);
        
                Recipe r = new Recipe();
            try
            {
                r.Id = htEachRecipe[NoonConstants.KID].ToString();
                r.Label = htEachRecipe[NoonConstants.KLABEL].ToString();
                r.Craftable = Convert.ToBoolean(htEachRecipe[NoonConstants.KCRAFTABLE]);
                r.ActionId = htEachRecipe[NoonConstants.KACTIONID].ToString();
                r.StartDescription = htEachRecipe[NoonConstants.KSTARTDESCRIPTION].ToString();
                r.Description = htEachRecipe[NoonConstants.KDESCRIPTION].ToString();
                r.Warmup = Convert.ToInt32(htEachRecipe[NoonConstants.KWARMUP]);
                r.Loop = htEachRecipe[NoonConstants.KLOOP] == null ? null : htEachRecipe[NoonConstants.KLOOP].ToString();
                r.Ending = htEachRecipe[NoonConstants.KENDING] == null ? null : htEachRecipe[NoonConstants.KENDING].ToString();

            }
            catch (Exception e)
            {
                if (htEachRecipe[NoonConstants.KID] == null)

                    LogProblem("Problem importing recipe with unknown id - " + e.Message);
                else
                    LogProblem("Problem importing recipe '" + htEachRecipe[NoonConstants.KID] + "' - " + e.Message);
            }

            Hashtable htReqs = htEachRecipe.GetHashtable(NoonConstants.KREQUIREMENTS);
            foreach (string k in htReqs.Keys)
            {
                LogNonexistentElementId(k, r.Id, "(requirements)");
                r.Requirements.Add(k,Convert.ToInt32(htReqs[k]));
            }

            Hashtable htEffects = htEachRecipe.GetHashtable(NoonConstants.KEFFECTS);
            if(htEffects==null)
                LogProblem("No effects found for recipe " + r.Id );
            foreach (string k in htEffects.Keys)
            {
                LogNonexistentElementId(k, r.Id, "(effects)");
                r.Effects.Add(k,Convert.ToInt32(htEffects[k]));
            }

            Hashtable htPersistIngredients = htEachRecipe.GetHashtable(NoonConstants.KPERSISTINGREDIENTSWITH);
            if(htPersistIngredients!=null)
            foreach (string k in htPersistIngredients.Keys)
            {
                r.PersistsIngredientsWith.Add(k, Convert.ToInt32(htPersistIngredients[k]));
            }

            Hashtable htRetrievesContentsWith = htEachRecipe.GetHashtable(NoonConstants.KRETRIEVESCONTENTSWITH);
            if (htRetrievesContentsWith != null)
                foreach (string k in htRetrievesContentsWith.Keys)
                {
                    r.RetrievesContentsWith.Add(k, Convert.ToInt32(htRetrievesContentsWith[k]));
                }

            Hashtable htSlots = htEachRecipe.GetHashtable(NoonConstants.KSLOTS);
            r.SlotSpecifications = AddSlotsFromHashtable(htSlots);

            ArrayList alRecipeAlternatives = htEachRecipe.GetArrayList(NoonConstants.KALTERNATIVERECIPES);
            if(alRecipeAlternatives!=null)
            { 
                foreach (Hashtable ra in alRecipeAlternatives)
                {
                    string raID = ra[NoonConstants.KID].ToString();
                    int raChance = Convert.ToInt32(ra[NoonConstants.KCHANCE]);
                    bool raAdditional = Convert.ToBoolean(ra[NoonConstants.KADDITIONAL] ?? false);

                    r.AlternativeRecipes.Add(new RecipeAlternative(raID,raChance,raAdditional));
                }
            }


            Recipes.Add(r);
        }


    }

    private void LogNonexistentElementId(string elementId, string recipeId, string context)
    {
        if(!Elements.ContainsKey(elementId))
        LogProblem("'" + recipeId + "' references non-existent element '" + elementId + "' " + " " + context);
    }

    public void PopulateCompendium(ICompendium compendium)
    {
        _compendium = compendium;
        ImportVerbs();
        ImportElements();
        ImportRecipes();

        _compendium.UpdateRecipes(Recipes);
        _compendium.UpdateElements(Elements);
        _compendium.UpdateVerbs(Verbs);

    }

}
