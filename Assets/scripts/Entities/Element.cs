using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

    public void AddAspectsFromHashtable(Hashtable htAspects)
    {
        Aspects = Noon.NoonUtility.ReplaceConventionValues(htAspects);
    }

    public void AddSlotsFromHashtable(Hashtable htSlots)
    {
        if(htSlots!=null)
        { 
        
            foreach (string k in htSlots.Keys)
            {
                ChildSlotSpecifications.Add(new ChildSlotSpecification(k));

                Hashtable htThisSlot = htSlots[k] as Hashtable;

                Hashtable htRequired = htThisSlot["required"] as Hashtable;
                if(htRequired!=null)
                { 
                foreach (string rk in htRequired.Keys)
                ChildSlotSpecifications[ChildSlotSpecifications.Count-1].Required.Add(rk,1);
                }
                Hashtable htForbidden = htThisSlot["forbidden"] as Hashtable;
                if(htForbidden!=null)
                { 
                foreach (string fk in htForbidden.Keys)
                    ChildSlotSpecifications[ChildSlotSpecifications.Count - 1].Forbidden.Add(fk, 1);
                }
            }
        }

    }
}

