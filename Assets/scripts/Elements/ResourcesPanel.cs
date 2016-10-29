using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ResourcesPanel : BoardMonoBehaviour,IDropHandler,IStockpileLocation {

    public void OnDrop(PointerEventData eventData)
    {
        DraggableElementToken elementTokenDropped = BM.CurrentDragItem.GetComponent<DraggableElementToken>();
        if (elementTokenDropped != null)
        {
            BM.ReturnElementTokenToStorage(elementTokenDropped);

            //clear any slot we just took it from
            SlotReceiveElement currentSlot = BM.CurrentDragItem.StartParent.GetComponent<SlotReceiveElement>();
            if (currentSlot!=null)
                currentSlot.ClearThisSlot();
    

        }

    }

    }

public interface IStockpileLocation
{
}

