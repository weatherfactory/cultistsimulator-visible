using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.EventSystems;

public class SlotReceiveElement : BoardMonoBehaviour, IDropHandler
{
    public GameObject ChildSlotOrganiser;

    private DraggableToken GetTokenInSlot()
    {
        if (ItemInSlot == null)
            return null;
        else
            return  ItemInSlot.GetComponent<DraggableToken>();
    }

    public void ClearThisSlot()
    {
        if (ChildSlotOrganiser != null)
            ChildSlotOrganiser.GetComponent<ChildSlotOrganiser>().Remove(); //this is potentially recursive
        
        DraggableToken tokenToRemove = GetTokenInSlot();
        if(tokenToRemove!=null)
            tokenToRemove.ReturnToOrigin();
    }

    public GameObject ItemInSlot
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
        if (BM.CurrentDragItem.tag == "Element")
        { 
            
            if (ItemInSlot && ItemInSlot.GetComponent<DraggableToken>())
            {
              ClearThisSlot();
            }
            DraggableElementToken draggableElementToken = BM.CurrentDragItem.GetComponent<DraggableElementToken>();
            draggableElementToken.transform.SetParent(transform);
            BM.UpdateAspectDisplay();

            if(draggableElementToken.HasChildSlots())
                BM.CreateChildSlotsOrganiser(gameObject.GetComponent<SlotReceiveElement>(), draggableElementToken);


        }
    }


}
