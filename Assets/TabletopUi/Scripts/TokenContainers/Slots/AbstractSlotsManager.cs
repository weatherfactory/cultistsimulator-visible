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
using UnityEditor.PackageManager.UI;


public abstract class AbstractSlotsManager : MonoBehaviour {

    
    protected List<RecipeSlot> validSlots;
    



    public virtual IList<RecipeSlot> GetAllSlots() {

        return validSlots;
    }

    public RecipeSlot GetSlotBySaveLocationInfoPath(string saveLocationInfoPath) {
        var candidateSlots = GetAllSlots();
        RecipeSlot slotToReturn = candidateSlots.SingleOrDefault(s => s.GetPath().ToString() == saveLocationInfoPath);
        return slotToReturn;
    }



    public abstract void RespondToStackRemoved(ElementStack stack, Context context);

    public abstract void RespondToStackAdded(RecipeSlot slot, ElementStack stack, Context context);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="includeElementAspects">true to return aspects for the elements themselves as well; false to include only their aspects</param>
    /// <returns></returns>
    public AspectsDictionary GetAspectsFromSlottedCards(bool includeElementAspects) {
        AspectsDictionary currentAspects = new AspectsDictionary();
        ElementStack stack;

        foreach (RecipeSlot slot in GetAllSlots()) {
            stack = slot.GetElementTokenInSlot();

            if (stack != null)
                currentAspects.CombineAspects(stack.GetAspects(includeElementAspects));
        }

        return currentAspects;
    }

    public IEnumerable<ElementStack> GetStacksInSlots() {
        IList<ElementStack> stacks = new List<ElementStack>();
        ElementStack stack;

        foreach (RecipeSlot slot in GetAllSlots()) {
            stack = slot.GetElementTokenInSlot();

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

        Token tokenContained = slot.GetTokenInSlot();

        if (tokenContained != null) {
            tokenContained.ReturnToTabletop(context);
        }

        slot.Retire();
    }




}
