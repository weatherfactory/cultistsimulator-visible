using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


///this is a reference object stored in Compendium where we indicate aspects, child slots and other properties
public class Element
{
    public Dictionary<string, int> Aspects;
    public string Id { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public List<ChildSlotSpecification> ChildSlotSpecifications { get; set; }

    public Dictionary<string, int> AspectsIncludingSelf
    {
        get
        {
            Dictionary<string,int> aspectsIncludingElementItself=new Dictionary<string, int>();
            foreach(string k in Aspects.Keys)
                aspectsIncludingElementItself.Add(k,Aspects[k]);
            if(!aspectsIncludingElementItself.ContainsKey(Id))
                aspectsIncludingElementItself.Add(Id,1);
            
            return aspectsIncludingElementItself;
        }
    }

    public Element(string id, string label, string description)
    {
        Id = id;
        Label = label;
        Description = description;
        ChildSlotSpecifications=new List<ChildSlotSpecification>();
        Aspects=new Dictionary<string, int>();
    }

    public Boolean HasChildSlots()
    {
        return ChildSlotSpecifications.Count > 0;
    }
}

