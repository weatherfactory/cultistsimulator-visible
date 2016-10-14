using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class StorageSlot : BoardMonoBehaviour,IDropHandler
{

    private int quantity;
    private DraggableElementToken elementTokenContained;
    public int Quantity { get { return quantity; }}
    public Element Element { get { return elementTokenContained.Element; } }


    public void Awake()
    {
        elementTokenContained = GetComponentInChildren<DraggableElementToken>();
    }

    public void PopulateSlot(string elementId, int change, ContentRepository cm)
    {
        elementTokenContained.PopulateForElementId(elementId,cm);
        quantity = change;
        elementTokenContained.DisplayQuantity(quantity);
    }

    public void SplitContents(int numberRemoved)
    {
        GameObject newContents=Instantiate(elementTokenContained.gameObject,transform) as GameObject;
        elementTokenContained = newContents.GetComponentInChildren<DraggableElementToken>();
        ModifyQuantity(-numberRemoved);
    }

    public void ModifyQuantity(int change)
    {
        quantity += change;
        if(quantity<=0)
            Destroy(gameObject);
        elementTokenContained.DisplayQuantity(quantity);
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (BM.CurrentDragItem.tag == "Element")
        {

            DraggableElementToken draggableElementToken = BM.CurrentDragItem.GetComponent<DraggableElementToken>();
            ModifyQuantity(1);
            BM.SendToLimbo(BM.CurrentDragItem.gameObject);
            GameObject.Destroy(BM.CurrentDragItem.gameObject);

            BM.UpdateAspectDisplay();
           
            //TODO: filter on correct element, or swap out
            //TODO: clear child slots

        }
    }

}







