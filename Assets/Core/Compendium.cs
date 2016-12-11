using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using OrbCreationExtensions;
using UnityEngine.Rendering;


public interface ICompendium
{
    void UpdateRecipes(List<Recipe> allRecipes);
    void UpdateElements(Dictionary<string, Element> elements);
    void UpdateVerbs(Dictionary<string, IVerb> verbs);
    Recipe GetFirstRecipeForAspectsWithVerb(IDictionary<string, int> aspects,string verb);
    List<Recipe> GetAllRecipesAsList();
    Recipe GetRecipeById(string recipeId);
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);
    List<IVerb> GetAllVerbs();
    IVerb GetVerbById(string verbId);
    Notification GetNotificationForEndingFlag(string endingFlag);
}

public class Compendium : ICompendium
{
    private List<Recipe> _recipes;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, IVerb> _verbs;


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


    public Recipe GetFirstRecipeForAspectsWithVerb(IDictionary<string, int> aspects,string verb)
    {
        //for each recipe,
        foreach (var recipe in _recipes.Where(r=>r.ActionId==verb && r.Craftable))
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

    public IVerb GetVerbById(string verbId)
    {
        if (!_verbs.ContainsKey(verbId))
            return null;

        return _verbs[verbId];
    }

    public Notification GetNotificationForEndingFlag(string endingFlag)
    {
        //TODO! this is a demo hack
        if(endingFlag=="deathofthebody")
            return new Notification("MY BODY IS DEAD",
                "Where will they find me? I am not here. In the end, my strength was insufficient to sustain my failing heart. [I had no Health remaining.]");

        return new Notification("IT IS FINISHED","This one is done.");
    }
}
