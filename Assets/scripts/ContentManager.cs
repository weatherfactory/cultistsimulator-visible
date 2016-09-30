using UnityEngine;
using System;

using System.Collections;

using JetBrains.Annotations;
using UnityEditor.iOS.Xcode;

public class ContentManager : Singleton<ContentManager>
{

    public string Status = "";
    public ElementsCollection AllElements;



    public Hashtable ImportVerbs()
    {
        string json = Resources.Load<TextAsset>("content/verbs").text;
        return SimpleJsonImporter.Import(json);
    }

    public Hashtable ImportElements()
    {
        string json = Resources.Load<TextAsset>("content/elements").text;

        return SimpleJsonImporter.Import(json);
    }
}

public class ElementsCollection
{
    public Hashtable Members { get; set; }

    public ElementsCollection(Hashtable initialMembers)
    {
        Members = initialMembers;
    }
}