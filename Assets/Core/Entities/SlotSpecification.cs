using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Noon;

/// <summary>
/// Entity class: a child slot for an element
/// </summary>
public class SlotSpecification
{
    public string Label { get; set; }
    /// <summary>
    /// The element in this slot must possess at least one of these aspects
    /// </summary>
    public Dictionary<string, int> Required { get; set; }
    /// <summary>
    /// The element in this slot cannot possess any of these aspects
    /// </summary>
    public Dictionary<string, int> Forbidden { get; set; }
    public bool Greedy = false;

    public SlotSpecification(string label)
    {
        Label = label;
        Required = new Dictionary<string, int>();
        Forbidden = new Dictionary<string, int>();
    }

    public SlotMatchForAspects GetSlotMatchForAspects(Dictionary<string,int> aspects)
    {

        foreach (string k in Forbidden.Keys)
        {
            if(aspects.ContainsKey(k))
            {
                return new SlotMatchForAspects(new List<string>() {k}, SlotMatchForAspectsType.ForbiddenAspectPresent);
            }
        }
        
        if(Required.Keys.Count==0)
            return new SlotMatchForAspects(null,SlotMatchForAspectsType.Okay);

        foreach (string k in Required.Keys) //only one needs to match
        {
            if (aspects.ContainsKey(k))
                return new SlotMatchForAspects(null, SlotMatchForAspectsType.Okay);
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