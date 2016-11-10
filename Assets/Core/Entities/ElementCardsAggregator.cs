using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;


public class ElementCardsAggregator
{
    private IEnumerable<IElementCard> elementCards;
    protected List<ICharacterInfoSubscriber> _detailsSubscribers;

    public ElementCardsAggregator(IEnumerable<IElementCard> forCards)
    {
        elementCards = forCards;
    }

    public void ModifyElementQuantity(string elementId, int quantityChange)
    {
        
    }


    public int GetCurrentElementQuantity(string elementId)
    {
            return elementCards.Where(e => e.ElementId == elementId).Sum(e => e.Quantity);
    }

    public Dictionary<string,int> GetCurrentElementTotals()
    {
        
        var totals = elementCards.GroupBy(c => c.ElementId)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(q => q.Quantity)))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return totals;
    }

    public Dictionary<string,int> GetTotalAspects()
    {
        var totals=new Dictionary<string,int>();

        foreach (var elementCard in elementCards)
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

