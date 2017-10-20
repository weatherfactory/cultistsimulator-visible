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
    /// <summary>
    /// XTriggers allow the triggering aspect to transform the element into something else. For example, if the Knock aspect were present, and the element was a locked_box with Knock:open_box,
    /// then the box would become an open_box regardless of what else happened in the recipe.
    /// XTriggers run *before* the rest of the recipe (so if the recipe affected open_box elements but not locked_box elements, those effects would take place if there was a Knock in the mix).
    /// </summary>
    public Dictionary<string, string> XTriggers;
    public string Id { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public List<SlotSpecification> ChildSlotSpecifications { get; set; }
    public bool IsAspect { get; set; }
    public float Lifetime { get; set; }

    /// <summary>
    /// all aspects the element has, *including* the aspect itself as an element
    /// </summary>
    public IAspectsDictionary AspectsIncludingSelf
    {
        get
        {
            IAspectsDictionary aspectsIncludingElementItself =new AspectsDictionary();

            foreach(string k in Aspects.Keys)
                aspectsIncludingElementItself.Add(k,Aspects[k]);

            if (!aspectsIncludingElementItself.ContainsKey(Id))
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
        XTriggers=new Dictionary<string, string>();
    }

    public Boolean HasChildSlots()
    {
        return ChildSlotSpecifications.Count > 0;
    }
}

