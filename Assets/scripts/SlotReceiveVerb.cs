using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SlotReceiveVerb : BoardMonoBehaviour, IDropHandler {


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
        if (BM.itemBeingDragged.tag=="Verb")
       {
           if (itemInSlot && itemInSlot.GetComponent<DraggableToken>())
           {
               DraggableToken itemInSlotComponent = itemInSlot.GetComponent<DraggableToken>();
                itemInSlot.transform.SetParent(itemInSlotComponent.originSlot);
            }

            BM.itemBeingDragged.transform.SetParent(transform);
         BM.MakeFirstSlotAvailable(transform.localPosition);
           
       }
    }
}
