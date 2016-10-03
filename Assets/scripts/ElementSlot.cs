using UnityEngine;
using System.Collections;
using System.ComponentModel;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElementSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private Element element;
    public string ElementId { get { return element.Id; } }
    public string Description { get { return element.Description; } }

    public void SetElementValues(string elementId, ContentManager cm)
    {

        element = cm.PopulateElementForId(elementId);
        DisplayName(element);
        DisplayIcon(element);
    }

    private void DisplayName(Element e)
    {
        Text nameText = GetComponentsInChildren<Text>()[0];
        nameText.text = e.Label;
    }

    private void DisplayIcon(Element e)
    {
        Image[] childImages = GetComponentsInChildren<Image>();
        Image elementImage = childImages[2];
        Sprite elementSprite = Resources.Load<Sprite>("FlatIcons/png/32px/" + e.Id);
        elementImage.sprite = elementSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("entered" + element.Description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("exited" + element.Description);
    }
}

public class Element
{


    public Element(string id, string label, string description)
    {
        Id = id;
        this.Label = label;
        this.Description = description;
    }

    public string Id { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }

}