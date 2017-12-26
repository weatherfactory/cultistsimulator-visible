using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;



/// <summary>
/// Tracks and performs operations on tokens, considered as game model objects
/// Uses an ITokenPhysicalLocation (in Unity, a TokenTransformWrapper) to change the display
/// Referenced through gameobjects (Unity layer) which implement IContainsTokens
/// IContainsTokens objects should never have direct access to the ITokenPhysicalLocation (though it references them) because everything needs to be filtered through
/// the StacksManager for model management purposes
/// </summary>
public class ElementStacksManager : IElementStacksManager {
    private readonly ITokenPhysicalLocation TokenPhysicalLocation;
    private List<IElementStack> Stacks;
    public string Name { get; set; }

    public ElementStacksManager(ITokenPhysicalLocation w,string name) {
        TokenPhysicalLocation = w;
        Stacks=new List<IElementStack>();
        Name = name;

        var catalogue = Registry.Retrieve<StackManagersCatalogue>();
        catalogue.RegisterStackManager(this);

    }

    public void Deregister()
    {
        var catalogue = Registry.Retrieve<StackManagersCatalogue>();
        if(catalogue!=null)
         catalogue.DeregisterStackManager(this);
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

        var newStack=TokenPhysicalLocation.ProvisionElementStack(elementId, quantityChange,stackSource, locatorid);
        AcceptStack(newStack);
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

    public IEnumerable<IElementStack> GetStacks()
    {
        return Stacks.Where(s=>!s.Defunct).ToList();
    }

    public void AcceptStack(IElementStack stack) {
        NoonUtility.Log("Reassignment: " + stack.Id + " to " + this.Name,3);
        stack.AssignToStackManager(this);
        Stacks.Add(stack);
        TokenPhysicalLocation.DisplayHere(stack);
        
    }

    public void AcceptStacks(IEnumerable<IElementStack> stacks) {
        foreach (var eachStack in stacks) {
            AcceptStack(eachStack);
        }
    }

    public void RemoveStack(IElementStack stack)
    {
        Stacks.Remove(stack);
    }

    public void ConsumeAllStacks() {
        foreach (IElementStack stack in TokenPhysicalLocation.GetStacks())
            stack.SetQuantity(0);
    }


}

