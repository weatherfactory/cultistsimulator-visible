using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ResourcesPanel : BoardMonoBehaviour,IDropHandler {

    public void OnDrop(PointerEventData eventData)
    {
        DraggableElementToken elementTokenDropped = BM.CurrentDragItem.GetComponent<DraggableElementToken>();
        if (elementTokenDropped != null)
        {
            BM.ReturnElementTokenToStorage(elementTokenDropped);

            SlotReceiveElement currentSlot = BM.CurrentDragItem.StartParent.GetComponent<SlotReceiveElement>();
            if (currentSlot!=null)
                currentSlot.ClearThisSlot();
    

        }

    }

    }

