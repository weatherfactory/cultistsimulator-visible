﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;


public interface IElementStacksGateway
{
    /// <summary>
    /// Reduces matching stacks until change is satisfied
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="quantityChange">must be negative</param>
    /// <returns>returns any unsatisfied change remaining</returns>
    int ReduceElement(string elementId, int quantityChange);

    int IncreaseElement(string elementId, int quantityChange);
    int GetCurrentElementQuantity(string elementId);
    Dictionary<string,int> GetCurrentElementTotals();
    Dictionary<string,int> GetTotalAspects();
    IEnumerable<IElementStack> GetStacks();
    void AcceptElementStack(IElementStack stack);
    void AcceptStacks(IEnumerable<IElementStack> stacks);
}

public class ElementStacksGateway : IElementStacksGateway
{
    private IElementStacksWrapper wrapper;
    
    public ElementStacksGateway(IElementStacksWrapper w)
    {
        wrapper = w;
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
             IElementStack cardToRemove = wrapper.GetStacks().FirstOrDefault(c => c.ElementId == elementId && c.Defunct==false);
            if(cardToRemove==null)
                //we've run out of matching cards; break, and return unsatisfied change amount
                return unsatisfiedChange;

            int originalQuantity = cardToRemove.Quantity;
            cardToRemove.ModifyQuantity(unsatisfiedChange);
            unsatisfiedChange += originalQuantity;
        }
        return unsatisfiedChange;
    }

    public int IncreaseElement(string elementId, int quantityChange)
    {
        if (quantityChange <= 0)
            throw new ArgumentException("Tried to call IncreaseElement for " + elementId + " with a <=0 change (" + quantityChange + ")");

        wrapper.ProvisionElementStack(elementId, quantityChange);
        return quantityChange;
    }


    public int GetCurrentElementQuantity(string elementId)
    {
            return wrapper.GetStacks().Where(e => e.ElementId == elementId).Sum(e => e.Quantity);
    }

    public Dictionary<string,int> GetCurrentElementTotals()
    {
        var totals = wrapper.GetStacks().GroupBy(c => c.ElementId)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(q => q.Quantity)))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return totals;
    }

    public Dictionary<string,int> GetTotalAspects()
    {
        var totals=new Dictionary<string,int>();

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

    public void AcceptElementStack(IElementStack stack)
    {
        wrapper.Accept(stack);
    }

    public void AcceptStacks(IEnumerable<IElementStack> stacks)
    {
        foreach (var eachStack in stacks)
        {
            AcceptElementStack(eachStack);
        }
    }
}

