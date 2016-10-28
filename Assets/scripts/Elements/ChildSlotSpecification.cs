using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ChildSlotSpecification
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

    public ChildSlotSpecification(string label)
    {
        Label = label;
        Required = new Dictionary<string, int>();
        Forbidden = new Dictionary<string, int>();
    }

    public ElementSlotMatch GetElementSlotMatchFor(Element element)
    {

        foreach (string k in Forbidden.Keys)
        {
            if(element.Aspects.ContainsKey(k))
            {
                return new ElementSlotMatch(new List<string>() {k}, ElementSlotSuitability.ForbiddenAspectPresent);
            }
        }
        
        if(Required.Keys.Count==0)
            return new ElementSlotMatch(null,ElementSlotSuitability.Okay);

        foreach (string k in Required.Keys) //only one needs to match
        {
            if (element.Aspects.ContainsKey(k))
                return new ElementSlotMatch(null, ElementSlotSuitability.Okay);
        }

        return new ElementSlotMatch(Required.Keys, ElementSlotSuitability.RequiredAspectMissing);


    }
}

public class ElementSlotMatch
{
    public  IEnumerable<string> ProblemAspectIds=new List<string>();
    public ElementSlotSuitability ElementSlotSuitability { get; set; }

    public ElementSlotMatch(IEnumerable<string> problemAspectIds, ElementSlotSuitability esm)
    {
        ProblemAspectIds = problemAspectIds;
        ElementSlotSuitability = esm;
    }    
}

public enum ElementSlotSuitability
{
Okay,
    RequiredAspectMissing,
    ForbiddenAspectPresent
}