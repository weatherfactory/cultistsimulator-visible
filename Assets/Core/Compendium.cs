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
using Assets.CS.TabletopUI;
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
    void ReplaceTokens(IGameEntityStorage populatedCharacter);
}

public class Compendium : ICompendium
{
    private List<Recipe> _recipes;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, IVerb> _verbs;
    private Dictionary<string, Legacy> _legacies;
    private Dictionary<string, IDeckSpec> _decks;


    public void UpdateRecipes(List<Recipe> allRecipes)
    {
        _recipes = allRecipes;

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

    /// <summary>
    
    /// </summary>
    /// <param name="aspects"></param>
    /// <param name="verb"></param>
    /// <param name="character"></param>
    /// <param name="getHintRecipes">If true, get recipes with hintonly=true (and *only* hintonly=true)</param>
    /// <returns></returns>
    public Recipe GetFirstRecipeForAspectsWithVerb(IAspectsDictionary aspects, string verb, Character character,bool getHintRecipes)
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

    public List<Recipe> GetAllRecipesAsList()
    {
        return _recipes;
    }

    public Recipe GetRecipeById(string recipeId)
    {
        if (_recipes.Count(r=> r.Id==recipeId) > 1)
            throw new ApplicationException("Found more than one recipe with id " + recipeId);


        return _recipes.SingleOrDefault(r => r.Id == recipeId);   
    }

    public Dictionary<string,Element> GetAllElementsAsDictionary()
    {
        return _elements;
    }

    public Element GetElementById(string elementId)
    {
        if (!_elements.ContainsKey(elementId))
            return null;
        return _elements[elementId];

    }

    public Boolean IsKnownElement(string elementId)
    {
        return _elements.ContainsKey(elementId);

    }


    public List<IVerb> GetAllVerbs()
    {
        List<IVerb> verbsList = new List<IVerb>();

        foreach (KeyValuePair<string, IVerb> keyValuePair in _verbs)
        {
            verbsList.Add(keyValuePair.Value);
        }

        return verbsList;
    }

    public List<IDeckSpec> GetAllDeckSpecs()
    {
        List<IDeckSpec> decksList = new List<IDeckSpec>();

        foreach (KeyValuePair<string, IDeckSpec> keyValuePair in _decks)
        {
            decksList.Add(keyValuePair.Value);
        }

        return decksList;
    }

    public List<Legacy> GetAllLegacies()
    {
        List<Legacy> legaciesList = new List<Legacy>();

        foreach (KeyValuePair<string, Legacy> keyValuePair in _legacies)
        {
            legaciesList.Add(keyValuePair.Value);
        }

        return legaciesList;
    }

    public IVerb GetVerbById(string verbId)
    {
        if (!_verbs.ContainsKey(verbId))
            return null;

        return _verbs[verbId];
    }

    public IDeckSpec GetDeckSpecById(string id)
    {
        if (!_decks.ContainsKey(id))
            return null;
        return _decks[id];
    }


    public Legacy GetLegacyById(string legacyId)
    {
        if (!_legacies.ContainsKey(legacyId))
            return null;
        return _legacies[legacyId];
    }

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

        if(endingFlag== "minorgrailvictory")
            return new Ending(endingFlag,"THE FEAST OF THE TRUE BIRTH",
                "For a little while I dwell in the high rooms of the Mansus, and then I return through the Tricuspid Gate, and I tear free of the sticky rags of my old flesh. My new body is white without and red within like a sweet fruit." +
                " My limbs are strong as cables and my senses are knife-keen. I will not grow old. I will walk the world in the service of the Grail, feasting, growing. Perhaps I will rebel. Perhaps, one day, I will rise even higher." +
                " [Congratulations on a minor Sensation victory. You have wrestled the game to its knees. There are other paths.] ","redgrail","DramaticLightCool");

        if (endingFlag=="deathofthebody")
            return new Ending(endingFlag, "MY BODY IS DEAD",
                "Where will they find me? I am not here. In the end, my strength was insufficient to sustain my failing heart. [I was starving, and I had no Health remaining. I should have " +
                "ensured I had money to purchase essentials; I could have used Dream to rest and recover from my weakness.]", "suninrags", "DramaticLightEvil");
        if (endingFlag == "despairending")
            return new Ending(endingFlag, "NO MORE",
                "Despair, the wolf that devours thought. Am I alive, or am I dead? It no longer matters. [I allowed the Despair token to reach 3 Dread or Injury.]", "despair", "DramaticLightEvil");
        if (endingFlag == "visionsending")
            return new Ending(endingFlag, "GLORY",
                "First it was the dreams. Then it was the visions. Now it's everything. I no longer have any idea what is real, and what is not. [I allowed the Visions token to reach 3 Fascination.]", "madness", "DramaticLight");

        if (endingFlag == "wintersacrifice")
            return new Ending(endingFlag, "GOING QUIETLY",
                "In the upper room of the house where I am taken, my breath fogs and my eyes grow soft. The light in the room is the light at the end of the sun. I am a beautiful ending.", "suninrags", "DramaticLight");


        if (endingFlag == "enlightenmentminor")
            return new Ending(endingFlag, "EACH HOUR HAS ITS COLOUR. EACH FLAME HAS ITS FUEL",
                "I've walked behind the Watchman: I've seen his shadow on the stone. This is the first step in understanding the shaping of fate. If I had more time, I could learn to walk the Mansus; gather disciples;" +
                "find the star-shattered fane; watch the Hours walk; grow Long. [By the standards of this  " +
                "beta, this is a victory. In the full game, there will be greater victories.]", "notionenlightenment", "");

        if (endingFlag == "arrest") 
            return new Ending(endingFlag, "Bars across the Sun",
                "The nature of my crimes was vague, and the trial contentious. But there is a consensus that I have done something I should not. I wish it could have been different. I wish " +
                " that I could have done *everything* I should not.", "notorious", "DramaticLightEvil"
                );

        if (endingFlag == "workvictory")
            return new Ending(endingFlag, "This is pleasant",
                "I have my fire, my books, my clock, my window on the world where they do other things. I could have been unhappy. I'm not unhappy. This was a successful life, and when it is " +
                "over the sweet earth will fill my mouth, softer than splinters. [This might be considered a victory.]", "insomnia", "DramaticLight");

        return new Ending("default", "IT IS FINISHED","This one is done.", "suninrags", "DramaticLight");
    }

    public void ReplaceTokens(IGameEntityStorage populatedCharacter)
    {
        foreach (var r in _recipes)
        {
            r.Label = populatedCharacter.ReplaceTextFor(r.Label);
            r.StartDescription = populatedCharacter.ReplaceTextFor(r.StartDescription);
            r.Description = populatedCharacter.ReplaceTextFor(r.Description);
        }

        foreach (var k in _elements.Keys)
        {
            var e = _elements[k];
            e.Label = populatedCharacter.ReplaceTextFor(e.Label);
            e.Description = populatedCharacter.ReplaceTextFor(e.Description);

        }
       
    }
}
