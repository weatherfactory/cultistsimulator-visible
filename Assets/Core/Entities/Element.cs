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
    public Dictionary<string, List<MorphDetails>> XTriggers;

    private string _label="";
    private string _description="";
    public string Id { get; set; }
    private string _icon;
    private bool _resaturate;

    public string Label
    {
        get { return _label; }
        set { _label = value ?? ""; }
    }
    

    public string Icon
    {
        get { return _icon; }
    }

    //if true, when the card decays it should become more, rather than less saturated with colour (eg Fatigue->Health)
    public bool Resaturate
    {
        get { return _resaturate; }
        set { _resaturate = value; }
    }


    public string Description
    {
        get { return _description; }
        set { _description = value ?? ""; }
    }

    private int AnimFrames { get; set; } //no longer used; leaving it in here in case we find we need it after all
    public List<SlotSpecification> ChildSlotSpecifications { get; set; }
    public bool IsAspect { get; set; }
    public bool IsHidden { get; set; } //use with caution! this is intended specifically for uniqueness group aspects. It will only work on aspect displays, anyhoo

    /// <summary>
    /// 
    /// </summary>
    public string OverrideVerbIcon { get; set; }

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

    public string UniquenessGroup { get; set; }

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
        XTriggers=new Dictionary<string, List<MorphDetails>>();

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


public class MorphDetails
{
    private readonly bool _additional;
    private readonly string _id;
    private readonly int _chance;
    
    public string Id
    {
        get { return _id; }
    }

    public int Chance
    {
        get { return _chance; }
    }

    public bool Additional
    {
        get { return _additional; }
    }
    public MorphDetails(string id)
    {
        _id = id;
        _additional = false;
        _chance = 100;

    }

    public MorphDetails(string id, int chance, bool additional)
    {
        _additional = additional;
        _id = id;
        _chance = chance;

    }
}