using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;


public interface IElementStacksManager {
    /// <summary>
    /// Reduces matching stacks until change is satisfied - NB a match is also a stack which possesses this aspect
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="quantityChange">must be negative</param>
    /// <returns>returns any unsatisfied change remaining</returns>
    int ReduceElement(string elementId, int quantityChange);
    int IncreaseElement(string elementId, int quantityChange, Source stackSource, string locatorId = null);
    int GetCurrentElementQuantity(string elementId);
    IDictionary<string, int> GetCurrentElementTotals();
    AspectsDictionary GetTotalAspects(bool showElementAspects = true);
    IEnumerable<IElementStack> GetStacks();
    void AcceptStack(IElementStack stack);
    void AcceptStacks(IEnumerable<IElementStack> stacks);
    void ConsumeStack(IElementStack stack);
    void ConsumeAllStacks();
    void ModifyElementQuantity(string elementId, int quantityChange,Source stackSource);
}

public class ElementStacksManager : IElementStacksManager {
    private readonly ITokenTransformWrapper _wrapper;
    private List<IElementStack> Stacks;


    public ElementStacksManager(ITokenTransformWrapper w) {
        _wrapper = w;
        Stacks=new List<IElementStack>();
    }

    public void ModifyElementQuantity(string elementId, int quantityChange,Source stackSource) {
        if (quantityChange > 0)
            IncreaseElement(elementId, quantityChange,stackSource);
        else
            ReduceElement(elementId, quantityChange);
    }

    /// <summary>
    /// Reduces matching stacks until change is satisfied
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="quantityChange">must be negative</param>
    /// <returns>returns any unsatisfied change remaining</returns>
    public int ReduceElement(string elementId, int quantityChange) {
      
        CheckQuantityChangeIsNegative(elementId, quantityChange);

        int unsatisfiedChange = quantityChange;
        while (unsatisfiedChange < 0) {
            IElementStack stackToAffect = Stacks.FirstOrDefault(c => !c.Defunct && c.GetAspects().ContainsKey(elementId));

            if (stackToAffect == null) //we haven't found either a concrete matching element, or an element with that ID.
                //so end execution here, and return the unsatisfied change amount
                return unsatisfiedChange;

            int originalQuantity = stackToAffect.Quantity;
            stackToAffect.ModifyQuantity(unsatisfiedChange);
            unsatisfiedChange += originalQuantity;

        }
        return unsatisfiedChange;
    }

    private static void CheckQuantityChangeIsNegative(string elementId, int quantityChange)
    {
        if (quantityChange >= 0)
            throw new ArgumentException("Tried to call ReduceElement for " + elementId + " with a >=0 change (" +
                                        quantityChange + ")");
    }

    public int IncreaseElement(string elementId, int quantityChange, Source stackSource, string locatorid = null) {

        if (quantityChange <= 0)
            throw new ArgumentException("Tried to call IncreaseElement for " + elementId + " with a <=0 change (" + quantityChange + ")");

        IElementStack newStack=_wrapper.ProvisionElementStack(elementId, quantityChange,stackSource, locatorid);
        Stacks.Add(newStack);
        return quantityChange;
    }


    public int GetCurrentElementQuantity(string elementId) {
        return Stacks.Where(e => e.Id == elementId).Sum(e => e.Quantity);
    }
    /// <summary>
    /// All the elements in all the stacks (there may be duplicate elements in multiple stacks)
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, int> GetCurrentElementTotals() {
        var totals = Stacks.GroupBy(c => c.Id)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(q => q.Quantity)))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return totals;
    }

    /// <summary>
    /// All the aspects in all the stacks, summing the aspects
    /// </summary>
    /// <returns></returns>
    public AspectsDictionary GetTotalAspects(bool includingSelf = true) {
        AspectsDictionary totals = new AspectsDictionary();

        foreach (var elementCard in Stacks) {
            var aspects = elementCard.GetAspects(includingSelf);

            foreach (string k in aspects.Keys) {
                if (totals.ContainsKey(k))
                    totals[k] += aspects[k];
                else
                    totals.Add(k, aspects[k]);
            }
        }

        return totals;
    }

    public IEnumerable<IElementStack> GetStacks() {
        return _wrapper.GetStacks();
    }

    public void AcceptStack(IElementStack stack) {
        //Redundant code here while we work through the differences between appearance and fundamental reality.
        //THERE IS NEVER ONLY ONE HISTORY. THE MANSUS HAS NO WALLS.
        stack.CurrentStacksManager = this;
        Stacks.Add(stack);
        _wrapper.Accept(stack);
    }

    public void AcceptStacks(IEnumerable<IElementStack> stacks) {
        foreach (var eachStack in stacks) {
            AcceptStack(eachStack);
        }
    }

    public void ConsumeStack(IElementStack stack)
    {
        Stacks.Remove(stack);
    }

    public void ConsumeAllStacks() {
        foreach (IElementStack stack in _wrapper.GetStacks())
            stack.SetQuantity(0);
    }


}

