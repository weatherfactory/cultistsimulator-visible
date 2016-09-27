using UnityEngine;
using System.Collections;
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
        if (!itemInSlot && DragHandler.itemBeingDragged.tag=="Verb")
            DragHandler.itemBeingDragged.transform.SetParent(transform);
    }
}
