using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Element
{
    public Dictionary<string, int> Aspects;
    public string Id { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }


    public Element(string id, string label, string description)
    {
        Id = id;
        Label = label;
        Description = description;

    }

    public void AddAspectsFromHashtable(Hashtable htAspects)
    {
        Aspects = Noon.Utility.ReplaceConventionValues(htAspects);
    }
}