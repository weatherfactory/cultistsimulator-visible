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

    public void EmptySlot()
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
        if (BM.itemBeingDragged.tag == "Element")
        { 
            
            if (ItemInSlot && ItemInSlot.GetComponent<DraggableToken>())
            {
              EmptySlot();
            }
            DraggableElementToken draggableElementToken = BM.itemBeingDragged.GetComponent<DraggableElementToken>();
            draggableElementToken.transform.SetParent(transform);
            BM.UpdateAspectDisplay();

            if(draggableElementToken.HasChildSlots())
                BM.CreateChildSlotsOrganiser(gameObject.GetComponent<SlotReceiveElement>(), draggableElementToken);


        }
    }
}
