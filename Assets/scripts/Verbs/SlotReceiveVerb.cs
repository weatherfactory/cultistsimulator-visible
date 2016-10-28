using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SlotReceiveVerb : BoardMonoBehaviour, IDropHandler {


    public DraggableVerbToken VerbTokenInSlot
    {
        get
        {
            if (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                return child.gameObject.GetComponent<DraggableVerbToken>();
            }
               
                return null;

        }
    }

    public void ClearThisSlot()
    {
        if(VerbTokenInSlot!=null)
            VerbTokenInSlot.ReturnToOrigin();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (BM.CurrentDragItem.tag=="Verb")
       {
           if (VerbTokenInSlot && VerbTokenInSlot.GetComponent<DraggableVerbToken>())
           {
                DraggableVerbToken itemInSlotComponent = VerbTokenInSlot.GetComponent<DraggableVerbToken>();
                VerbTokenInSlot.transform.SetParent(itemInSlotComponent.OriginTransform);
            }
           BM.VerbAddedToSlot(transform);

        }
    }

    public string GetCurrentVerbId()
    {
        if (VerbTokenInSlot != null)
            return VerbTokenInSlot.Verb.Id;
        else return "";
    }
}
