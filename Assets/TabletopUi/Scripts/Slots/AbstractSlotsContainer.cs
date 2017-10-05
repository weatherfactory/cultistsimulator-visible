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

public abstract class AbstractSlotsContainer : MonoBehaviour
{

    protected SituationController _situationController;
    protected RecipeSlot primarySlot;

    public  virtual void Initialise(SituationController sc)
    {
        _situationController = sc;
    }


    public IList<RecipeSlot> GetAllSlots()
    {
        var children = GetComponentsInChildren<RecipeSlot>();
        var allSlots = new List<RecipeSlot>(children);
        var undestroyedSlots = new List<RecipeSlot>(allSlots.Where(rs => rs.Defunct == false));
        return undestroyedSlots;
    }

    public IRecipeSlot GetSlotBySaveLocationInfoPath(string saveLocationInfoPath)
    {
        var candidateSlots = GetAllSlots();
        IRecipeSlot slotToReturn =candidateSlots.SingleOrDefault(s => s.SaveLocationInfoPath == saveLocationInfoPath);
        return slotToReturn;
    }

    void HandleOnSlotDroppedOn(RecipeSlot slot,IElementStack stack)
    {
        RespondToStackAdded(slot, stack);
        
    }

    public virtual RecipeSlot BuildSlot(string slotName, SlotSpecification slotSpecification, RecipeSlot parentSlot)
    {
        var slot = PrefabFactory.CreateLocally<RecipeSlot>(transform);

        slot.name = slotName;
        slot.ParentSlot = parentSlot;
        slot.SlotLabel.text = slotSpecification.Label;
        if (slotSpecification != null)
        {
            slot.GoverningSlotSpecification = slotSpecification;
            slot.name += " - " + slotSpecification.Id;
        }
        slot.onCardDropped += HandleOnSlotDroppedOn;
        slot.onCardPickedUp += RespondToStackPickedUp;

        if (slotSpecification.Greedy)
            slot.IsGreedy = true;
        if (slotSpecification.Consumes)
            slot.IsConsuming = true;

        return slot;    
    }

    public abstract void RespondToStackPickedUp(IElementStack stack);

    public abstract void RespondToStackAdded(RecipeSlot slot, IElementStack stack);

    public AspectsDictionary GetAspectsFromSlottedCards()
    {
        AspectsDictionary currentAspects=new AspectsDictionary();
        foreach (IRecipeSlot slot in GetAllSlots())
            if(slot.GetElementStackInSlot()!=null)
            currentAspects.CombineAspects(slot.GetElementStackInSlot().GetAspects());

        return currentAspects;
    }

    public IEnumerable<IElementStack> GetStacksInSlots()
    {
        IList<IElementStack> stacks= new List<IElementStack>();
        foreach (IRecipeSlot slot in GetAllSlots())
            if(slot.GetElementStackInSlot()!=null)
            stacks.Add(slot.GetElementStackInSlot());

        return stacks;
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
        DraggableToken tokenContained = slot.GetTokenInSlot();
        if (tokenContained != null)
        {
            tokenContained.ReturnToTabletop(null);
        }
        slot.Retire();
    }




    public bool AllowDrag { get { return true; } }
}
