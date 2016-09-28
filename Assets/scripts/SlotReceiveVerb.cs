using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.EventSystems;

public class SlotReceiveVerb : MonoBehaviour, IDropHandler {


    public GameObject itemInSlot
    {
        get
        {
            if (transform.childCount > 0)
                return transform.GetChild(0).gameObject;
            else
                return null;

        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if (DraggableToSlot.itemBeingDragged.tag=="Verb")
       {
           if (itemInSlot && itemInSlot.GetComponent<DraggableToSlot>())
           {
               DraggableToSlot itemInSlotComponent = itemInSlot.GetComponent<DraggableToSlot>();
                itemInSlot.transform.SetParent(itemInSlotComponent.originSlot);
            }
            
            DraggableToSlot.itemBeingDragged.transform.SetParent(transform);
            BoardManager.SetFirstElementVisibility(true);
           
       }
    }
}
