using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SlotReceiveElement : BoardMonoBehaviour, IDropHandler {

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
        if (!itemInSlot && DraggableToSlot.itemBeingDragged.tag == "Element")
            DraggableToSlot.itemBeingDragged.transform.SetParent(transform);
    }
}
