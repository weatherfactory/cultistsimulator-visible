﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using OrbCreationExtensions;
using UnityEngine.Rendering;


public interface ICompendium
{
    void UpdateRecipes(List<Recipe> allRecipes);
    void UpdateElements(Dictionary<string, Element> elements);
    void UpdateVerbs(Dictionary<string, Verb> verbs);
    Recipe GetFirstRecipeForAspectsWithVerb(Dictionary<string, int> aspects,string verb);
    List<Recipe> GetAllRecipesAsList();
    Recipe GetRecipeById(string recipeId);
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);
    List<Verb> GetAllVerbs();
    Verb GetVerbById(string verbId);
}

public class Compendium : ICompendium
{
    private List<Recipe> _recipes;
    private readonly IDice _dice;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, Verb> _verbs;


    public Compendium()
    {

    }

    public void UpdateRecipes(List<Recipe> allRecipes)
    {
        _recipes = allRecipes;

    }

    public void UpdateElements(Dictionary<string, Element> elements)
    {
        _elements = elements;
    }

    public void UpdateVerbs(Dictionary<string, Verb> verbs)
    {
        _verbs = verbs;
    }


    public Recipe GetFirstRecipeForAspectsWithVerb(Dictionary<string, int> aspects,string verb)
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


    public List<Verb> GetAllVerbs()
    {
        List<Verb> verbsList = new List<Verb>();

        foreach (KeyValuePair<string, Verb> keyValuePair in _verbs)
        {
            verbsList.Add(keyValuePair.Value);
        }

        return verbsList;
    }

    public Verb GetVerbById(string verbId)
    {
        if (!_verbs.ContainsKey(verbId))
            throw new ApplicationException("Couldn't find verb id " + verbId);

        return _verbs[verbId];
    }

  
}
