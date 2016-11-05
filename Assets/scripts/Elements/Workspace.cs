using System;
using UnityEngine;
using System.Collections;
using Noon;

public class Workspace : BoardMonoBehaviour, IElementSlotEventSubscriber
{

    [SerializeField] private SlotReceiveVerb VerbSlot;
    private GameObject RootElementSlotObj;
    [SerializeField]
    GameObject prefabChildSlotsOrganiser;

    public bool IsRootElementPresent { get { return RootElementSlotObj != null; } }
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
       RootElementSlotObj = Instantiate(prefabEmptyElementSlot, transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governorPosition.x + governedStepRight, governorPosition.y + nudgeDown);
        RootElementSlotObj.transform.localPosition = newSlotPosition;
            SlotReceiveElement elementSlot = RootElementSlotObj.GetComponent<SlotReceiveElement>();
            elementSlot.AddSubscriber(this);
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

    public void ElementAddedToSlot(Element element,SlotReceiveElement slot)
    {
        BM.UpdateAspectDisplay();
        //add child slots for this token's element, if it has any
        if (element.HasChildSlots())
            AddChildSlots(slot);
    }

    private void AddChildSlots(SlotReceiveElement governingSlot)
    {

        float governingSlotHeight = governingSlot.gameObject.GetComponent<RectTransform>().rect.height;
        Transform governingSlotTransform = governingSlot.gameObject.transform;
        governingSlot.DependentChildSlotOrganiser = Instantiate(prefabChildSlotsOrganiser, transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governingSlotTransform.localPosition.x, governingSlotTransform.localPosition.y - governingSlotHeight);
        governingSlot.DependentChildSlotOrganiser.transform.localPosition = newSlotPosition;

        governingSlot.DependentChildSlotOrganiser.GetComponent<ChildSlotOrganiser>().Populate(governingSlot.GetTokenInSlot());

    }

    public void ElementCannotBeAddedToSlot(Element element, ElementSlotMatch match)
    {
        if (match.ElementSlotSuitability == ElementSlotSuitability.ForbiddenAspectPresent)
        {
            string problemAspects = NoonUtility.ProblemAspectsDescription(match);
            BM.Log("Elements with the " + problemAspects + " aspects are unacceptable here. *Unacceptable*.", Style.Assertive);

            return;
        }

        if (match.ElementSlotSuitability == ElementSlotSuitability.RequiredAspectMissing)
        {
            string problemAspects = NoonUtility.ProblemAspectsDescription(match);
            BM.Log("Only elements with the " + problemAspects + " aspects can go here.", Style.Assertive);

            return;
        }
    }
}
