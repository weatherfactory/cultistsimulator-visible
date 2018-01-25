#pragma warning disable 0649
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

    [SerializeField] Image[] elementImages;
    [SerializeField] TextMeshProUGUI[] elementCounts;

    public void Initialise(StackManagersCatalogue elementStacksCatalogue) {
        //ensure we get updates about stack changes
        elementStacksCatalogue.Subscribe(this);
    }

    public void NotifyStacksChanged() {
        UpdateDisplay();
    }

    // A total number of 4 supported by the bar currently. Not more, not fewer
    private string[] overviewElementIds = new string[] {
        "health", "passion", "reason", "funds"
    };

    public void SetElement(int i, string elementId, int count) {
        if (i < 0 || i >= elementImages.Length || i >= elementCounts.Length) {
            Debug.LogWarning("Can not display Resource for index " + i);
            return;
        }

        var sprite = ResourcesManager.GetSpriteForElement(elementId);
        var color = (count == 0 ? Color.red : Color.white);

        elementImages[i].sprite = sprite;
        elementCounts[i].text = count.ToString();
        elementCounts[i].color = color;
    }

    public void UpdateDisplay()
    {
        // now called from the notification chain in StacksCatalogue
        var stacksCatalogue = Registry.Retrieve<StackManagersCatalogue>();

        for (int i = 0; i < overviewElementIds.Length; i++)
        {
            int count = 0;
            foreach (var stackManager in stacksCatalogue.GetRegisteredStackManagers())
            {
                string countElementId = overviewElementIds[i];
                count += stackManager.GetCurrentElementQuantity(countElementId);
                SetElement(i, countElementId, count);
            }
        }

    }


}
