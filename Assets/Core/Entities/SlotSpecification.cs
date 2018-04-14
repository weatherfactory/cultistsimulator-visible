using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Noon;

/// <summary>
/// Entity class: a child slot for an element
/// </summary>
public class SlotSpecification
{
    public string Id { get; set; }
    public string Label { get; set; }
    public string ForVerb { get; set; }

    /// <summary>
    /// currently, this is only used by the primary slot specification
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The element in this slot must possess at least one of these aspects
    /// </summary>
    public IAspectsDictionary Required { get; set; }
    /// <summary>
    /// The element in this slot cannot possess any of these aspects
    /// </summary>
    public IAspectsDictionary Forbidden { get; set; }
    /// <summary>
    /// A Greedy slot will find a card on the desktop that matches its specification, and insert it.
    /// </summary>
    public bool Greedy = false;
    /// <summary>
    /// A Consuming slot will destroy its contents when a recipe begins
    /// </summary>
    public bool Consumes = false;
    private const string PRIMARY_SLOT="primary";

    public SlotSpecification(string id)
    {
        Id = id;
        Label = id;
        Required = new AspectsDictionary();
        Forbidden = new AspectsDictionary();
        ForVerb = string.Empty;
    }

    public static SlotSpecification CreatePrimarySlotSpecification()
    {
        var spec=new SlotSpecification(PRIMARY_SLOT);
        spec.Label = "";
        spec.Description = "'It is the empty space which makes the bowl useful.' - Lao Tzu. [Drop any card here.]";
        return spec;
    }

    public SlotMatchForAspects GetSlotMatchForAspects(IAspectsDictionary aspects)
    {

        foreach (string k in Forbidden.Keys)
        {
            if(aspects.ContainsKey(k))
            {
                return new SlotMatchForAspects(new List<string>() {k}, SlotMatchForAspectsType.ForbiddenAspectPresent);
            }
        }
        
        //passed the forbidden check
        //if there are no specific requirements, then we're now okay
        if(Required.Keys.Count==0)
            return new SlotMatchForAspects(null,SlotMatchForAspectsType.Okay);


        foreach (string k in Required.Keys) //only one needs to match
        {
            if (aspects.ContainsKey(k))
            { 
            int aspectAtValue = aspects[k];
            if (aspectAtValue >= Required[k])
                return new SlotMatchForAspects(null, SlotMatchForAspectsType.Okay);
            }
        }


        return new SlotMatchForAspects(Required.Keys, SlotMatchForAspectsType.RequiredAspectMissing);


    }
}





public enum SlotMatchForAspectsType
{
Okay,
    RequiredAspectMissing,
    ForbiddenAspectPresent
}