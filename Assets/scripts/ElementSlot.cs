using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ElementSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private Element element;
    private int quantity;
    public string ElementId { get { return element.Id; } }
    public string Description { get { return element.Description; } }
    public int Quantity { get { return quantity; }}
    
    public ElementSlot()
    {
        element=new Element("","","");
    }

    public void PopulateSlotValues(string elementId, int change, ContentManager cm)
    {

        element = cm.PopulateElementForId(elementId);
        DisplayName(element);
        DisplayIcon(element);
        quantity = change;
        DisplayQuantity(quantity);
    }

    public void ModifyQuantity(int change)
    {
        quantity += change;
        if(quantity<=0)
            Destroy(gameObject);
        DisplayQuantity(quantity);
    }


    private void DisplayName(Element e)
    {
        Text nameText = GetComponentsInChildren<Text>().Single(t=>t.name=="txtElementName");
        nameText.text = e.Label;
    }

    private void DisplayIcon(Element e)
    {
        Image elementImage = GetComponentsInChildren<Image>().Single(i => i.name=="imgElementIcon");
        Sprite elementSprite = Resources.Load<Sprite>("FlatIcons/png/32px/" + e.Id);
        elementImage.sprite = elementSprite;
    }

    private void DisplayQuantity(int quantity)
    {
        Text quantityText = GetComponentsInChildren<Text>().Single(t => t.name == "txtQuantity");
        quantityText.text = quantity.ToString();

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
    public Dictionary<string,int> Aspects;
    public string Id { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }


    public Element(string id, string label, string description)
    {
        Id = id;
        Label = label;
        Description = description;
       
    }

    

    public void AddAspects(Hashtable htAspects)
    {
        Aspects = Noon.Utility.ReplaceConventionValues(htAspects);
    }
    
}