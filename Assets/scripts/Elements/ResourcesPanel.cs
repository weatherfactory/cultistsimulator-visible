using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ResourcesPanel : BoardMonoBehaviour,IDropHandler {

    public void OnDrop(PointerEventData eventData)
    {
        DraggableElementToken draggableElementToken = BM.CurrentDragItem.GetComponent<DraggableElementToken>();
        if (draggableElementToken!=null)
           
           BM.ModifyElementQuantityOnBoard(draggableElementToken.Element.Id,draggableElementToken.Quantity);
            BM.SendToLimbo(BM.CurrentDragItem.gameObject);
            GameObject.Destroy(BM.CurrentDragItem.gameObject);

            BM.UpdateAspectDisplay();

        }
    }

