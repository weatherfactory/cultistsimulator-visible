#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementOverview : MonoBehaviour, IStacksChangeSubscriber {


    [SerializeField] StatusBarElementCount[] elementCounts;
    private Legacy _activeLegacy;
    private ICompendium _compendium;
    private const int MAX_ELEMENTS = 4;

    public void Initialise(Legacy activeLegacy, StackManagersCatalogue elementStacksCatalogue,ICompendium compendium) {
        //ensure we get updates about stack changes
        elementStacksCatalogue.Subscribe(this);

        _activeLegacy = activeLegacy;
        _compendium = compendium;

        for (int a = 0; a < MAX_ELEMENTS; a++)
        {
            elementCounts[a].gameObject.SetActive(false);
        }
        if (_activeLegacy.StatusBarElements.Count == 0)
        {
            activeLegacy.StatusBarElements.Add("health");
            activeLegacy.StatusBarElements.Add("passion");
            activeLegacy.StatusBarElements.Add("reason");
            activeLegacy.StatusBarElements.Add("funds");
        }

        if(_activeLegacy.StatusBarElements.Count>MAX_ELEMENTS)
            NoonUtility.Log("Too many status bar elements specified - not all will appear (" + _activeLegacy.StatusBarElements.Count + " elements specified)");

        int i = 0;
        
        foreach (var e in _activeLegacy.StatusBarElements)
        {
            elementCounts[i].PopulateWithElement(compendium.GetEntityById<Element>(e));
            i++;
            if (i >= MAX_ELEMENTS)
                break;
        }

    }

    public void NotifyStacksChanged() {
        UpdateDisplay();
    }


    public void UpdateDisplay()
    {
        // now called from the notification chain in StacksCatalogue
        var ttm = Registry.Get<TabletopManager>();
        var aspectsInContext = ttm.GetAspectsInContext(new AspectsDictionary());
  
        for (int i = 0; i <= 3; i++)
        {
            if(elementCounts[i].isActiveAndEnabled && elementCounts[i].Element!=null)
            { 
                string elementId = elementCounts[i].Element.Id;
       
               // int count = GetCountForElement(elementId,stacksCatalogue);
               int count = aspectsInContext.AspectsExtant.AspectValue(elementId);
                elementCounts[i].SetCount(count);

                if (elementId=="health")
                    elementCounts[i].SetFatiguedCount(aspectsInContext.AspectsExtant.AspectValue("fatigue"));
                //elementCounts[i].SetFatiguedCount(GetCountForElement("fatigue",stacksCatalogue));
                else if (elementId == "passion")
                    elementCounts[i].SetFatiguedCount(aspectsInContext.AspectsExtant.AspectValue("passionexhausted") + aspectsInContext.AspectsExtant.AspectValue("passionexhausted"));
                //elementCounts[i].SetFatiguedCount(GetCountForElement("passionexhausted", stacksCatalogue) + GetCountForElement("disillusionment", stacksCatalogue));
                else if (elementId == "reason")
                    elementCounts[i].SetFatiguedCount(aspectsInContext.AspectsExtant.AspectValue("concentration"));
                //elementCounts[i].SetFatiguedCount(GetCountForElement("concentration", stacksCatalogue));
                else
                    elementCounts[i].SetFatiguedCount(0);
            }

        }


    }

    //private int GetCountForElement(string forElementId, StackManagersCatalogue stacksCatalogue)
    //{
    //    int count = 0;
    //    foreach (var stackManager in stacksCatalogue.GetRegisteredStackManagers())
    //    {

    //        count += stackManager.GetCurrentElementQuantity(forElementId);

    //    }

    //    return count;
    //}


}
