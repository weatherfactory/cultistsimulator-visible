using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;

public abstract class AbstractSlotsContainer : MonoBehaviour,ITokenSubscriber
{

    protected SituationController _situationController;
    protected RecipeSlot primarySlot;
    

    public  virtual void Initialise(SituationController sc)
    {

    }



    void HandleOnSlotDroppedOn(RecipeSlot slot)
    {

        ElementStack stack = DraggableToken.itemBeingDragged as ElementStack;
        if (stack != null)
        {
            SlotMatchForAspects match = slot.GetSlotMatchForStack(stack);
            if (match.MatchType == SlotMatchForAspectsType.Okay)
                StackInSlot(slot, stack);
            else
                stack.ReturnToTabletop(new Notification("I can't put that there - ", match.GetProblemDescription()));

        }
    }

    public RecipeSlot BuildSlot(string slotName = "Recipe Slot", SlotSpecification slotSpecification = null)
    {
        var slot = PrefabFactory.CreateLocally<RecipeSlot>(transform);

        slot.name = slotName;
        if (slotSpecification != null)
        {
            slot.GoverningSlotSpecification = slotSpecification;
            slot.name += " - " + slotSpecification.Label;
        }

        slot.onCardDropped += HandleOnSlotDroppedOn;
        return slot;
    }

    public abstract void StackInSlot(RecipeSlot slot, ElementStack stack);

    public AspectsDictionary GetAspectsFromSlottedCards()
    {
        AspectsDictionary currentAspects = GetStacksGateway().GetTotalAspects();
        return currentAspects;
    }


    protected static void PositionStackInSlot(RecipeSlot slot, ElementStack stack)
    {
        stack.transform.SetParent(slot.transform);
        stack.transform.localPosition = Vector3.zero;
        stack.transform.localRotation = Quaternion.identity;
    }

    public ElementStacksGateway GetStacksGateway()
    {
        return new ElementStacksGateway(new TabletopElementStacksWrapper(transform));
    }



    protected void ClearAndDestroySlot(RecipeSlot slot)
    {
        if (slot == null)
            return;
        //if there are any child slots on this slot, recurse
        if (slot.childSlots.Count > 0)
        {
            List<RecipeSlot> childSlots = new List<RecipeSlot>(slot.childSlots);
            foreach (var cs in childSlots)
                ClearAndDestroySlot(cs);
            slot.childSlots.Clear();
        }
        ElementStack stackContained = slot.GetElementStackInSlot();
        if (stackContained != null)
        {
            stackContained.ReturnToTabletop(null);
        }
        DestroyObject(slot.gameObject);
    }



    public abstract void TokenPickedUp(DraggableToken draggableToken);

    public void TokenInteracted(DraggableToken draggableToken)
    {
        
    }

    public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
    {
        
    }
}
