using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
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
    Recipe GetFirstRecipeForAspectsWithVerb(IDictionary<string, int> aspects, string verb, Character character);
    List<Recipe> GetAllRecipesAsList();
    Recipe GetRecipeById(string recipeId);
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);
    List<IVerb> GetAllVerbs();
    IVerb GetVerbById(string verbId);
    Ending GetEndingById(string endingFlag);
    IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command);

    List<Legacy> GetAllLegacies();
    Legacy GetLegacyById(string legacyId);
}

public class Compendium : ICompendium
{
    private List<Recipe> _recipes;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, IVerb> _verbs;
    private Dictionary<string, Legacy> _legacies;


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

    public void UpdateLegacies(Dictionary<string, Legacy> legacies)
    {
        _legacies = legacies;
    }


    public Recipe GetFirstRecipeForAspectsWithVerb(IDictionary<string, int> aspects, string verb, Character character)
    {
        //for each recipe,
        foreach (var recipe in _recipes.Where(r=>r.ActionId==verb && r.Craftable && !character.HasExhaustedRecipe(r)))
        {
            //for each requirement in recipe, check if that aspect does *not* exist at that level in Aspects
            bool matches = true;
            foreach (string requirementId in recipe.Requirements.Keys)
            {
                if (!aspects.Any(a => a.Key == requirementId && a.Value >= recipe.Requirements[requirementId]))
                    matches = false;
            }
            //if none fail, return that recipe
            if (matches) return recipe;
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
       var createdVerb = new CreatedVerb(command.Recipe.ActionId, command.Recipe.Label,
      command.Recipe.Description);
        return createdVerb;
    }



    public Ending GetEndingById(string endingFlag)
    {
        if (endingFlag == "introguilt")
            return new Ending(endingFlag, "A Warning",
                "" +
                " that I could have done *everything* I should not.", ""
            );

        if (endingFlag == "introdelight")
            return new Ending(endingFlag, "A Temptation",
                "" +
                " that I could have done *everything* I should not.", ""
            );

        if (endingFlag=="deathofthebody")
            return new Ending(endingFlag, "MY BODY IS DEAD",
                "Where will they find me? I am not here. In the end, my strength was insufficient to sustain my failing heart. [I was starving, and I had no Health remaining. I should have " +
                "ensured I had money to purchase essentials; I could have used Dream to rest and recover from my weakness.]","");
        if (endingFlag == "powerminor")
            return new Ending(endingFlag,"WHAT IS BELOW CAN'T ESCAPE WHAT IS ABOVE",
                "The Red Grail is the Hour of blood and of birth. It has touched me, and I've gained a little of its power. If I had more time, I could" +
                "draw disciples to me; grow fierce with blood and delight; be the herald of a new age; use that power to ascend a secret throne, one day.  [By the standards of this  " +
                "prologue, this is a victory: but it was only a taste. In a full game, there would be much further to go.]", "");

        if (endingFlag == "enlightenmentminor")
            return new Ending(endingFlag, "EACH HOUR HAS ITS COLOUR. EACH FLAME HAS ITS FUEL",
                "I've walked behind the Watchman: I've seen his shadow on the stone. This is the first step in understanding the shaping of fate. If I had more time, I could learn to walk the Mansus; gather disciples;" +
                "find the star-shattered fane; watch the Hours walk; grow Long. [By the standards of this  " +
                "prologue, this is a victory: but it was only a taste. In a full game, there would be much further to go.]", "");

        if (endingFlag == "arrest") 
            return new Ending(endingFlag, "Bars across the Sun",
                "The nature of my crimes was vague, and the trial contentious. But there is a consensus that I have done something I should not. I wish it could have been different. I wish " +
                " that I could have done *everything* I should not.", ""
                );

        if (endingFlag == "workvictory")
            return new Ending(endingFlag, "This is pleasant",
                "I have my fire, my books, my clock, my window on the world where they do other things. I could have been unhappy. I'm not unhappy. This was a successful life, and when it is" +
                "over the sweet earth will fill my mouth, softer than splinters. [By the standards of this  " +
                "prologue, this might be considered a victory. you could carry the legacy through to another character.]", "");

        return new Ending("default", "IT IS FINISHED","This one is done.", "");
    }
}
