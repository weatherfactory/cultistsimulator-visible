#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Constants.Events;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementOverview : MonoBehaviour, ISphereCatalogueEventSubscriber {


    [SerializeField] StatusBarElementCount[] elementCounts;
    private const int MAX_ELEMENTS = 4;

    public void Start() {

        Registry.Get<SphereCatalogue>().Subscribe(this);

 
        Legacy activeLegacy = Registry.Get<Character>().ActiveLegacy;
        List<string> statusBarElementIds = new List<string>(activeLegacy.StatusBarElements);

        for (int a = 0; a < MAX_ELEMENTS; a++)
        {
            elementCounts[a].gameObject.SetActive(false);
        }
        if (statusBarElementIds.Count == 0)
        {
            statusBarElementIds.Add("health");
            statusBarElementIds.Add("passion");
            statusBarElementIds.Add("reason");
            statusBarElementIds.Add("funds");
        }

        if(statusBarElementIds.Count>MAX_ELEMENTS)
            NoonUtility.Log("Too many status bar elements specified - not all will appear (" + statusBarElementIds.Count + " elements specified)");

        int i = 0;
        
        foreach (var e in statusBarElementIds)
        {
            elementCounts[i].PopulateWithElement(Registry.Get<Compendium>().GetEntityById<Element>(e));
            i++;
            if (i >= MAX_ELEMENTS)
                break;
        }

    }


    public void UpdateDisplay()
    {

        var tc = Registry.Get<SphereCatalogue>();
        var aspectsInContext = tc.GetAspectsInContext(new AspectsDictionary());
  
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

    
    public void NotifyTokensChanged(SphereContentsChangedEventArgs args)
    {
        UpdateDisplay();

    }

    public void OnTokenInteraction(TokenInteractionEventArgs args)
    {
    //
    }
}
