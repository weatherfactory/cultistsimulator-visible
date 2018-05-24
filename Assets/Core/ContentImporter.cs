using UnityEngine;
using System;
using Noon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

public class ContentImporter
{
    private IList<ContentImportProblem> contentImportProblems;
    private const string CONST_CONTENTDIR = "content/";
    private const string CONST_ELEMENTS = "elements";
    private const string CONST_RECIPES = "recipes";
    private const string CONST_VERBS = "verbs";
    private const string CONST_DECKS = "decks";
    private const string CONST_LEGACIES = "legacies";
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

    public List<SlotSpecification> AddSlotsFromArrayList(ArrayList alSlots)
    {
        List<SlotSpecification> cssList = new List<SlotSpecification>();

   
            foreach (Hashtable htThisSlot in alSlots)
            {
                string slotId = htThisSlot[NoonConstants.KID].ToString();

                SlotSpecification slotSpecification = new SlotSpecification(slotId);
                try
                {
                    if (htThisSlot[NoonConstants.KLABEL] != null)
                        slotSpecification.Label = htThisSlot[NoonConstants.KLABEL].ToString();

                if (htThisSlot[NoonConstants.KDESCRIPTION] != null)
                        slotSpecification.Description = htThisSlot[NoonConstants.KDESCRIPTION].ToString();

                    if ((string) htThisSlot[NoonConstants.KGREEDY] == "true")
                        slotSpecification.Greedy = true;

                    if ((string) htThisSlot[NoonConstants.KCONSUMES] == "true")
                        slotSpecification.Consumes = true;


                    if ((string)htThisSlot[NoonConstants.KNOANIM] == "true")
                        slotSpecification.NoAnim = true;

                if (htThisSlot[NoonConstants.KACTIONID] != null)
                        slotSpecification.ForVerb = htThisSlot[NoonConstants.KACTIONID].ToString();
       

                Hashtable htRequired = htThisSlot["required"] as Hashtable;
                    if (htRequired != null)
                    {
                        foreach (string rk in htRequired.Keys)
                            slotSpecification.Required.Add(rk, Convert.ToInt32(htRequired[rk]));
                    }

                    Hashtable htForbidden = htThisSlot["forbidden"] as Hashtable;
                    if (htForbidden != null)
                    {
                        foreach (string fk in htForbidden.Keys)
                            slotSpecification.Forbidden.Add(fk, Convert.ToInt32(htRequired[fk]));
                    }
                }
                catch (Exception e)
                {
                    LogProblem("Couldn't retrieve slot " + slotId + " - " + e.Message);
                }

                cssList.Add(slotSpecification);
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
            var htElements = SimpleJsonImporter.Import(json);
            totalElementsFound += PopulateElements(htElements);
        }

        NoonUtility.Log("Total elements found: " + totalElementsFound,2);

        foreach (var e in Elements)
        {
            foreach (var xt in e.Value.XTriggers)
            {
                if (!Elements.ContainsKey(xt.Value))
                    LogProblem("Element " + e.Key + " specifies an invalid result (" + xt.Value + ") for xtrigger " + xt.Key);
            }
            if(!string.IsNullOrEmpty(e.Value.DecayTo))
            { 
                if(!Elements.ContainsKey(e.Value.DecayTo))
                    LogProblem("Element " + e.Key + " specifies an invalid result (" + e.Value.DecayTo + ") for DecayTo. ");

            }
        }
    }

