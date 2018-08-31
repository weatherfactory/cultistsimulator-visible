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
        if (endingFlag == "minorforgevictory")
            return new Ending(endingFlag, "THE CONFLAGRATION OF THE HEART",
                "For a little while I dwell in the high rooms of the Mansus, and then I return through the Tricuspid Gate, and my body stirs in the ashes. I am hairless and imperishable as marble, and the Forge's fire still burns within me. " +
                "I carry the Shaping Strength. I will not grow old. Perhaps I will rebel. Perhaps, one day, I will rise even higher." +
                " [Congratulations on a standard Power victory. You have wrestled the game to its knees. There are other paths.] ", "forgeofdays", EndingFlavour.Grand, "DramaticLightCool","A_ENDING_MINORFORGEVICTORY");

        if (endingFlag== "minorgrailvictory")
            return new Ending(endingFlag,"THE FEAST OF THE TRUE BIRTH",
                "For a little while I dwell in the high rooms of the Mansus, and then I return through the Tricuspid Gate, and I tear free of the sticky rags of my old flesh. My new body is smooth without and red within like a sweet fruit." +
                " My limbs are strong as cables. My senses are knives. I will not grow old. I will walk the world in the service of the Grail, feasting, growing. Perhaps I will rebel. Perhaps, one day, I will rise even higher." +
                " [Congratulations on a standard Sensation victory. You have wrestled the game to its knees. There are other paths.] ", "redgrail", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORGRAILVICTORY");

        if (endingFlag == "minorlanternvictory")
            return new Ending(endingFlag, "THE INCURSUS",
                "I have passed through the Tricuspid Gate, and entered the high rooms of the Mansus. I will not live, but neither will I die. The Glory is very close here. It leaks through the fabric of the House to contribute its light. " +
                " One day - perhaps one day soon - the Pilgrimage will conclude, and the Watchman will permit seven souls to ascend further. The Hour called Vagabond will be the first. Perhaps I will be the seventh." +
                " [Congratulations on a standard Enlightenment victory. You have wrestled the game to its knees. There are other paths.] ", "doorintheeye", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORLANTERNVICTORY");

        if (endingFlag=="deathofthebody")
            return new Ending(endingFlag, "MY BODY IS DEAD",
                "Where will they find me? I am not here. In the end, my strength was insufficient to sustain my failing heart. [I was starving, and I had no Health remaining. I should have " +
                "ensured I had money to purchase essentials; I could have used Dream to rest and recover from my weakness.]", "suninrags", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_DEATHOFTHEBODY");

        if (endingFlag == "despairending")
            return new Ending(endingFlag, "NO MORE",
                "Despair, the wolf that devours thought. Am I alive, or am I dead? It no longer matters. [I allowed the Despair token to reach 3 Dread or Injury.]", "despair", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_DESPAIRENDING");
        if (endingFlag == "visionsending")
            return new Ending(endingFlag, "GLORY",
                "First it was the dreams. Then it was the visions. Now it's everything. I no longer have any idea what is real, and what is not. [I allowed the Visions token to reach 3 Fascination.]", "madness", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_VISIONSENDING");

        if (endingFlag == "wintersacrifice")
            return new Ending(endingFlag, "GOING QUIETLY",
                "In the upper room of the house where I am taken, my breath fogs and my eyes grow soft. The light in the room is the light at the end of the sun. I am a beautiful ending.", "suninrags", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WINTERSACRIFICE");

        if (endingFlag == "arrest") 
            return new Ending(endingFlag, "BARS ACROSS THE SUN",
                "The nature of my crimes was vague, and the trial contentious. But there is a consensus that I have done something I should not. I wish it could have been different. I wish " +
                " that I could have done <i>everything</i> I should not. [Many Hunters have specific weaknesses. Perhaps you can use those weaknesses to stop them before they bring you to trial.]", "notorious", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_ARREST"
                );

        if (endingFlag == "workvictory")
            return new Ending(endingFlag, "EVENING FALLS",
                "I have my fire, my books, my clock, my window on the world where they do other things. I could have been unhappy. I'm not unhappy. This was a successful life, and when it is " +
                "over the sweet earth will fill my mouth, softer than splinters. [This might be considered a victory.]", "insomnia", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WORKVICTORY");

        if (endingFlag == "workvictoryb")
            return new Ending(endingFlag, "AMBITION'S TIDE",
                "I will rise high, and higher yet. My affairs will prosper. I will fill a fine house with elegant things. I will be honoured by my peers and slandered by my  rivals. I will grow " +
                "used to the sound of my name. Then one day, I will die, and some time after that, my name will be heard no more. [This should be considered a minor victory.]", "finehouse", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WORKVICTORYB");
        if (endingFlag == "workvictorymarriage")
            return new Ending(endingFlag, "A NEW LIFE",
                "I was poor, and now I am rich. I was something common, and now I am something rare. On summer mornings the hills outside my bedroom window are green with sun and on winter mornings they are white with snow and over all of it is the sound of " +
                "the laughter of our heirs, and the sound of the wind. The wind rattles the casements and runs into the hills and is gone. [This should be considered a minor victory.]", "moonlitcastle", EndingFlavour.Grand, "DramaticLight", "A_ENDING_WORKVICTORYMARRIAGE");

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
