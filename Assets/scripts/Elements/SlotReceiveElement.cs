using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Assets.scripts.Interfaces;
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
    private List<IElementSlotEventSubscriber> _subscribers=new List<IElementSlotEventSubscriber>();

    private GameObject ItemInSlot
    {
        get
        {
            if (transform.childCount > 0)
                return transform.GetChild(0).gameObject;

                return null;
        }
    }

    public DraggableElementToken GetTokenInSlot()
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

    public void AddSubscriber(IElementSlotEventSubscriber subscriber)
    {
        if(!_subscribers.Contains(subscriber))
        _subscribers.Add(subscriber);
    }


    public void OnDrop(PointerEventData eventData)
    {
        DraggableElementToken draggableElementToken = BM.CurrentDragItem.GetComponent<DraggableElementToken>();
        if(draggableElementToken!=null)
        { 
            
            //is there already a token in the slot?
            if (GetTokenInSlot()!=null)
            {
              ClearThisSlot();
            }
           

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
        
        PublishElementAddedToSlot(draggableElementToken.Element);

        //add child slots for this token's element, if it has any
        if (draggableElementToken.HasChildSlots())
            BM.AddChildSlots(gameObject.GetComponent<SlotReceiveElement>(), draggableElementToken);
    }

    private void PublishElementAddedToSlot(Element element)
    {
        foreach (var elementSlotEventSubscriber in _subscribers)
        {
            elementSlotEventSubscriber.ElementAddedToSlot(element);
        }
    }
}
