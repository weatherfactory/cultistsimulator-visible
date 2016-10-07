﻿using UnityEngine;
using System;

using System.Collections;

using JetBrains.Annotations;
using OrbCreationExtensions;


public class ContentManager : Singleton<ContentManager>
{

    private const string CONST_CONTENTDIR = "content/";
    private const string CONST_ELEMENTS = "elements";
    private const string CONST_RECIPES = "recipes";
    private const string CONST_ID = "id";
    private const string CONST_LABEL = "label";
    private const string CONST_DESCRIPTION = "description";

    private Hashtable htElements;
    private Hashtable htRecipes;

    public void ImportElements()
    {
        string json = Resources.Load<TextAsset>(CONST_CONTENTDIR + CONST_ELEMENTS).text;
        htElements = SimpleJsonImporter.Import(json);
    }

    public void ImportRecipes()
    {
        string json = Resources.Load<TextAsset>(CONST_CONTENTDIR + CONST_RECIPES).text;
        htRecipes = SimpleJsonImporter.Import(json);
    }

    public Element PopulateElementForId(string id)
    {
        //find Element in json
        //find description in Element
     Hashtable htElement=htElements.GetNodeWithProperty(CONST_ID, id);
       Hashtable htAspects = htElement.GetHashtable("aspects");
        Hashtable htSlots = htElement.GetHashtable("slots");

        Element element=new Element(id,
           htElement.GetString(CONST_LABEL),
            htElement.GetString(CONST_DESCRIPTION));

        element.AddAspectsFromHashtable(htAspects);
        element.AddSlotsFromHashtable(htSlots);

        return element;
    }
}
