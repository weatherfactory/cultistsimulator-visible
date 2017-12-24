#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementOverview : MonoBehaviour {

    [SerializeField] Image[] elementImages;
    [SerializeField] TextMeshProUGUI[] elementCounts;

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

    public void UpdateDisplay(IElementStacksManager allCurrentStacksOnTabletop,IEnumerable<SituationController> situations) 
    {
        // TODO: This does a lot of iterating each frame to grab all cards in play. If possible change this to use the planned "lists" instead
        // TODO: This is being called every frame in update, if possible only call it when the stacks have changed? Have a global "elements changed" event to call?

        var draggedElementStack = (DraggableToken.itemBeingDragged != null ? DraggableToken.itemBeingDragged as ElementStackToken : null);
        int count;

        for (int i = 0; i < overviewElementIds.Length; i++)
        {
            count = allCurrentStacksOnTabletop.GetCurrentElementQuantity(overviewElementIds[i]);

            foreach (var sit in situations)
                count += sit.GetElementCountInSituation(overviewElementIds[i]);

            if (draggedElementStack != null && draggedElementStack.Id == overviewElementIds[i])
                count += draggedElementStack.Quantity;

            SetElement(i, overviewElementIds[i], count);
        }
    }

}
