using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Noon;
using UnityEngine.Analytics;

public interface ICompendium
{
    Recipe GetFirstRecipeForAspectsWithVerb(AspectsInContext aspectsInContext, string verb, Character character,bool getHintRecipes);
    List<Recipe> GetAllRecipesAsList();
    Recipe GetRecipeById(string recipeId);
    Dictionary<string,Element> GetAllElementsAsDictionary();
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);

    List<IVerb> GetAllVerbs();
    BasicVerb GetVerbById(string verbId);
    IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command);

    List<Ending> GetAllEndings();
    Ending GetEndingById(string endingFlag);

    List<Legacy> GetAllLegacies();
    Legacy GetLegacyById(string legacyId);

    List<IDeckSpec> GetAllDeckSpecs();
    DeckSpec GetDeckSpecById(string id);
    void SupplyLevers(IGameEntityStorage populatedCharacter);
    string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects);

    bool TryAddDeckSpec(DeckSpec deck);

    void AddEntity(string id, Type type, IEntityWithId entity);
    /// <summary>
    /// Run all second-stage populations that occur between / across entities
    /// </summary>
    void RefineAllEntities(ContentImportLog log);

    void Reset();

    void LogFnords(ContentImportLog log);
    void CountWords(ContentImportLog log);
    void LogMissingImages(ContentImportLog log);
}

public class Compendium : ICompendium
{
    private Dictionary<Type, IDictionary> allEntities;

    private List<Recipe> _recipes=new List<Recipe>();
    private Dictionary<LegacyEventRecordId, string> _pastLevers;

    private Dictionary<string, Recipe> _recipeDict;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, BasicVerb> _verbs;
    private Dictionary<string, Legacy> _legacies;
    private Dictionary<string, Ending> _endings;
    private Dictionary<string, DeckSpec> _decks;


    public Compendium()
    {
    Reset(); //a little inelegant to call this twice - we call it explicitly in the content importer too
    }

    public void Reset()
    {
        allEntities= new Dictionary<Type, IDictionary>();

        _recipeDict = new Dictionary<string, Recipe>();
    _elements = new Dictionary<string, Element>();
    _verbs = new Dictionary<string, BasicVerb>();
    _legacies = new Dictionary<string, Legacy>();
    _endings = new Dictionary<string, Ending>();
    _decks = new Dictionary<string, DeckSpec>();

         allEntities.Add(typeof(Recipe), _recipeDict);
        allEntities.Add(typeof(Element), _elements);
        allEntities.Add(typeof(BasicVerb), _verbs);
        allEntities.Add(typeof(Legacy), _legacies);
        allEntities.Add(typeof(Ending), _endings);
        allEntities.Add(typeof(DeckSpec), _decks);
    }


    public void AddEntity(string id, Type type, IEntityWithId entity)
    {
        var relevantStore = allEntities[type];
        relevantStore.Add(id,entity);

        if(type==typeof(Recipe))
            _recipes.Add(entity as Recipe);
    }

    public void RefineAllEntities(ContentImportLog log)
    {
        foreach (var d in allEntities.Values)
        {
            HashSet <IEntity> entities= new HashSet<IEntity>((IEnumerable<IEntity>) d.Values); //we might modify the collection as it gets refined, so we need to copy it first

            foreach (var e in entities)
                e.RefineWithCompendium(log,this);

        }
    }

    // -- Update Collections ------------------------------

    public void UpdateRecipes(List<Recipe> allRecipes)
    {
        _recipes = allRecipes;
        
        foreach (var item in allRecipes) {
            if (_recipeDict.ContainsKey(item.Id)) {
                #if UNITY_EDITOR
                UnityEngine.Debug.LogWarning("Duplicate Recipe Id " + item.Id + "! Skipping...");
                #endif
                continue;
            }

            _recipeDict.Add(item.Id, item);
        }
    }

    /// <summary>

