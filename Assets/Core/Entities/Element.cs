using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using UnityEngine;


///this is a reference object stored in Compendium where we indicate aspects, child slots and other properties
public class Element
{
    public IAspectsDictionary Aspects;
    public string Id { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public List<SlotSpecification> ChildSlotSpecifications { get; set; }
    public bool IsAspect { get; set; }
    public float Lifetime { get; set; }

    public IAspectsDictionary AspectsIncludingSelf
    {
        get
        {
            IAspectsDictionary aspectsIncludingElementItself =new AspectsDictionary();
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
        ChildSlotSpecifications=new List<SlotSpecification>();
        Aspects=new AspectsDictionary();
    }

    public Boolean HasChildSlots()
    {
        return ChildSlotSpecifications.Count > 0;
    }
}

