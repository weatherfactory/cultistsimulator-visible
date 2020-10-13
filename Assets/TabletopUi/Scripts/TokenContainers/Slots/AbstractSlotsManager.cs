﻿using UnityEngine;
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



public abstract class AbstractSlotsManager : MonoBehaviour {

    protected SituationController situationController;
    
    protected List<RecipeSlot> validSlots;

    

    public virtual void Initialise(IVerb verb,SituationController sc)
    {
        situationController = sc;
        var children = GetComponentsInChildren<RecipeSlot>();
        var allSlots = new List<RecipeSlot>(children);
         validSlots = new List<RecipeSlot>(allSlots.Where(rs => rs.Defunct == false && rs.GoverningSlotSpecification!=null));
   
    }

    public virtual IList<RecipeSlot> GetAllSlots() {

        return validSlots;
    }

    public IRecipeSlot GetSlotBySaveLocationInfoPath(string saveLocationInfoPath) {
        var candidateSlots = GetAllSlots();
        IRecipeSlot slotToReturn = candidateSlots.SingleOrDefault(s => s.SaveLocationInfoPath == saveLocationInfoPath);
        return slotToReturn;
    }

    protected virtual RecipeSlot BuildSlot(string slotName, SlotSpecification slotSpecification, RecipeSlot parentSlot, bool wideLabel = false) {
        var slot = Registry.Get<PrefabFactory>().CreateLocally<RecipeSlot>(transform);

        slot.name = slotName + (slotSpecification != null ? " - " + slotSpecification.Id : "");
        slot.ParentSlot = parentSlot;
        slot.Initialise(slotSpecification);
        slot.onCardDropped += RespondToStackAdded;
        slot.onCardRemoved += RespondToStackRemoved;
        if (wideLabel)
        {
            var slotTransform = slot.SlotLabel.GetComponent<RectTransform>();
            var originalSize = slotTransform.sizeDelta;
            slotTransform.sizeDelta = new Vector2(originalSize.x * 1.5f, originalSize.y * 0.75f);
        }

        validSlots.Add(slot);

        return slot;
    }

    public abstract void RespondToStackRemoved(ElementStackToken stack, Context context);

    public abstract void RespondToStackAdded(RecipeSlot slot, ElementStackToken stack, Context context);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="includeElementAspects">true to return aspects for the elements themselves as well; false to include only their aspects</param>
    /// <returns></returns>
    public AspectsDictionary GetAspectsFromSlottedCards(bool includeElementAspects) {
        AspectsDictionary currentAspects = new AspectsDictionary();
        ElementStackToken stack;

        foreach (IRecipeSlot slot in GetAllSlots()) {
            stack = slot.GetElementStackInSlot();

            if (stack != null)
                currentAspects.CombineAspects(stack.GetAspects(includeElementAspects));
        }

        return currentAspects;
    }

    public IEnumerable<ElementStackToken> GetStacksInSlots() {
        IList<ElementStackToken> stacks = new List<ElementStackToken>();
        ElementStackToken stack;

        foreach (IRecipeSlot slot in GetAllSlots()) {
            stack = slot.GetElementStackInSlot();

            if (stack != null)
                stacks.Add(stack);
        }

        return stacks;
    }

    protected virtual void ClearAndDestroySlot(RecipeSlot slot, Context context) {
        if (slot == null)
            return;

        validSlots.Remove(slot);

        //if there are any child slots on this slot, recurse
        if (slot.childSlots.Count > 0) {
            List<RecipeSlot> childSlots = new List<RecipeSlot>(slot.childSlots);

            foreach (var cs in childSlots)
                ClearAndDestroySlot(cs, context);

            slot.childSlots.Clear();
        }

        AbstractToken tokenContained = slot.GetTokenInSlot();

        if (tokenContained != null) {
            tokenContained.ReturnToTabletop(context);
        }

        slot.Retire();
    }




}