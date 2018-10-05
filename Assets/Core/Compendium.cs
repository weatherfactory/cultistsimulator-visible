using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
    void UpdateDeckSpecs(Dictionary<string, IDeckSpec> deckSpecs);
    Recipe GetFirstRecipeForAspectsWithVerb(IAspectsDictionary aspects, string verb, Character character,bool getHintRecipes);
    List<Recipe> GetAllRecipesAsList();
    Recipe GetRecipeById(string recipeId);
    Dictionary<string,Element> GetAllElementsAsDictionary();
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);
    List<IVerb> GetAllVerbs();
    IVerb GetVerbById(string verbId);
    Ending GetEndingById(string endingFlag);
    IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command);

    List<Legacy> GetAllLegacies();
    Legacy GetLegacyById(string legacyId);

    List<IDeckSpec> GetAllDeckSpecs();
    IDeckSpec GetDeckSpecById(string id);
    void SupplyLevers(IGameEntityStorage populatedCharacter);
}

public class Compendium : ICompendium
{
    private List<Recipe> _recipes;
    private Dictionary<string, Recipe> _recipeDict;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, IVerb> _verbs;
    private Dictionary<string, Legacy> _legacies;
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


    // -- Misc Getters ------------------------------

    /// <summary>

    /// </summary>
    /// <param name="aspects"></param>
    /// <param name="verb"></param>
    /// <param name="character"></param>
    /// <param name="getHintRecipes">If true, get recipes with hintonly=true (and *only* hintonly=true)</param>
    /// <returns></returns>
    public Recipe GetFirstRecipeForAspectsWithVerb(IAspectsDictionary aspects, string verb, Character character, bool getHintRecipes)
    {
        //for each recipe,
        //note: we *either* get craftable recipes *or* if we're getting hint recipes we don't care if they're craftable
        List<Recipe> candidateRecipes=_recipes.Where(r => r.ActionId == verb && ( r.Craftable || getHintRecipes) && r.HintOnly==getHintRecipes && !character.HasExhaustedRecipe(r)).ToList();
        foreach (var recipe in candidateRecipes )
        {
            //for each requirement in recipe, check if that aspect does *not* exist at that level in Aspects

            if (recipe.RequirementsSatisfiedBy(aspects))
                return recipe;
            //Why wasn't the code using RequirementsSatisfiedBy? I think because it's very old code; but I've left it
            //here for now in case there was a good reason for special case behaviour (like not honouring -1/NOT) - AK
            //bool matches = true;
            //foreach (string requirementId in recipe.Requirements.Keys)
            //{

            //    if (!aspects.Any(a => a.Key == requirementId && a.Value >= recipe.Requirements[requirementId]))
            //        matches = false;
            //}
            //if none fail, return that recipe
            //if (matches) return recipe;
            //if any fail, continue
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
        throw new ApplicationException("Can't find recipe id " + recipeId);
        
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

    // -- Assorted Methods ------------------------------

    public IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command)
    {
        var candidateVerb = GetVerbById(command.Recipe.ActionId);

        if (candidateVerb != null)
            return candidateVerb;

        var createdVerb = new CreatedVerb(command.Recipe.ActionId, command.Recipe.Label,command.Recipe.Description);

        return createdVerb;
    }

    public Ending GetEndingById(string endingFlag)
    {
		Analytics.CustomEvent( "A_ENDING", new Dictionary<string,object>{ {"id",endingFlag} } );

		string textId1 = "END_" + endingFlag.ToUpper() + "_1";
		string textId2 = "END_" + endingFlag.ToUpper() + "_2";
		string textId3 = "END_" + endingFlag.ToUpper() + "_3";
		string textId4 = "END_" + endingFlag.ToUpper() + "_4";

		string title = LanguageTable.Get( textId1 );
		string desc = LanguageTable.Get( textId2 ) + " " + LanguageTable.Get( textId3 ) + " " + LanguageTable.Get( textId4 );

        if (endingFlag == "minorforgevictory")
            return new Ending(endingFlag, title, desc, "forgeofdays", EndingFlavour.Grand, "DramaticLightCool","A_ENDING_MINORFORGEVICTORY");

        if (endingFlag== "minorgrailvictory")
            return new Ending(endingFlag, title, desc, "redgrail", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORGRAILVICTORY");

        if (endingFlag == "minorlanternvictory")
            return new Ending(endingFlag, title, desc, "doorintheeye", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORLANTERNVICTORY");

        if (endingFlag=="deathofthebody")
            return new Ending(endingFlag, title, desc, "suninrags", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_DEATHOFTHEBODY");

        if (endingFlag == "despairending")
            return new Ending(endingFlag, title, desc, "despair", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_DESPAIRENDING");

        if (endingFlag == "visionsending")
            return new Ending(endingFlag, title, desc, "madness", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_VISIONSENDING");

        if (endingFlag == "wintersacrifice")
            return new Ending(endingFlag, title, desc, "suninrags", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WINTERSACRIFICE");

        if (endingFlag == "arrest") 
            return new Ending(endingFlag, title, desc, "notorious", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_ARREST");

        if (endingFlag == "workvictory")
            return new Ending(endingFlag, title, desc, "insomnia", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WORKVICTORY");

        if (endingFlag == "workvictoryb")
            return new Ending(endingFlag, title, desc, "finehouse", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WORKVICTORYB");

        return Ending.DefaultEnding();
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
}
