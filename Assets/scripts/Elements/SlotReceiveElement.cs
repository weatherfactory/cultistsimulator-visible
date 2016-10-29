using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.EventSystems;

public class SlotReceiveElement : BoardMonoBehaviour, IDropHandler
{
    /// <summary>
    /// If this slot contains an element which has slots, this slot has a dependent childslotorganiser
    /// </summary>
    public GameObject DependentChildSlotOrganiser;
    /// <summary>
    /// if this slot isn't the primary slot, it will have required and forbidden aspects; the ChildSlotSpecification governs them.
    /// </summary>
    public ChildSlotSpecification  GoverningChildSlotSpecification;

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
        if (DependentChildSlotOrganiser != null)
            DependentChildSlotOrganiser.GetComponent<ChildSlotOrganiser>().Remove(); //this is potentially recursive
        
        DraggableElementToken tokenToRemove = GetTokenInSlot();
        if(tokenToRemove!=null)
            tokenToRemove.ReturnToOrigin();

    }



    public void OnDrop(PointerEventData eventData)
    {
        if (BM.CurrentDragItem.tag == "Element")
        { 
            
            //is there already a token in the slot?
            if (GetTokenInSlot()!=null)
            {
              ClearThisSlot();
            }
            DraggableElementToken draggableElementToken = BM.CurrentDragItem.GetComponent<DraggableElementToken>();

            if(GoverningChildSlotSpecification!=null)
            { 
            //does the element obey restrictions for the current slot?
            ElementSlotMatch elementSlotMatch= GoverningChildSlotSpecification.GetElementSlotMatchFor(draggableElementToken.Element);
            if (elementSlotMatch.ElementSlotSuitability == ElementSlotSuitability.ForbiddenAspectPresent)
            {
                string problemAspects = ProblemAspectsDescription(elementSlotMatch);

                BM.Log("Elements with the " + problemAspects +" aspects are unacceptable here. *Unacceptable*.", Style.Assertive);
                    draggableElementToken.ReturnToOrigin();
                    return;
                }

            if (elementSlotMatch.ElementSlotSuitability == ElementSlotSuitability.RequiredAspectMissing)
            {
                    string problemAspects = ProblemAspectsDescription(elementSlotMatch);

                    BM.Log("Only elements with the " + problemAspects + " aspects can go here.", Style.Assertive);
                    draggableElementToken.ReturnToOrigin();
                return;
            }
            }
            AddElementToSlot(draggableElementToken);
        }
    }

    private static string ProblemAspectsDescription(ElementSlotMatch elementSlotMatch)
    {
        string problemAspects = "";
        foreach (var problemAspectId in elementSlotMatch.ProblemAspectIds)
        {
            if (problemAspects != "")
                problemAspects += " or ";
            problemAspects += problemAspectId;
        }
        return problemAspects;
    }

    private void AddElementToSlot(DraggableElementToken draggableElementToken)
    {
//settle the element in the slot, and update aspects
        draggableElementToken.transform.SetParent(transform);
        BM.UpdateAspectDisplay();

        //add child slots for this token's element, if it has any
        if (draggableElementToken.HasChildSlots())
            BM.AddChildSlots(gameObject.GetComponent<SlotReceiveElement>(), draggableElementToken);
    }
}
