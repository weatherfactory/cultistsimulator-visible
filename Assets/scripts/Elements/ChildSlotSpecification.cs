using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ChildSlotSpecification
{
    public string Label { get; set; }
    public Dictionary<string, int> Required { get; set; }
    public Dictionary<string, int> Forbidden { get; set; }

    public ChildSlotSpecification(string label)
    {
        Label = label;
        Required = new Dictionary<string, int>();
        Forbidden = new Dictionary<string, int>();
    }

    public ElementSlotMatch GetElementSlotMatchFor(Element element)
    {
        foreach (string k in Required.Keys)
        {
            if(!element.Aspects.ContainsKey(k))
                return new ElementSlotMatch(k,ElementSlotSuitability.RequiredAspectMissing);
        }

        foreach (string k in Forbidden.Keys)
        {
            if(element.Aspects.ContainsKey(k))
                return new ElementSlotMatch(k,ElementSlotSuitability.ForbiddenAspectPresent);
        }

        return new ElementSlotMatch(null,ElementSlotSuitability.Okay);
  
    }
}

public class ElementSlotMatch
{
    public string ProblemAspectId { get; set; }
    public ElementSlotSuitability ElementSlotSuitability { get; set; }

    public ElementSlotMatch(string problemAspectId, ElementSlotSuitability esm)
    {
        ProblemAspectId = problemAspectId;
        ElementSlotSuitability = esm;
    }    
}

public enum ElementSlotSuitability
{
Okay,
    RequiredAspectMissing,
    ForbiddenAspectPresent
}