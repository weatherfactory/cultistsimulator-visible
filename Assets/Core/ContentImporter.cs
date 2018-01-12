using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private const string CONST_DECKS = "decks";
    private const string CONST_LEGACIES = "legacies";
    private const string CONST_ID = "id";
    private const string CONST_LABEL = "label";
    private const string CONST_LIFETIME = "lifetime";
    private const string CONST_DESCRIPTION = "description";
    private const string CONST_ANIMFRAMES = "animFrames";
    private const string CONST_ISASPECT = "isAspect";
    private const string CONST_UNIQUE = "unique";
    public ICompendium _compendium { get; private set; }


    public Dictionary<string, IVerb> Verbs;
    public Dictionary<string, Element> Elements;
    public Dictionary<string, Legacy> Legacies;
    public List<Recipe> Recipes;
    private Dictionary<string, IDeckSpec> DeckSpecs;


    public ContentImporter()
    {
        contentImportProblems = new List<ContentImportProblem>();
        Verbs = new Dictionary<string, IVerb>();
        Elements = new Dictionary<string, Element>();
        Recipes = new List<Recipe>();
        DeckSpecs = new Dictionary<string, IDeckSpec>();
        Legacies = new Dictionary<string, Legacy>();
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
        List<SlotSpecification> cssList = new List<SlotSpecification>();


        if (htSlots != null)
        {
            foreach (string k in htSlots.Keys)
            {

                Hashtable htThisSlot = htSlots[k] as Hashtable;

                SlotSpecification slotSpecification = new SlotSpecification(k);
                try
                {
                    if (htThisSlot[NoonConstants.KDESCRIPTION] != null)
                        slotSpecification.Description = htThisSlot[NoonConstants.KDESCRIPTION].ToString();

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
                }
                catch (Exception e)
                {
                    LogProblem("Couldn't retrieve slot " + k + " - " + e.Message);
                }

                cssList.Add(slotSpecification);
            }
        }

        return cssList;

    }

    public void ImportElements()
    {
        TextAsset[] elementTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_ELEMENTS);
        int totalElementsFound = 0;
        foreach (TextAsset ta in elementTextAssets)
        {
            string json = ta.text;
            Hashtable htElements = new Hashtable();
            htElements = SimpleJsonImporter.Import(json);
            totalElementsFound += PopulateElements(htElements);
        }

        NoonUtility.Log("Total elements found: " + totalElementsFound);
    }

    public int PopulateElements(Hashtable htElements)
    {

        if (htElements == null)
            LogProblem("Elements were never imported; PopulateElementForId failed");

        ArrayList alElements = htElements.GetArrayList("elements");

        foreach (Hashtable htElement in alElements)
        {


            Hashtable htAspects = htElement.GetHashtable(NoonConstants.KASPECTS);
            Hashtable htSlots = htElement.GetHashtable(NoonConstants.KSLOTS);
            Hashtable htXTriggers = htElement.GetHashtable(NoonConstants.KXTRIGGERS);


            Element element = new Element(htElement.GetString(CONST_ID),
                htElement.GetString(CONST_LABEL),
                htElement.GetString(CONST_DESCRIPTION),
                htElement.GetInt(CONST_ANIMFRAMES));

            if(element.Label==null)
                LogProblem("No label for element " + element.Id);

            if (element.Description == null)
                LogProblem("No description for element " + element.Id);
            try
            {

                if (htElement.ContainsKey(CONST_LIFETIME))
                    element.Lifetime = float.Parse(htElement[CONST_LIFETIME].ToString());

                if (htElement.GetString(CONST_ISASPECT) == "true")
                    element.IsAspect = true;
                else
                    element.IsAspect = false;

                if (htElement.GetString(CONST_UNIQUE) == "true")
                    element.Unique = true;
                else
                    element.Unique = false;

                element.Aspects = NoonUtility.ReplaceConventionValues(htAspects);
                element.ChildSlotSpecifications = AddSlotsFromHashtable(htSlots);
                if (htXTriggers != null)
                {
                    foreach (string k in htXTriggers.Keys)
                    {
                        //the element we want to transform this element to when the trigger fires
                        var xid = htXTriggers[k].ToString();
                        LogIfNonexistentElementId(k, element.Id, "(aspect that fires an xtrigger)");
                        LogIfNonexistentElementId(k, xid, "(xtrigger x id)");
                        element.XTriggers.Add(k, xid);
                    }
                }

                Elements.Add(element.Id, element);
            }
            catch (Exception e)
            {

                LogProblem("Couldn't add all properties for element " + element.Id + ": " + e.Message);

            }

            try
            {
                ArrayList alInducedRecipes = htElement.GetArrayList(NoonConstants.KINDUCES);
                if (alInducedRecipes != null)
                {
                    foreach (Hashtable lr in alInducedRecipes)
                    {
                        string lrID = lr[NoonConstants.KID].ToString();
                        int lrChance = Convert.ToInt32(lr[NoonConstants.KCHANCE]);
                        bool lrAdditional = Convert.ToBoolean(lr[NoonConstants.KADDITIONAL] ?? false);

                        element.Induces.Add(new LinkedRecipeDetails(lrID, lrChance, lrAdditional));
                    }
                }

            }
            catch (Exception e)
            {

                LogProblem("Problem importing induced recipes for element '" + element.Id + "' - " + e.Message);
            }


        }

        return alElements.Count;
    }

    public void ImportRecipes()
    {
        TextAsset[] recipeTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_RECIPES);
        ArrayList recipesArrayList = new ArrayList();

        foreach (TextAsset ta in recipeTextAssets)
        {
            string json = ta.text;
            try
            {
                recipesArrayList.AddRange(SimpleJsonImporter.Import(json).GetArrayList("recipes"));
            }


            catch (Exception e)
            {
                NoonUtility.Log("This file broke: " + ta.name + " with error " + e.Message);
                throw;
            }

        }

        PopulateRecipeList(recipesArrayList);
        NoonUtility.Log("Total recipes found: " + recipesArrayList.Count);

    }

    public void ImportVerbs()
    {
        ArrayList verbsArrayList = new ArrayList();
        TextAsset[] verbTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_VERBS);
        foreach (TextAsset ta in verbTextAssets)
        {
            string json = ta.text;
            verbsArrayList.AddRange(SimpleJsonImporter.Import(json).GetArrayList("verbs"));
        }

        foreach (Hashtable h in verbsArrayList)
        {
            IVerb v = new BasicVerb(h["id"].ToString(), h["label"].ToString(), h["description"].ToString(),
                Convert.ToBoolean(h["atStart"]));
            Verbs.Add(v.Id, v);
        }

    }

    private void ImportDeckSpecs()
    {
        ArrayList decksArrayList = new ArrayList();
        TextAsset[] deckTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_DECKS);
        foreach (TextAsset ta in deckTextAssets)
        {
            string json = ta.text;
            decksArrayList.AddRange(SimpleJsonImporter.Import(json).GetArrayList(CONST_DECKS));
        }

        for (int i = 0; i < decksArrayList.Count; i++)
        {
            Hashtable htEachDeck = decksArrayList.GetHashtable(i);

            //deckspec
            var thisDeckSpec = new List<string>();
            try
            {
                ArrayList htDeckSpec = htEachDeck.GetArrayList(NoonConstants.KDECKSPEC);
                if (htDeckSpec != null)
                {
                    foreach (string v in htDeckSpec)
                    {
                        LogIfNonexistentElementId(v, htEachDeck[NoonConstants.KID].ToString(), "(deckSpec spec items)");
                        thisDeckSpec.Add(v);
                    }
                }
            }
            catch (Exception e)
            {
                LogProblem("Problem importing deckspec for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                           "' - " + e.Message);
            }


            string defaultCardId = "";
            try
            {
                defaultCardId = htEachDeck.GetValue(NoonConstants.KDECKDEFAULTCARD).ToString();
            }
            catch (Exception e)
            {
                LogProblem("Problem importing default card for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                           "' - " + e.Message);
            }

            bool resetOnExhaustion = false;
            try
            {
                resetOnExhaustion = Convert.ToBoolean(htEachDeck.GetValue(NoonConstants.KRESETONEXHAUSTION));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            DeckSpec d = new DeckSpec(htEachDeck["id"].ToString(), thisDeckSpec, defaultCardId, resetOnExhaustion);

            DeckSpecs.Add(d.Id, d);
        }

    }



    public void ImportLegacies()
    {
        ArrayList legaciesArrayList = new ArrayList();
        TextAsset[] legacyTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_LEGACIES);
        foreach (TextAsset ta in legacyTextAssets)
        {
            string json = ta.text;
            legaciesArrayList.AddRange(SimpleJsonImporter.Import(json).GetArrayList(CONST_LEGACIES));
        }

        for (int i = 0; i < legaciesArrayList.Count; i++)
        {
            Hashtable htEachLegacy = legaciesArrayList.GetHashtable(i);

            Legacy l = new Legacy(htEachLegacy[NoonConstants.KID].ToString(),
                htEachLegacy[NoonConstants.KLABEL].ToString(), htEachLegacy[NoonConstants.KDESCRIPTION].ToString(),
                htEachLegacy[NoonConstants.KSTARTDESCRIPTION].ToString(),
                htEachLegacy[NoonConstants.KIMAGE].ToString());

            Hashtable htEffects = htEachLegacy.GetHashtable(NoonConstants.KEFFECTS);
            if (htEffects != null)
            {
                foreach (string k in htEffects.Keys)
                {
                    LogIfNonexistentElementId(k, l.Id, "(effects)");
                    l.Effects.Add(k, Convert.ToInt32(htEffects[k]));
                }
            }

            Legacies.Add(l.Id, l);
        }


    }

    public void PopulateRecipeList(ArrayList importedRecipes)
    {
        for (int i = 0; i < importedRecipes.Count; i++)
        {
            Hashtable htEachRecipe = importedRecipes.GetHashtable(i);

            Recipe r = new Recipe();
            try
            {
                r.Id = htEachRecipe[NoonConstants.KID].ToString();
                htEachRecipe.Remove(NoonConstants.KID);

                r.Label = htEachRecipe[NoonConstants.KLABEL] == null
                    ? r.Id
                    : htEachRecipe[NoonConstants.KLABEL].ToString();
                htEachRecipe.Remove(NoonConstants.KLABEL);

                r.Craftable = Convert.ToBoolean(htEachRecipe[NoonConstants.KCRAFTABLE]);
                htEachRecipe.Remove(NoonConstants.KCRAFTABLE);

                r.HintOnly = Convert.ToBoolean(htEachRecipe[NoonConstants.KHINTONLY]);
                htEachRecipe.Remove(NoonConstants.KHINTONLY);

                r.ActionId = htEachRecipe[NoonConstants.KACTIONID] == null
                    ? null
                    : htEachRecipe[NoonConstants.KACTIONID].ToString();
                if (r.ActionId == null)
                    LogProblem(r.Id + " has no actionId specified");
                htEachRecipe.Remove(NoonConstants.KACTIONID);

                if (htEachRecipe.ContainsKey(NoonConstants.KSTARTDESCRIPTION))
                    r.StartDescription = htEachRecipe[NoonConstants.KSTARTDESCRIPTION].ToString();
                htEachRecipe.Remove(NoonConstants.KSTARTDESCRIPTION);
                

                if (htEachRecipe.ContainsKey(NoonConstants.KDESCRIPTION))
                    r.Description = htEachRecipe[NoonConstants.KDESCRIPTION].ToString();
                htEachRecipe.Remove(NoonConstants.KDESCRIPTION);

                

                if (htEachRecipe.ContainsKey(NoonConstants.KASIDE))
                    r.Aside = htEachRecipe[NoonConstants.KASIDE].ToString();
                htEachRecipe.Remove(NoonConstants.KASIDE);

                if (htEachRecipe.ContainsKey(NoonConstants.KDECKEFFECT))
                {
                    string deckId = htEachRecipe[NoonConstants.KDECKEFFECT].ToString();
                    LogIfNonexistentDeckId(deckId, r.Id);
                    r.DeckEffect = deckId;
                    htEachRecipe.Remove(NoonConstants.KDECKEFFECT);
                }

                r.Warmup = Convert.ToInt32(htEachRecipe[NoonConstants.KWARMUP]);
                htEachRecipe.Remove(NoonConstants.KWARMUP);


                r.EndingFlag = htEachRecipe[NoonConstants.KENDING] == null
                    ? null
                    : htEachRecipe[NoonConstants.KENDING].ToString();
                htEachRecipe.Remove(NoonConstants.KENDING);

                if (htEachRecipe.ContainsKey(NoonConstants.KMAXEXECUTIONS))
                    r.MaxExecutions = Convert.ToInt32(htEachRecipe[NoonConstants.KMAXEXECUTIONS]);
                htEachRecipe.Remove(NoonConstants.KMAXEXECUTIONS);

                if (htEachRecipe.ContainsKey(NoonConstants.KBURNIMAGE))
                    r.BurnImage = htEachRecipe[NoonConstants.KBURNIMAGE].ToString();
                htEachRecipe.Remove(NoonConstants.KBURNIMAGE);

            }
            catch (Exception e)
            {
                if (htEachRecipe[NoonConstants.KID] == null)

                    LogProblem("Problem importing recipe with unknown id - " + e.Message);
                else
                    LogProblem("Problem importing recipe '" + htEachRecipe[NoonConstants.KID] + "' - " + e.Message);
            }

            //REQUIREMENTS
            try
            {
                Hashtable htReqs = htEachRecipe.GetHashtable(NoonConstants.KREQUIREMENTS);
                if (htReqs != null)
                {
                    foreach (string k in htReqs.Keys)
                    {
                        LogIfNonexistentElementId(k, r.Id, "(requirements)");
                        r.Requirements.Add(k, Convert.ToInt32(htReqs[k]));
                    }
                }
            }
            catch (Exception e)
            {
                LogProblem("Problem importing requirements for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KREQUIREMENTS);

            /////////////////////////////////////////////

            //ASPECTS
            try
            {
                Hashtable htAspects = htEachRecipe.GetHashtable(NoonConstants.KASPECTS);
                if (htAspects != null)
                {
                    foreach (string k in htAspects.Keys)
                    {
                        LogIfNonexistentElementId(k, r.Id, "(aspects)");
                        r.Aspects.Add(k, Convert.ToInt32(htAspects[k]));
                    }
                }
            }
            catch (Exception e)
            {
                LogProblem("Problem importing aspects for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KASPECTS);

            /////////////////////////////////////////////


            //EFFECTS
            try
            {
                Hashtable htEffects = htEachRecipe.GetHashtable(NoonConstants.KEFFECTS);
                if (htEffects != null)
                {
                    foreach (string k in htEffects.Keys)
                    {
                        LogIfNonexistentElementId(k, r.Id, "(effects)");
                        r.Effects.Add(k, Convert.ToInt32(htEffects[k]));
                    }
                }
            }
            catch (Exception e)
            {
                LogProblem("Problem importing effects for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KEFFECTS);

            /////////////////////////////////////////////
            try
            {

                Hashtable htSlots = htEachRecipe.GetHashtable(NoonConstants.KSLOTS);
                r.SlotSpecifications = AddSlotsFromHashtable(htSlots);
                if (r.SlotSpecifications.Count > 1)
                    LogProblem(r.Id + " has more than one slot specified, which we don't allow at the moment.");
            }
            catch (Exception e)
            {

                LogProblem("Problem importing slots for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KSLOTS);

            try
            {

                ArrayList alRecipeAlternatives = htEachRecipe.GetArrayList(NoonConstants.KALTERNATIVERECIPES);
                if (alRecipeAlternatives != null)
                {
                    foreach (Hashtable ra in alRecipeAlternatives)
                    {
                        string raID = ra[NoonConstants.KID].ToString();
                        int raChance = Convert.ToInt32(ra[NoonConstants.KCHANCE]);
                        bool raAdditional = Convert.ToBoolean(ra[NoonConstants.KADDITIONAL] ?? false);

                        r.AlternativeRecipes.Add(new LinkedRecipeDetails(raID, raChance, raAdditional));
                    }
                }
            }
            catch (Exception e)
            {

                LogProblem("Problem importing alternative recipes for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KALTERNATIVERECIPES);


            try
            {
                ArrayList alLinkedRecipes = htEachRecipe.GetArrayList(NoonConstants.KLINKED);
                if (alLinkedRecipes != null)
                {
                    foreach (Hashtable lr in alLinkedRecipes)
                    {
                        string lrID = lr[NoonConstants.KID].ToString();
                        int lrChance = Convert.ToInt32(lr[NoonConstants.KCHANCE]);
                        bool lrAdditional = Convert.ToBoolean(lr[NoonConstants.KADDITIONAL] ?? false);

                        r.LinkedRecipes.Add(new LinkedRecipeDetails(lrID, lrChance, lrAdditional));
                    }
                }

            }
            catch (Exception e)
            {

                LogProblem("Problem importing linked recipes for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KLINKED);
            Recipes.Add(r);

            htEachRecipe.Remove("comments"); //this should be the only nonprocessed property at this point

            foreach (var k in htEachRecipe.Keys)
            {
                NoonUtility.Log("Unprocessed recipe property for " + r.Id + ": " + k);
            }

        }

        foreach (var r in Recipes)
        {
            foreach (var n in r.LinkedRecipes)
                LogIfNonexistentRecipeId(n.Id, r.Id, " - as next recipe");
            foreach (var a in r.AlternativeRecipes)
                LogIfNonexistentRecipeId(a.Id, r.Id, " - as alternative");
        }
    }

    private void LogIfNonexistentElementId(string elementId, string recipeId, string context)
    {
        if (!Elements.ContainsKey(elementId))
            LogProblem("'" + recipeId + "' references non-existent element '" + elementId + "' " + " " + context);
    }

    private void LogIfNonexistentDeckId(string deckId, string recipeId)
    {
        if (!DeckSpecs.ContainsKey(deckId))
            LogProblem("'" + recipeId + "' references non-existent deckSpec '" + deckId + "'");
    }

    private void LogIfNonexistentRecipeId(string referencedId, string parentRecipeId, string context)
    {
        if (referencedId != null && Recipes.All(r => r.Id != referencedId))
            LogProblem(
                "'" + parentRecipeId + "' references non-existent recipe '" + referencedId + "' " + " " + context);
    }

    private void LogMissingImages()
    {
        //check for missing images
        var allElements = _compendium.GetAllElementsAsDictionary();
        string missingAspectImages = "";
        int missingAspectImageCount = 0;
        string missingElementImages = "";
        int missingElementImageCount = 0;
        foreach (var k in allElements.Keys)
        {
            if (allElements[k].IsAspect)
            {
                if (ResourcesManager.GetSpriteForAspect(k) == null)
                {
                    missingAspectImages += (" " + k);
                    missingAspectImageCount++;
                }
            }
            else
            {
                if (ResourcesManager.GetSpriteForElement(k) == null)
                {
                    missingElementImages += (" " + k);
                    missingElementImageCount++;
                }
            }
        }

        if (missingAspectImages != "")
            NoonUtility.Log("Missing " + missingAspectImageCount + " images for aspects:" + missingAspectImages);

        if (missingElementImages != "")
            NoonUtility.Log("Missing " + missingElementImageCount + " images for elements:" + missingElementImages);
    }

    public void PopulateCompendium(ICompendium compendium)
    {
        _compendium = compendium;
        ImportVerbs();
        ImportElements();
        ImportDeckSpecs();
        ImportRecipes();
        ImportLegacies();

        //I'm not sure why I use fields rather than local variables returned from the import methods?
        //that might be something to tidy up; I suspect it's left from an early design

        _compendium.UpdateRecipes(Recipes);
        _compendium.UpdateElements(Elements);
        _compendium.UpdateVerbs(Verbs);
        _compendium.UpdateDeckSpecs(DeckSpecs);
        _compendium.UpdateLegacies(Legacies);

        LogMissingImages();
        LogFnords();
     

        foreach (var p in GetContentImportProblems())
            NoonUtility.Log(p.Description);

    }

    private void LogFnords()
    {
        const string FNORD = "FNORD";

        var allElements = _compendium.GetAllElementsAsDictionary();
        string elementFnords = "";
        int elementFnordCount = 0;
        foreach (var k in allElements.Keys)
        {
            var thisElement = allElements[k];

            if (thisElement.Label.ToUpper().Contains(FNORD)
            || thisElement.Description.ToUpper().Contains(FNORD)
            )
            {
                    elementFnords += (" " + k);
                    elementFnordCount++;
            }
        }

        var allRecipes = _compendium.GetAllRecipesAsList();
        string recipeFnords = "";
        int recipeFnordCount = 0;
        foreach (var r in allRecipes)
        {

            if (r.Label.ToUpper().Contains(FNORD)
                || r.StartDescription.ToUpper().Contains(FNORD)
                || r.Description.ToUpper().Contains(FNORD)

            )
            {

                recipeFnords += (" " + r.Id);
                recipeFnordCount++;
            }
        }


        if (elementFnords != "")
            NoonUtility.Log(elementFnordCount + "  fnords for elements:" + elementFnords);

        if (recipeFnords != "")
            NoonUtility.Log(recipeFnordCount + "  fnords for recipes:" + recipeFnords);


    }
}
