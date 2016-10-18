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

    public void AddSlotsFromHashtable(Hashtable slots)
    {
        if(slots!=null)
        { 
        //ultimately, each slot can be unique
        //for(int i=1; i<= Convert.ToInt32(slots["quantity"]);i++ )
        //    ChildSlots.Add(new ChildSlot());
            foreach (string k in slots.Keys)
            {
                Debug.Log(k);
                ChildSlots.Add(new ChildSlot(k));
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
    }
}