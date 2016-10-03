using UnityEngine;
using System;

using System.Collections;

using JetBrains.Annotations;
using OrbCreationExtensions;
using UnityEditor.iOS.Xcode;

public class ContentManager : Singleton<ContentManager>
{

    private const string CONST_CONTENTDIR = "content/";
    private const string CONST_ELEMENTS = "ELEMENTS";
    private const string CONST_ID = "id";
    private const string CONST_LABEL = "label";
    private const string CONST_DESCRIPTION = "description";

    private Hashtable htElements;

    ///NOT CURRENTLY USED
    public Hashtable ImportVerbs()
    {
        string json = Resources.Load<TextAsset>("content/verbs").text;
        return SimpleJsonImporter.Import(json);
    }

    public void ImportElements()
    {
        string json = Resources.Load<TextAsset>(CONST_CONTENTDIR + CONST_ELEMENTS).text;
        htElements = SimpleJsonImporter.Import(json);
    }

    public Element PopulateElementForId(string id)
    {
        //find element in json
        //find description in element
     Hashtable htElement=htElements.GetNodeWithProperty(CONST_ID, id);
        Element element=new Element(id,
           htElement.GetString(CONST_LABEL),
            htElement.GetString(CONST_DESCRIPTION));
        return element;
    }
}
