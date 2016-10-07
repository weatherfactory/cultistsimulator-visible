using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class StorageSlot : MonoBehaviour
{

    private int quantity;
    private DraggableElementToken draggableElementToken;
    public int Quantity { get { return quantity; }}
    public Element Element { get { return draggableElementToken.Element; } }


    public void Awake()
    {
        draggableElementToken = GetComponentInChildren<DraggableElementToken>();
    }

    public void PopulateSlot(string elementId, int change, ContentManager cm)
    {
        draggableElementToken.PopulateForElementId(elementId,cm);

        quantity = change;
        draggableElementToken.DisplayQuantity(quantity);
    }

    public void ModifyQuantity(int change)
    {
        quantity += change;
        if(quantity<=0)
            Destroy(gameObject);
        draggableElementToken.DisplayQuantity(quantity);
    }



}



    


    
