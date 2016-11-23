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

public abstract class AbstractSlotsContainer : MonoBehaviour,ITokenContainer
{

    protected SituationController _situationController;
    protected RecipeSlot primarySlot;
    

    public  virtual void Initialise(SituationController sc)
    {

    }


    public IList<RecipeSlot> GetAllSlots()
    {
     return  new List<RecipeSlot>(GetComponentsInChildren<RecipeSlot>());
    }

    void HandleOnSlotDroppedOn(RecipeSlot slot,IElementStack s)
    {

       var stack = s as ElementStack;
        stack.SetContainer(this);
        RespondToStackAdded(slot, stack);
        
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

    public abstract void RespondToStackAdded(RecipeSlot slot, ElementStack stack);

    public AspectsDictionary GetAspectsFromSlottedCards()
    {
        AspectsDictionary currentAspects = GetStacksGateway().GetTotalAspects();
        return currentAspects;
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
