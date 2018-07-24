#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
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

    public void Initialise(StackManagersCatalogue elementStacksCatalogue) {
        //ensure we get updates about stack changes
        elementStacksCatalogue.Subscribe(this);
        elementCounts[0].PopulateImageForElement("health");
        elementCounts[1].PopulateImageForElement("passion");
        elementCounts[2].PopulateImageForElement("reason");
        elementCounts[3].PopulateImageForElement("funds");
    }

    public void NotifyStacksChanged() {
        UpdateDisplay();
    }


    public void UpdateDisplay()
    {
        
        // now called from the notification chain in StacksCatalogue
        var stacksCatalogue = Registry.Retrieve<StackManagersCatalogue>();

        int healthCount = GetCountForElement("health",stacksCatalogue);
        int fatigueCount = GetCountForElement("fatigue", stacksCatalogue);
        elementCounts[0].SetCount(healthCount);
        elementCounts[0].SetFatiguedCount(fatigueCount);


        int passionCount = GetCountForElement("passion", stacksCatalogue);
        int passionExhaustedCount = GetCountForElement("passionexhausted", stacksCatalogue) + GetCountForElement("disillusionment", stacksCatalogue); ;
        elementCounts[1].SetCount(passionCount);
        elementCounts[1].SetFatiguedCount(passionExhaustedCount);


        int reasonCount = GetCountForElement("reason", stacksCatalogue);
        int reasonExhaustedCount = GetCountForElement("concentration",stacksCatalogue);
        elementCounts[2].SetCount(reasonCount);
        elementCounts[2].SetFatiguedCount(reasonExhaustedCount);



        int fundsCount = GetCountForElement("funds", stacksCatalogue);
        elementCounts[3].SetCount(fundsCount);
        elementCounts[3].SetFatiguedCount(0);

    }

    private int GetCountForElement(string forElementId, StackManagersCatalogue stacksCatalogue)
    {
        int count = 0;
        foreach (var stackManager in stacksCatalogue.GetRegisteredStackManagers())
        {

            count += stackManager.GetCurrentElementQuantity(forElementId);

        }

        return count;
    }


}
