using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;


public class ElementStacksGateway
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
             IElementStack cardToRemove = wrapper.Stacks().FirstOrDefault(c => c.ElementId == elementId && c.Defunct==false);
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
            return wrapper.Stacks().Where(e => e.ElementId == elementId).Sum(e => e.Quantity);
    }

    public Dictionary<string,int> GetCurrentElementTotals()
    {
        var totals = wrapper.Stacks().GroupBy(c => c.ElementId)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(q => q.Quantity)))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return totals;
    }

    public Dictionary<string,int> GetTotalAspects()
    {
        var totals=new Dictionary<string,int>();

        foreach (var elementCard in wrapper.Stacks())
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
}