    /// </summary>
    /// <param name="aspects"></param>
    /// <param name="verb"></param>
    /// <param name="character"></param>
    /// <param name="getHintRecipes">If true, get recipes with hintonly=true (and *only* hintonly=true)</param>
    /// <returns></returns>
    public Recipe GetFirstRecipeForAspectsWithVerb(AspectsInContext aspectsInContext, string verb, Character character, bool getHintRecipes)
    {

        aspectsInContext.ThrowErrorIfNotPopulated(verb);
        //for each recipe,
        //note: we *either* get craftable recipes *or* if we're getting hint recipes we don't care if they're craftable
        List<Recipe> candidateRecipes=_recipes.Where(r => r.ActionId == verb && ( r.Craftable || getHintRecipes) && r.HintOnly==getHintRecipes && !character.HasExhaustedRecipe(r)).ToList();
        foreach (var recipe in candidateRecipes )
        {
            //for each requirement in recipe, check if that aspect does *not* exist at that level in Aspects

            if (recipe.RequirementsSatisfiedBy(aspectsInContext) )
                return recipe;

        }

        return null;
    }

    public Boolean IsKnownElement(string elementId) {
        return _elements.ContainsKey(elementId);
    }

    // -- Get All ------------------------------

    public List<Recipe> GetAllRecipesAsList() {
        return _recipes;
    }

    public Dictionary<string, Element> GetAllElementsAsDictionary() {
        return _elements;
    }

    public List<IVerb> GetAllVerbs() {
        return new List<IVerb>(_verbs.Values);
    }

    public List<IDeckSpec> GetAllDeckSpecs() {
        return new List<IDeckSpec>(_decks.Values);
    }

    public List<Legacy> GetAllLegacies() {
        return new List<Legacy>(_legacies.Values);
    }

    public List<Ending> GetAllEndings() {
        return new List<Ending>(_endings.Values);
    }

    // -- Get By Id ------------------------------

    public Recipe GetRecipeById(string recipeId)
    {
        if (recipeId == null || recipeId=="NULL") //sometimes this gets parsed out of the save data?
            return null;
        if (_recipeDict.ContainsKey(recipeId))
        {
            var recipe = _recipeDict[recipeId];
            return recipe;
        }
        else
        throw new ApplicationException("Can't find recipe id '" + recipeId + "'");

    }

    public Element GetElementById(string elementId) {
        Element element;
        if (elementId.StartsWith(NoonConstants.LEVER_PREFIX))
        {

            string leverId = elementId.Replace(NoonConstants.LEVER_PREFIX, "");
            if (!Enum.IsDefined(typeof(LegacyEventRecordId), leverId))
                return null;
            else
            {
            LegacyEventRecordId leverEnum = (LegacyEventRecordId) Enum.Parse(typeof(LegacyEventRecordId), leverId);
                if (!_pastLevers.ContainsKey(leverEnum))
                    return null;
                else
                elementId = _pastLevers[leverEnum];

            }

        }


        _elements.TryGetValue(elementId, out element);

        return element;
    }

    public BasicVerb GetVerbById(string verbId) {
        BasicVerb verb;
        _verbs.TryGetValue(verbId, out verb);

        return verb;
    }

    public DeckSpec GetDeckSpecById(string id) {
        DeckSpec deck;
        _decks.TryGetValue(id, out deck);

        return deck;
    }


    public bool TryAddDeckSpec(DeckSpec deck)
    {
        if (!_decks.ContainsKey(deck.Id))
        {
            _decks.Add(deck.Id,deck);
            return true;
        }

        return false;
    }



    public Legacy GetLegacyById(string legacyId) {
        Legacy legacy;
        _legacies.TryGetValue(legacyId, out legacy);

        return legacy;
    }

    public Ending GetEndingById(string endingId)
	{
		Analytics.CustomEvent( "A_ENDING", new Dictionary<string,object>{ {"id",endingId} } );

        Ending ending;
		if (_endings.TryGetValue(endingId, out ending))
		{
			return ending;
		}

		return Ending.DefaultEnding();
    }

    // -- Assorted Methods ------------------------------

