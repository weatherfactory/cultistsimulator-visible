using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


/// <summary>
/// displays all the elements the character currently possesses. This behaviour is quite buggy at the moment;
/// stacks display with 0, stacks move when the player picks up one of their members. That's on the fix list
/// </summary>
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

