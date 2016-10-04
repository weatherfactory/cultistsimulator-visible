using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ElementSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{

    private int quantity;
    private DraggableElementDisplay draggableElementDisplay;
    public int Quantity { get { return quantity; }}
    public string ElementId { get { return draggableElementDisplay.Element.Id; } }


    public void Awake()
    {
        draggableElementDisplay = GetComponentInChildren<DraggableElementDisplay>();
    }

    public void PopulateSlot(string elementId, int change, ContentManager cm)
    {
        draggableElementDisplay.PopulateForElementId(elementId,cm);

        quantity = change;
        draggableElementDisplay.DisplayQuantity(quantity);
    }

    public void ModifyQuantity(int change)
    {
        quantity += change;
        if(quantity<=0)
            Destroy(gameObject);
        draggableElementDisplay.DisplayQuantity(quantity);
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("entered" + draggableElementDisplay.Element.Description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
     
    }
}



    


    
