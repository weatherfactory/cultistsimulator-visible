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

public class SlotsContainer : MonoBehaviour,ITokenSubscriber
{

    private SituationController _situationController;
    private RecipeSlot primarySlot;
    private bool AllowChildSlots = false;

    public void InitialiseSlotsForActiveSituation(Situation s,SituationController sc)
    {
        AllowChildSlots = false;

        _situationController = sc;
        var slotsToBuild = s.GetSlots();
        if (slotsToBuild.Any())
        {
            gameObject.SetActive(true);
        foreach (SlotSpecification css in s.GetSlots())
            BuildSlot(css.Label, css);
        }

        

    }

    public void InitialiseSlotsForEmptySituation(SituationController sc)
    {
        AllowChildSlots = true;

        _situationController = sc;
        gameObject.SetActive(true);
        primarySlot = BuildSlot();
        ArrangeSlots();
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

    public void StackInSlot(RecipeSlot slot, ElementStack stack)
    {
        DraggableToken.resetToStartPos = false;
        // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
        PositionStackInSlot(slot, stack);

        _situationController.DisplayRecipeForAspects(GetAspectsFromSlottedCards());
        stack.SetContainer(this);

        if (stack.HasChildSlots())
            AddSlotsForStack(stack, slot);

        ArrangeSlots();
    }

    public AspectsDictionary GetAspectsFromSlottedCards()
    {
        AspectsDictionary currentAspects = GetStacksGateway().GetTotalAspects();
        return currentAspects;
    }

    private void AddSlotsForStack(ElementStack stack, RecipeSlot slot)
    {
        foreach (var childSlotSpecification in stack.GetChildSlotSpecifications())
            //add slot to child slots of slot
            slot.childSlots.Add(BuildSlot("childslot of " + stack.ElementId, childSlotSpecification));
    }


    private static void PositionStackInSlot(RecipeSlot slot, ElementStack stack)
    {
        stack.transform.SetParent(slot.transform);
        stack.transform.localPosition = Vector3.zero;
        stack.transform.localRotation = Quaternion.identity;
    }

    public ElementStacksGateway GetStacksGateway()
    {
        return new ElementStacksGateway(new TabletopElementStacksWrapper(transform));
    }

    private float SlotSpaceNeeded(RecipeSlot forSlot, float slotWidth, float slotSpacing)
    {
        float childSpaceNeeded = 0;
        foreach (RecipeSlot c in forSlot.childSlots)
            childSpaceNeeded += SlotSpaceNeeded(c, slotWidth, slotSpacing);

        return Mathf.Max(childSpaceNeeded, slotWidth + slotSpacing);
    }



    private void AlignSlot(RecipeSlot thisSlot, int index, float parentX, float parentY, float slotWidth, float slotHeight, float slotSpacing)
    {
        float thisY = parentY - (slotHeight + slotSpacing);
        float spaceNeeded = SlotSpaceNeeded(thisSlot, slotWidth, slotSpacing);
        float thisX = parentX + index * spaceNeeded;
        thisSlot.transform.localPosition = new Vector3(thisX, thisY);
        for (int i = 0; i < thisSlot.childSlots.Count; i++)
        {
            //space needed is space needed for each child slot, + spacing
            var nextSlot = thisSlot.childSlots[i];
            float nextX = thisX + ((slotWidth + slotSpacing) * index);
            AlignSlot(nextSlot, i, nextX, thisY, slotWidth, slotHeight, slotSpacing);
        }

    }

    public void ArrangeSlots()
    {
        if(AllowChildSlots)
        {
            float slotSpacing = 10;
            float slotWidth = ((RectTransform) primarySlot.transform).rect.width;
            float slotHeight = ((RectTransform) primarySlot.transform).rect.height;
            float startingHorizSpace = ((RectTransform) primarySlot.transform.parent).rect.width;
            float startingX = startingHorizSpace/2 - slotWidth;
            float startingY = -120;
            primarySlot.transform.localPosition = new Vector3(startingX, startingY);


            if (primarySlot.childSlots.Count > 0)
            {
                for (int i = 0; i < primarySlot.childSlots.Count; i++)
                {
                    //space needed is space needed for each child slot, + spacing
                    var s = primarySlot.childSlots[i];
                    AlignSlot(s, i, startingX, startingY, slotWidth, slotHeight, slotSpacing);
                }
            }
        }
    }

    private void ClearAndDestroySlot(RecipeSlot slot)
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

    private void RemoveAnyChildSlotsWithEmptyParent()
    {
        List<RecipeSlot> currentSlots = new List<RecipeSlot>(GetComponentsInChildren<RecipeSlot>());
        foreach (RecipeSlot s in currentSlots)
        {
            if (s != null & s.GetElementStackInSlot() == null & s.childSlots.Count > 0)
            {
                List<RecipeSlot> currentChildSlots = new List<RecipeSlot>(s.childSlots);
                s.childSlots.Clear();
                foreach (RecipeSlot cs in currentChildSlots.Where(eachSlot => eachSlot != null))
                    ClearAndDestroySlot(cs);
            }
        }
    }

    public void TokenRemovedFromSlot()
    {
        RemoveAnyChildSlotsWithEmptyParent();
        ArrangeSlots();
    }


    public void TokenPickedUp(DraggableToken draggableToken)
    {
        _situationController.DisplayRecipeForAspects(GetAspectsFromSlottedCards());
        draggableToken.SetContainer(null);
        TokenRemovedFromSlot();
    }

    public void TokenInteracted(DraggableToken draggableToken)
    {
        
    }

    public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
    {
        
    }
}
