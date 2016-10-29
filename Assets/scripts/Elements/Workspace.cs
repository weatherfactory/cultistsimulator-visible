using System;
using UnityEngine;
using System.Collections;

public class Workspace : BoardMonoBehaviour
{

    [SerializeField] private SlotReceiveVerb VerbSlot;
    private GameObject RootElementSlot;

    public bool IsRootElementPresent { get { return RootElementSlot != null; } }
    public string GetCurrentVerbId()
    {
        return VerbSlot.GetCurrentVerbId();
    }

    public DraggableElementToken[] GetCurrentElements()
    {
        return GetComponentsInChildren<DraggableElementToken>();
    }

    public void MakeFirstSlotAvailable(Vector3 governorPosition,GameObject prefabEmptyElementSlot)
    {
        if(!IsRootElementPresent)
        { 
        int governedStepRight = 50;
        int nudgeDown = -10;
       RootElementSlot = Instantiate(prefabEmptyElementSlot, transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governorPosition.x + governedStepRight, governorPosition.y + nudgeDown);
        RootElementSlot.transform.localPosition = newSlotPosition;
        }
    }

    public void ReturnEverythingToOrigin()
    {
        
        SlotReceiveElement[] slots = this.GetComponentsInChildren<SlotReceiveElement>();
        foreach(SlotReceiveElement slot in slots)
        { 
            slot.ClearThisSlot();
            BM.ExileToLimboThenDestroy(slot.gameObject);
        }
        SlotReceiveVerb verbSlot = this.GetComponentInChildren<SlotReceiveVerb>();
        if (verbSlot != null)
            verbSlot.ClearThisSlot();
           
    }

    public void ConsumeElements()
    {
        DraggableElementToken[] eTokens = this.GetComponentsInChildren<DraggableElementToken>();

        foreach (DraggableElementToken eToken in eTokens)
            if (!eToken.HasChildSlots())
            {
                ConsumeElementToken(eToken);
            }
        BM.ClearWorkspace();
    }

    private void ConsumeElementToken(DraggableElementToken eToken)
    {
        BM.ElementIntoStockpile(eToken.Element.Id, 1);
        BM.ModifyElementQuantity(eToken.Element.Id, -1);
        eToken.SetQuantity(0);
    }


    /// <summary>
    /// finds a slot with this element id; removes the token and clears the slot
    /// This is for when an element has been destroyed elsewhere.
    /// </summary>
    /// <returns>true if an element has been removed/destroyed; false otherwise</returns>
    public Boolean ElementInWorkspaceDestroyed(string elementId)
    {
        SlotReceiveElement[] slots = this.GetComponentsInChildren<SlotReceiveElement>();
        foreach (SlotReceiveElement slot in slots)
        {
            DraggableElementToken eToken = slot.GetTokenInSlot();
            if(eToken!=null && eToken.Element.Id==elementId)
            { 
                eToken.SetQuantity(0);
                slot.ClearThisSlot();
                return true;
            }
        }
        return false;
    }
}
