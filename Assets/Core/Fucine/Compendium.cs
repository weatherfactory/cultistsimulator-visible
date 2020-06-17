using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Noon;
using OrbCreationExtensions;
using UnityEngine.Rendering;
using UnityEngine.Analytics;

public interface ICompendium
{
    void UpdateRecipes(List<Recipe> allRecipes);
    void UpdateElements(Dictionary<string, Element> elements);
    void UpdateVerbs(Dictionary<string, IVerb> verbs);
    void UpdateLegacies(Dictionary<string, Legacy> legacies);
    void UpdateEndings(Dictionary<string, Ending> endings);
    void UpdateDeckSpecs(Dictionary<string, IDeckSpec> deckSpecs);
    Recipe GetFirstRecipeForAspectsWithVerb(AspectsInContext aspectsInContext, string verb, Character character,bool getHintRecipes);
    List<Recipe> GetAllRecipesAsList();
    Recipe GetRecipeById(string recipeId);
    Dictionary<string,Element> GetAllElementsAsDictionary();
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);

    List<IVerb> GetAllVerbs();
    IVerb GetVerbById(string verbId);
    IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command);

    List<Ending> GetAllEndings();
    Ending GetEndingById(string endingFlag);

    List<Legacy> GetAllLegacies();
    Legacy GetLegacyById(string legacyId);

    List<IDeckSpec> GetAllDeckSpecs();
    IDeckSpec GetDeckSpecById(string id);
    void SupplyLevers(IGameEntityStorage populatedCharacter);
    string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects);

    
}

public class Compendium : ICompendium
{
    private List<Recipe> _recipes;
    private Dictionary<string, Recipe> _recipeDict;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, IVerb> _verbs;
    private Dictionary<string, Legacy> _legacies;
    private Dictionary<string, Ending> _endings;
    private Dictionary<string, IDeckSpec> _decks;
    private Dictionary<LegacyEventRecordId, string> _pastLevers;

    // -- Update Collections ------------------------------

    public void UpdateRecipes(List<Recipe> allRecipes)
    {
        _recipes = allRecipes;
        _recipeDict = new Dictionary<string, Recipe>();

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

    public void UpdateElements(Dictionary<string, Element> elements)
    {
        _elements = elements;
    }

    public void UpdateVerbs(Dictionary<string, IVerb> verbs)
    {
        _verbs = verbs;
    }

    public void UpdateDeckSpecs(Dictionary<string, IDeckSpec> deckSpecs)
    {
        _decks = deckSpecs;
    }

    public void UpdateLegacies(Dictionary<string, Legacy> legacies)
    {
        _legacies = legacies;
    }

    public void UpdateEndings(Dictionary<string, Ending> endings)
    {
        _endings = endings;
    }

    // -- Misc Getters ------------------------------

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

    public IVerb GetVerbById(string verbId) {
        IVerb verb;
        _verbs.TryGetValue(verbId, out verb);

        return verb;
    }

    public IDeckSpec GetDeckSpecById(string id) {
        IDeckSpec deck;
        _decks.TryGetValue(id, out deck);

        return deck;
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
            var e = _elements[k];
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
                    if (!string.IsNullOrEmpty(e.OverrideVerbIcon))
                        return e.OverrideVerbIcon;
                }
                catch (Exception)
                {
                   throw new ApplicationException("Couldn't find OverrideVerbIcon for element" + a.Key  + " - does that element exist?");
                }
            }
        }

        return null;
    }
}
