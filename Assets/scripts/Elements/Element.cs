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
    public List<ChildSlot> ChildSlots { get; set; }


    public Element(string id, string label, string description)
    {
        Id = id;
        Label = label;
        Description = description;
        ChildSlots=new List<ChildSlot>();
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
                Debug.Log(k);
                ChildSlots.Add(new ChildSlot(k));

                Hashtable htThisSlot = htSlots[k] as Hashtable;

                Hashtable htRequired = htThisSlot["required"] as Hashtable;
                if(htRequired!=null)
                { 
                foreach (string rk in htRequired.Keys)
                ChildSlots[ChildSlots.Count-1].Required.Add(rk,1);
                }
                Hashtable htForbidden = htThisSlot["forbidden"] as Hashtable;
                if(htForbidden!=null)
                { 
                foreach (string fk in htForbidden.Keys)
                    ChildSlots[ChildSlots.Count - 1].Forbidden.Add(fk, 1);
                }
            }
        }

    }
}

public class ChildSlot
{
    public string Label { get; set; }
    public Dictionary<string, int> Required { get; set; }
    public Dictionary<string, int> Forbidden { get; set; }

    public ChildSlot(string label)
    {
        Label = label;
        Required=new Dictionary<string, int>();
        Forbidden=new Dictionary<string, int>();
    }
}