    public int PopulateElements(Hashtable htElements)
    {

        if (htElements == null)
            LogProblem("Elements were never imported; PopulateElementForId failed");

        ArrayList alElements = htElements.GetArrayList("elements");

        foreach (Hashtable htElement in alElements)
        {


            Hashtable htAspects = htElement.GetHashtable(NoonConstants.KASPECTS);
            ArrayList alSlots = htElement.GetArrayList(NoonConstants.KSLOTS);
            Hashtable htXTriggers = htElement.GetHashtable(NoonConstants.KXTRIGGERS);


            Element element = new Element(htElement.GetString(NoonConstants.KID),
              htElement.GetString(NoonConstants.KLABEL),
                htElement.GetString(NoonConstants.KDESCRIPTION),
                htElement.GetInt(NoonConstants.KANIMFRAMES),
                htElement.GetString(NoonConstants.KICON));

            if(element.Label==null)
                LogProblem("No label for element " + element.Id);

            if (element.Description == null)
                LogProblem("No description for element " + element.Id);
            try
            {

                if (htElement.ContainsKey(NoonConstants.KLIFETIME))
                    element.Lifetime = float.Parse(htElement[NoonConstants.KLIFETIME].ToString());
                if (htElement.ContainsKey(NoonConstants.KDECAYTO))
                    element.DecayTo = htElement.GetString(NoonConstants.KDECAYTO);

                if (htElement.GetString(NoonConstants.KISASPECT) == "true")
                    element.IsAspect = true;
                else
                    element.IsAspect = false;

                if (htElement.GetString(NoonConstants.KNOARTNEEDED) == "true")
                    element.NoArtNeeded = true;
                else
                    element.NoArtNeeded = false;

                if (htElement.GetString(NoonConstants.KUNIQUE) == "true")
                    element.Unique = true;
                else
                    element.Unique = false;

                element.Aspects = NoonUtility.ReplaceConventionValues(htAspects);
                if(alSlots!=null)
                element.ChildSlotSpecifications = AddSlotsFromArrayList(alSlots);
                foreach(var css in element.ChildSlotSpecifications)
                { 
                    if(string.IsNullOrEmpty(css.ForVerb))
                LogProblem("No actionId for a slot on " + element.Id + " with id " + css.Id);
                }
                if (htXTriggers != null)
                {
                    foreach (string k in htXTriggers.Keys)
                    {
                        //the element we want to transform this element to when the trigger fires
                        var xid = htXTriggers[k].ToString();
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

                        if (lrChance == 0)
                        {
                            LogProblem("Chance 0 or not specified in induced recipes for element " + element.Id);
                        }
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
        NoonUtility.Log("Total recipes found: " + recipesArrayList.Count,2);

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
            ArrayList alSlots = h.GetArrayList(NoonConstants.KSLOTS);
            if (alSlots != null)
            { 
        var slots=AddSlotsFromArrayList(alSlots);
                if (slots.Count > 1)
                    LogProblem(v.Id + " has more than one slot specified - we should only have a primary slot");
                else
                    v.PrimarySlotSpecification = slots.First();
            }
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
                        if(!v.Contains(NoonConstants.DECK_PREFIX))
                            LogIfNonexistentElementId(v, htEachDeck[NoonConstants.KID].ToString(), "(deckSpec spec items)");
                        

                            thisDeckSpec.Add(v);
                    }
                }
            }
            catch (Exception e)
            {
                LogProblem("Problem importing drawable items for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                           "' - " + e.Message);
            }
       

            bool resetOnExhaustion;
            try
            {
                resetOnExhaustion = Convert.ToBoolean(htEachDeck.GetValue(NoonConstants.KRESETONEXHAUSTION));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            string defaultCardId = "";
            //if we reset on exhaustion, we'll never see a default card, and we don't want
            //to throw an error on failing to import an unset default card.
            //Of course someone could have no default card and resetonexhaustion = false, but that's fundamentally their problem.
            if (!resetOnExhaustion)
            try
            {
                defaultCardId = htEachDeck.GetValue(NoonConstants.KDECKDEFAULTCARD).ToString();
            }
            catch (Exception e)
            {
                LogProblem("Problem importing default card for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                           "' - " + e.Message);
            }



            DeckSpec d = new DeckSpec(htEachDeck["id"].ToString(), thisDeckSpec, defaultCardId, resetOnExhaustion);


            try
            {
                Hashtable htDrawMessages = htEachDeck.GetHashtable(NoonConstants.KDECKSPEC_DRAWMESSAGES);
                if (htDrawMessages != null)
                {
                    d.DrawMessages = NoonUtility.HashtableToStringStringDictionary(htDrawMessages);

                    foreach (var drawmessagekey in d.DrawMessages.Keys)
                    {
                        if(!d.StartingCards.Contains(drawmessagekey))
                        LogProblem("Deckspec " + d.Id + " has a drawmessage for card " + drawmessagekey + ", but that card isn't in the list of drawable cards.");
                    }
                }
            }
            catch (Exception e)
            {
                LogProblem("Problem importing drawmessages for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                           "' - " + e.Message);
            }

            try
            {
                Hashtable htDefaultDrawMessages = htEachDeck.GetHashtable(NoonConstants.KDECKSPEC_DEFAULTDRAWMESSAGES);
                if (htDefaultDrawMessages != null)
                {
                    d.DefaultDrawMessages = NoonUtility.HashtableToStringStringDictionary(htDefaultDrawMessages);

                }
            }
            catch (Exception e)
            {
                LogProblem("Problem importing defaultdrawmessages for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                           "' - " + e.Message);
            }


            if (htEachDeck.ContainsKey(NoonConstants.KLABEL))
               d.Label = htEachDeck.GetValue(NoonConstants.KLABEL).ToString();
            if (htEachDeck.ContainsKey(NoonConstants.KDESCRIPTION))
                d.Description = htEachDeck.GetValue(NoonConstants.KDESCRIPTION).ToString();

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

            try
            {

                Legacy l = new Legacy(htEachLegacy[NoonConstants.KID].ToString(),
                    htEachLegacy[NoonConstants.KLABEL].ToString(), htEachLegacy[NoonConstants.KDESCRIPTION].ToString(),
                    htEachLegacy[NoonConstants.KSTARTDESCRIPTION].ToString(),
                    htEachLegacy[NoonConstants.KIMAGE].ToString(),
                    htEachLegacy[NoonConstants.KFROMENDING].ToString(),
                    Convert.ToBoolean(htEachLegacy[NoonConstants.KAVAILABLEWITHOUTENDINGMATCH])

                );

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

            catch
            {
                LogProblem("Can't parse this legacy: " + htEachLegacy[NoonConstants.KID].ToString());
            }
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

                r.SignalImportantLoop = Convert.ToBoolean(htEachRecipe[NoonConstants.KSIGNALIMPORTANTLOOP]);
                htEachRecipe.Remove(NoonConstants.KSIGNALIMPORTANTLOOP);

                if (htEachRecipe.Contains(NoonConstants.KPORTALEFFECT))
                {
                    string possiblePortalEffect = htEachRecipe[NoonConstants.KPORTALEFFECT].ToString();
                    try
                    {
                        r.PortalEffect = (PortalEffect)Enum.Parse(typeof(PortalEffect), possiblePortalEffect, true);
                        htEachRecipe.Remove(NoonConstants.KPORTALEFFECT);
                    }
                    catch 
                    {
                        LogProblem(r.Id + " has a PortalEffect specified that we don't think is right: " + possiblePortalEffect);
                    }
                    }

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

                
                


                r.Warmup = Convert.ToInt32(htEachRecipe[NoonConstants.KWARMUP]);
                htEachRecipe.Remove(NoonConstants.KWARMUP);


                r.EndingFlag = htEachRecipe[NoonConstants.KENDING] == null
                    ? null
                    : htEachRecipe[NoonConstants.KENDING].ToString();
                htEachRecipe.Remove(NoonConstants.KENDING);

               if(htEachRecipe[NoonConstants.KSIGNALENDINGFLAVOUR]==null)

                    r.SignalEndingFlavour=EndingFlavour.None;
               else
               {
                  string possibleSignalEndingFlavour= htEachRecipe[NoonConstants.KSIGNALENDINGFLAVOUR].ToString();
                   r.SignalEndingFlavour =
                       (EndingFlavour) Enum.Parse(typeof(EndingFlavour), possibleSignalEndingFlavour, true);
                   htEachRecipe.Remove(NoonConstants.KSIGNALENDINGFLAVOUR);
               }



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
            //DECKS

            try
            {
                Hashtable htDecks = htEachRecipe.GetHashtable(NoonConstants.KDECKEFFECT);
                if (htDecks != null)
                    foreach (string deckId in htDecks.Keys)
                    {
                        LogIfNonexistentDeckId(deckId, r.Id);
                        r.DeckEffects.Add(deckId,Convert.ToInt32(htDecks[deckId]));

                    }
            }
        
            catch (Exception e)
            {
                LogProblem("Problem importing decks for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KDECKEFFECT);
            /////////////////////////////////////////////
            //SLOTS
            try
            {

                ArrayList alSlots = htEachRecipe.GetArrayList(NoonConstants.KSLOTS);
                if(alSlots!=null)
                r.SlotSpecifications = AddSlotsFromArrayList(alSlots);
                if (r.SlotSpecifications.Count > 1)
                    LogProblem(r.Id + " has more than one slot specified, which we don't allow at the moment.");
            }
            catch (Exception e)
            {

                LogProblem("Problem importing slots for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KSLOTS);
            /////////////////////////////////////////////
            //ALTERNATIVES
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


                        if (raChance == 0)
                        {
                            LogProblem("Chance 0 or not specified in alternative recipes for recipe " + r.Id);
                        }
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




                        if (lrChance == 0)
                        {
                            LogProblem("Chance 0 or not specified in linked recipes for recipe " + r.Id);
                        }
                    }
                }

            }
            catch (Exception e)
            {

                LogProblem("Problem importing linked recipes for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KLINKED);


            /////////////////////////////////////////////
            //MUTATIONEFFECTS
            try
            {

                ArrayList alMutations = htEachRecipe.GetArrayList(NoonConstants.KMUTATIONS);
                if (alMutations != null)
                {
                    foreach (Hashtable htMutationEffect in alMutations)
                    {
                        string filterOnAspectId = htMutationEffect[NoonConstants.KFILTERONASPECTID].ToString();
                        string mutateAspectId = htMutationEffect[NoonConstants.KMUTATEASPECTID].ToString();
                        int mutationLevel= Convert.ToInt32(htMutationEffect[NoonConstants.KMUTATIONLEVEL]);
                        
                        bool additive = Convert.ToBoolean(htMutationEffect[NoonConstants.KADDITIVE] ?? false);

                        r.MutationEffects.Add(new MutationEffect(filterOnAspectId,mutateAspectId,mutationLevel,additive));

                    }
                }
            }
            catch (Exception e)
            {

                LogProblem("Problem importing mutationEffects recipes for recipe '" + r.Id + "' - " + e.Message);
            }

            htEachRecipe.Remove(NoonConstants.KMUTATIONS);




           


            //Finished! Import, tidy up.
            Recipes.Add(r);

            htEachRecipe.Remove("comments"); //this should be the only nonprocessed property at this point

            foreach (var k in htEachRecipe.Keys)
            {
                NoonUtility.Log("Unprocessed recipe property for " + r.Id + ": " + k);
            }

        }

       //check for common issues in recipes
        foreach (var r in Recipes)
        {
            if(r.Craftable && !r.Requirements.Any())
                LogProblem(r.Id + " is craftable, but has no requirements, so it will make its verb useless :O ");

            foreach (var n in r.LinkedRecipes)
                LogIfNonexistentRecipeId(n.Id, r.Id, " - as next recipe");
            foreach (var a in r.AlternativeRecipes)
                LogIfNonexistentRecipeId(a.Id, r.Id, " - as alternative");

            foreach (var m in r.MutationEffects)
            {
                LogIfNonexistentElementId(m.FilterOnAspectId,r.Id," - as mutation filter");
                LogIfNonexistentElementId(m.MutateAspectId, r.Id, " - as mutated aspect");
            }
        }
    }

    private void LogIfNonexistentElementId(string elementId, string containerId, string context)
    {
        if (!elementId.StartsWith(NoonConstants.LEVER_PREFIX) && !Elements.ContainsKey(elementId))
            LogProblem("'" + containerId + "' references non-existent element '" + elementId + "' " + " " + context);
    }

    private void LogIfNonexistentDeckId(string deckId, string containerId)
    {
        if (!DeckSpecs.ContainsKey(deckId))
            LogProblem("'" + containerId + "' references non-existent deckSpec '" + deckId + "'");
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
           var thisElement = allElements[k];

            if (thisElement.IsAspect )
            {
                if (!thisElement.NoArtNeeded && (ResourcesManager.GetSpriteForAspect(k) == null || ResourcesManager.GetSpriteForAspect(k).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME))
                {
                    missingAspectImages += (" " + k);
                    missingAspectImageCount++;
                }
            }
            else
            {
                

                if (!thisElement.NoArtNeeded && ResourcesManager.GetSpriteForElement(thisElement.Icon).name==ResourcesManager.PLACEHOLDER_IMAGE_NAME)
                {
                    missingElementImages += (" " + k);
                    missingElementImageCount++;
                }
            }
        }

        if (missingAspectImages != "")
            NoonUtility.Log("Missing " + missingAspectImageCount + " images for aspects:" + missingAspectImages,1);

        if (missingElementImages != "")
            NoonUtility.Log("Missing " + missingElementImageCount + " images for elements:" + missingElementImages,1);
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


        //var orderedElements = Elements.OrderByDescending(r => r.Value.Label.Length);
        //string output = string.Empty;
        //foreach (var or in orderedElements.Take(30))
        //    output += or.Key + "- " + or.Value.Label.Length+ "\n" + or.Value.Label + "\n\n";

        //Debug.Log(output);


        //List<string> studyRequirements=new List<string>();
        //string studyReqs=String.Empty;
        //foreach (var r in Recipes)
        //{
        //    if(r.Craftable && r.ActionId=="study")
        //    {
        //        foreach(var req in r.Requirements)
        //        {
        //            var reqElement = _compendium.GetElementById(req.Key);

        //        if(!studyRequirements.Contains(req.Key) && req.Value>0 && !reqElement.Aspects.ContainsKey("text"))
        //            studyRequirements.Add(req.Key);

        //        }
        //    }
        //}

        //foreach (var req in studyRequirements)
        //    studyReqs = studyReqs + req + ":1, ";


        //NoonUtility.Log(studyReqs);

#if DEBUG
        CountWords();
        LogMissingImages();
        LogFnords();
#endif



        foreach (var p in GetContentImportProblems())
            NoonUtility.Log(p.Description);

    }

    private void CountWords()
    {
        int words = 0;
        foreach (var r in Recipes)
        {
            words += (r.Label.Count(char.IsWhiteSpace)+1);
            words += (r.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (r.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var e in Elements.Values)
        {

            words += (e.Label.Count(char.IsWhiteSpace) + 1);
            words += (e.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var v in Verbs.Values)
        {
            words += (v.Label.Count(char.IsWhiteSpace) + 1);
            words += (v.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var l in Legacies.Values)
        {
            words += (l.Label.Count(char.IsWhiteSpace) + 1);
            words += (l.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (l.Description.Count(char.IsWhiteSpace) + 1);
        }

        NoonUtility.Log("Words (based on spaces +1 count): " + words,1);
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
            NoonUtility.Log(elementFnordCount + "  fnords for elements:" + elementFnords,1);

        if (recipeFnords != "")
            NoonUtility.Log(recipeFnordCount + "  fnords for recipes:" + recipeFnords,1);


    }

}
