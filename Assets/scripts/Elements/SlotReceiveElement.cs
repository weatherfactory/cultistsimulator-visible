using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.EventSystems;

public class SlotReceiveElement : BoardMonoBehaviour, IDropHandler
{
    public GameObject ChildSlotOrganiser;

    private GameObject ItemInSlot
    {
        get
        {
            if (transform.childCount > 0)
                return transform.GetChild(0).gameObject;
            else

                return null;
        }
    }

    private DraggableElementToken GetTokenInSlot()
    {
        if (ItemInSlot == null)
            return null;
        else
            return  ItemInSlot.GetComponent<DraggableElementToken>();
    }

    public void ClearThisSlot()
    {
        if (ChildSlotOrganiser != null)
            ChildSlotOrganiser.GetComponent<ChildSlotOrganiser>().Remove(); //this is potentially recursive
        
        DraggableElementToken tokenToRemove = GetTokenInSlot();
        if(tokenToRemove!=null)
            tokenToRemove.ReturnToOrigin();
    }



    public void OnDrop(PointerEventData eventData)
    {
        if (BM.CurrentDragItem.tag == "Element")
        { 
            
            if (GetTokenInSlot()!=null)
            {
              ClearThisSlot();
            }
            DraggableElementToken draggableElementToken = BM.CurrentDragItem.GetComponent<DraggableElementToken>();
            draggableElementToken.transform.SetParent(transform);
            BM.UpdateAspectDisplay();

            if(draggableElementToken.HasChildSlots())
                BM.AddChildSlots(gameObject.GetComponent<SlotReceiveElement>(), draggableElementToken);


        }
    }


}
