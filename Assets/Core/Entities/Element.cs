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

    private string _label="";
    private string _description="";
    public string Id { get; set; }
    public string _icon;

    public string Label
    {
        get { return _label; }
        set { _label = value ?? ""; }
    }

    

    public string Icon
    {
        get { return _icon; }
    }


    public string Description
    {
        get { return _description; }
        set { _description = value ?? ""; }
    }

    public int AnimFrames { get; set; }
    public List<SlotSpecification> ChildSlotSpecifications { get; set; }
    public bool IsAspect { get; set; }
    public bool NoArtNeeded { get; set; }
    public float Lifetime { get; set; }
    public string DecayTo { get; set; }
    /// <summary>
    /// Note: the 'additional' value here currently does nothing, but we might later use it to determine whether quantity of an aspect increases chance of induction
    /// </summary>
    public List<LinkedRecipeDetails> Induces { get; set; }
    /// <summary>
    /// If a Unique element is created and another one exists in games, the first one should be quietly removed. When a unique element is created, all references to it should be removed from all decks.
    /// [Note: if a deck resets on exhaustion, the rest will add a new element. So ideally, whenever a card is drawn from a deck, it should be checked for existing uniqueness. Chris' Mansus-management deck is a good place to enforce this if it doesn't already do it..]
    /// </summary>
    public bool Unique { get; set; }

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




    public Element(string id, string label, string description, int animFrames,string icon)
    {
        Id = id;
        Label = label;
        Description = description;
        AnimFrames = animFrames;

        ChildSlotSpecifications=new List<SlotSpecification>();
        Aspects=new AspectsDictionary();
        XTriggers=new Dictionary<string, string>();

        Induces=new List<LinkedRecipeDetails>();

        if (!string.IsNullOrEmpty(icon))
        
            _icon = icon;
                else
            _icon = id;
        
    }


    public Boolean HasChildSlotsForVerb(string forVerb)
    {
        return ChildSlotSpecifications.Any(cs => cs.ForVerb == forVerb || cs.ForVerb == String.Empty);



    }
}

