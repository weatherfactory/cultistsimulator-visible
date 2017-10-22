using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementOverview : MonoBehaviour {

    [SerializeField] Image[] elementImages;
    [SerializeField] TextMeshProUGUI[] elementCounts;

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

}
