using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;


public interface IElementStacksManager
{
    /// <summary>
    /// Reduces matching stacks until change is satisfied
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="quantityChange">must be negative</param>
    /// <returns>returns any unsatisfied change remaining</returns>
    int ReduceElement(string elementId, int quantityChange);
    int IncreaseElement(string elementId, int quantityChange,string locatorId=null);
    int GetCurrentElementQuantity(string elementId);
    IDictionary<string,int> GetCurrentElementTotals();
    AspectsDictionary GetTotalAspects();
    IEnumerable<IElementStack> GetStacks();
    void AcceptStack(IElementStack stack);
    void AcceptStacks(IEnumerable<IElementStack> stacks);
    void ConsumeAllStacks();
    void ModifyElementQuantity(string elementId, int quantityChange);
}

public class ElementStacksManager : IElementStacksManager
{
    private ITokenTransformWrapper wrapper;
    
    public ElementStacksManager(ITokenTransformWrapper w)
    {
        wrapper = w;
    }

    public void ModifyElementQuantity(string elementId, int quantityChange)
    {
        if (quantityChange > 0)
            IncreaseElement(elementId, quantityChange);
        else
            ReduceElement(elementId, quantityChange);
    }

    /// <summary>
    /// Reduces matching stacks until change is satisfied
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="quantityChange">must be negative</param>
    /// <returns>returns any unsatisfied change remaining</returns>
    public int ReduceElement(string elementId, int quantityChange)
    {
     if(quantityChange>=0)
            throw new ArgumentException("Tried to call ReduceElement for " + elementId + " with a >=0 change (" + quantityChange + ")");

        int unsatisfiedChange = quantityChange;
        while(unsatisfiedChange<0)
        {
            IElementStack cardToRemove = wrapper.GetStacks().FirstOrDefault(c => !c.Defunct && c.GetAspects().ContainsKey(elementId) );

            //IElementStack cardToRemove = wrapper.GetStacks().FirstOrDefault(c => c.Id == elementId && c.Defunct==false);
            //if (cardToRemove == null)
            //    //no elements which match that id: try looking for elements which possess an aspect with that id
            //    cardToRemove = wrapper.GetStacks().FirstOrDefault(c => c.GetAspects().ContainsKey(elementId));
            
            if(cardToRemove==null) //if it's *still* null, we haven't found either a concrete matching element, or an element with that ID.
                //so end execution here, and return the unsatisfied change amount
                return unsatisfiedChange;

                int originalQuantity = cardToRemove.Quantity;
                cardToRemove.ModifyQuantity(unsatisfiedChange);
                unsatisfiedChange += originalQuantity;
            
        }
        return unsatisfiedChange;
    }

    public int IncreaseElement(string elementId, int quantityChange,string locatorid=null)
    {
        if (quantityChange <= 0)
            throw new ArgumentException("Tried to call IncreaseElement for " + elementId + " with a <=0 change (" + quantityChange + ")");

        wrapper.ProvisionElementStack(elementId, quantityChange,locatorid);
        return quantityChange;
    }


    public int GetCurrentElementQuantity(string elementId)
    {
            return wrapper.GetStacks().Where(e => e.Id == elementId).Sum(e => e.Quantity);
    }
    /// <summary>
    /// All the elements in all the stacks (there may be duplicate elements in multiple stacks)
    /// </summary>
    /// <returns></returns>
    public IDictionary<string,int> GetCurrentElementTotals()
    {
        var totals = wrapper.GetStacks().GroupBy(c => c.Id)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(q => q.Quantity)))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return totals;
    }

    /// <summary>
    /// All the aspects in all the stacks, summing the aspects
    /// </summary>
    /// <returns></returns>
    public AspectsDictionary GetTotalAspects()
    {
        AspectsDictionary totals =new AspectsDictionary();

        foreach (var elementCard in wrapper.GetStacks())
        {
            var aspects=elementCard.GetAspects();
            foreach (string k in aspects.Keys)
            {
                if (totals.ContainsKey(k))
                    totals[k] += aspects[k];
                else
                totals.Add(k,aspects[k]);
            }
        }

        return totals;
    }

    public IEnumerable<IElementStack> GetStacks()
    {
        return wrapper.GetStacks();
    }

    public void AcceptStack(IElementStack stack)
    {
        wrapper.Accept(stack);
    }

    public void AcceptStacks(IEnumerable<IElementStack> stacks)
    {
        foreach (var eachStack in stacks)
        {
             AcceptStack(eachStack);
        }
    }

    public void ConsumeAllStacks()
    {
       foreach(IElementStack stack in wrapper.GetStacks())
            stack.SetQuantity(0);
    }


}