    public IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command)
    {
        var candidateVerb = GetVerbById(command.Recipe.ActionId);

        if (candidateVerb != null)
            return candidateVerb;

        var createdVerb = new CreatedVerb(command.Recipe.ActionId, command.Recipe.Label,command.Recipe.Description);

        return createdVerb;
    }
	
    /// <summary>
    /// allow the character to specify levers (legacy event records)
    /// replace tokens with lever values, and also store the levers for later use
    /// if we want to retrieve the actual levered elements.
    /// </summary>
    /// <param name="populatedCharacter"></param>
    public void SupplyLevers(IGameEntityStorage populatedCharacter)
    {
        _pastLevers = populatedCharacter.GetAllPastLegacyEventRecords();
        TokenReplacer tr = new TokenReplacer(populatedCharacter,this);

        foreach (var r in _recipes)
        {

            r.Label = tr.ReplaceTextFor(r.Label);
            r.StartDescription = tr.ReplaceTextFor(r.StartDescription);
            r.Description = tr.ReplaceTextFor(r.Description);
        }

        foreach (var k in _elements.Keys)
        {
            var e = _elements[k] as Element;
            e.Label = tr.ReplaceTextFor(e.Label);
            e.Description = tr.ReplaceTextFor(e.Description);

        }

    }

    public string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects)
    {
        if (currentAspects != null)
        {
            foreach (var a in currentAspects.Where(a=>a.Value>0))
            { 
                var e = GetElementById(a.Key);
                try
                {
                    //assume only one override, but out after
                    if (!string.IsNullOrEmpty(e.VerbIcon))
                        return e.VerbIcon;
                }
                catch (Exception)
                {
                   throw new ApplicationException("Couldn't find VerbIcon for element" + a.Key  + " - does that element exist?");
                }
            }
        }

        return null;
    }


    public void LogFnords(ContentImportLog log)
    {
        const string FNORD = "FNORD";

        var allElements = GetAllElementsAsDictionary();
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

        var allRecipes = GetAllRecipesAsList();
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
            log.LogInfo(elementFnordCount + "  fnords for elements:" + elementFnords);
        else
            log.LogInfo("No fnords found for elements.");
        if (recipeFnords != "")
            log.LogInfo(recipeFnordCount + "  fnords for recipes:" + recipeFnords);
        else
            log.LogInfo("No fnords found for recipes.");

    }

    public void CountWords(ContentImportLog log)
    {
        int words = 0;
        foreach (var r in _recipes)
        {
            words += (r.Label.Count(char.IsWhiteSpace) + 1);
            words += (r.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (r.Description.Count(char.IsWhiteSpace) + 1);
        }



        foreach (var e in _elements.Values)
        {
            words += (e.Label.Count(char.IsWhiteSpace) + 1);
            words += (e.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var v in _verbs.Values)
        {
            words += (v.Label.Count(char.IsWhiteSpace) + 1);
            words += (v.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var l in _legacies.Values)
        {
            words += (l.Label.Count(char.IsWhiteSpace) + 1);
            words += (l.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (l.Description.Count(char.IsWhiteSpace) + 1);
        }

        log.LogInfo("Words (based on spaces +1 count): " + words);

    }

    public void LogMissingImages(ContentImportLog log)
    {
        //check for missing images
        var allElements = GetAllElementsAsDictionary();
        string missingAspectImages = "";
        int missingAspectImageCount = 0;
        string missingElementImages = "";
        int missingElementImageCount = 0;
        foreach (var k in allElements.Keys)
        {
            var thisElement = allElements[k];

            if (thisElement.IsAspect)
            {
                if ((!thisElement.NoArtNeeded && !thisElement.IsHidden) && (ResourcesManager.GetSpriteForAspect(thisElement.Icon) == null || ResourcesManager.GetSpriteForAspect(thisElement.Icon).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME))
                {
                    missingAspectImages += (" " + k);
                    missingAspectImageCount++;
                }
            }
            else
            {
                if (!thisElement.NoArtNeeded && ResourcesManager.GetSpriteForElement(thisElement.Icon).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME)
                {
                    missingElementImages += (" " + k);
                    missingElementImageCount++;
                }
            }
        }

        if (missingAspectImages != "")
            log.LogInfo("Missing " + missingAspectImageCount + " images for aspects:" + missingAspectImages);
        else
            log.LogInfo("No missing aspect images found.");

        if (missingElementImages != "")
            log.LogInfo("Missing " + missingElementImageCount + " images for elephants:" + missingElementImages);
        else
            log.LogInfo("No missing elephant images found.");
    }


}
