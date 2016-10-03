using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ElementSlot : MonoBehaviour {

    public void SetElementAppearance(string id)
    {
        Text nameText = GetComponentsInChildren<Text>()[0];
        nameText.text = id;
        Image[] childImages = GetComponentsInChildren<Image>();
        Image elementImage = childImages[2];
        Sprite elementSprite = Resources.Load<Sprite>("FlatIcons/png/32px/" + id);
        elementImage.sprite = elementSprite;
    }

}
