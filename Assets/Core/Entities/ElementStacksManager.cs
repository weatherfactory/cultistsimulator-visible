using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;
using Assets.CS.TabletopUI.Interfaces;

/// <summary>
/// Tracks and performs operations on tokens, considered as game model objects
/// Uses an ITokenPhysicalLocation (in Unity, a TokenTransformWrapper) to change the display
/// Referenced through gameobjects (Unity layer) which implement IContainsTokens
/// IContainsTokens objects should never have direct access to the ITokenPhysicalLocation (though it references them) because everything needs to be filtered through
/// the StacksManager for model management purposes
/// </summary>
public class ElementStacksManager {

    private readonly ITokenContainer _tokenContainer;
    private List<ElementStackToken> _stacks;
    private StackManagersCatalogue _catalogue;

    public string Name { get; set; }
    public bool EnforceUniqueStacksInThisStackManager { get; set; }

    public ElementStacksManager(ITokenContainer container, string name) {
        Name = name;
        _tokenContainer = container;

        _stacks = new List<ElementStackToken>();
        _catalogue = Registry.Get<StackManagersCatalogue>();
        _catalogue.RegisterStackManager(this);
    }

    public void Deregister() {
        var catalogue = Registry.Get<StackManagersCatalogue>();
        if (catalogue != null)
            catalogue.DeregisterStackManager(this);
    }

    public void ModifyElementQuantity(string elementId, int quantityChange, Source stackSource, Context context) {
        if (quantityChange > 0)
            IncreaseElement(elementId, quantityChange, stackSource, context);
        else
            ReduceElement(elementId, quantityChange, context);
    }

    /// <summary>
    /// Reduces matching stacks until change is satisfied
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="quantityChange">must be negative</param>
    /// <returns>returns any unsatisfied change remaining</returns>
    public int ReduceElement(string elementId, int quantityChange, Context context) {
        CheckQuantityChangeIsNegative(elementId, quantityChange);

        int unsatisfiedChange = quantityChange;
        while (unsatisfiedChange < 0) {
            ElementStackToken stackToAffect = _stacks.FirstOrDefault(c => !c.Defunct && c.GetAspects().ContainsKey(elementId));

            if (stackToAffect == null) //we haven't found either a concrete matching element, or an element with that ID.
                //so end execution here, and return the unsatisfied change amount
                return unsatisfiedChange;

            int originalQuantity = stackToAffect.Quantity;
            stackToAffect.ModifyQuantity(unsatisfiedChange,context);
            unsatisfiedChange += originalQuantity;

        }
        return unsatisfiedChange;
    }



    private static void CheckQuantityChangeIsNegative(string elementId, int quantityChange) {
        if (quantityChange >= 0)
            throw new ArgumentException("Tried to call ReduceElement for " + elementId + " with a >=0 change (" +
                                        quantityChange + ")");
    }

    public int IncreaseElement(string elementId, int quantityChange, Source stackSource, Context context, string locatorid = null) {

        if (quantityChange <= 0)
            throw new ArgumentException("Tried to call IncreaseElement for " + elementId + " with a <=0 change (" + quantityChange + ")");

        var newStack = _tokenContainer.ProvisionElementStack(elementId, quantityChange, stackSource, context, locatorid);
        AcceptStack(newStack, context);
        return quantityChange;
    }

    


    public int GetCurrentElementQuantity(string elementId) {
        return _stacks.Where(e => e.EntityId == elementId).Sum(e => e.Quantity);
    }
    /// <summary>
    /// All the elements in all the stacks (there may be duplicate elements in multiple stacks)
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, int> GetCurrentElementTotals() {
        var totals = _stacks.GroupBy(c => c.EntityId)
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

        foreach (var elementCard in _stacks) {
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

    public IEnumerable<ElementStackToken> GetStacks() {
        return _stacks.Where(s => !s.Defunct).ToList();
    }

    public bool PersistBetweenScenes
    {
        get { return _tokenContainer.PersistBetweenScenes; }
    }

    public ElementStackToken AddAndReturnStack(string elementId, int quantity, Source stackSource, Context context)
    {
        var newStack = _tokenContainer.ProvisionElementStack(elementId, quantity, stackSource,context);
        AcceptStack(newStack,context);
        return newStack;

    }

    /// <summary>
    /// Accept a stack into this StackManager. This also notifies the related TokenContainer to accept it in the view.
    /// </summary>
    /// <param name="stack"></param>
    /// <param name="context">How did it get transferred?</param>
    public void AcceptStack(ElementStackToken stack, Context context) {
        if (stack == null)
            return;

        NoonUtility.Log("Reassignment: " + stack.EntityId + " to " + this.Name,0,VerbosityLevel.Trivia);

        // Check if we're dropping a unique stack? Then kill all other copies of it on the tabletop
        if (EnforceUniqueStacksInThisStackManager) 
            RemoveDuplicates(stack);
        
        // Check if the stack's elements are decaying, and split them if they are
        // Decaying stacks should not be allowed
        while (stack.Decays && stack.Quantity > 1)
        {
            AcceptStack(stack.SplitAllButNCardsToNewStack(stack.Quantity - 1, context), context);
        }

        stack.SetStackManager(this);
        _stacks.Add(stack);
        _tokenContainer.DisplayHere(stack as ElementStackToken, context);
        _catalogue.NotifyStacksChanged();
    }

    void RemoveDuplicates(ElementStackToken incomingStack) {
      
        if (!incomingStack.Unique && string.IsNullOrEmpty(incomingStack.UniquenessGroup))
            return;
        
        foreach (var existingStack in new List<ElementStackToken>(_stacks)) {
            
            if (existingStack != incomingStack && existingStack.EntityId == incomingStack.EntityId) {
                NoonUtility.Log("Not the stack that got accepted, but has the same ID as the stack that got accepted? It's a copy!");
                existingStack.Retire(CardVFX.CardHide);
                return; // should only ever be one stack to retire!
                        // Otherwise this crashes because Retire changes the collection we are looking at
            }
            else if (existingStack != incomingStack && !string.IsNullOrEmpty(incomingStack.UniquenessGroup))
            {
                if (existingStack.UniquenessGroup == incomingStack.UniquenessGroup)
                    existingStack.Retire(CardVFX.CardHide);

            }
        }
    }

    public void AcceptStacks(IEnumerable<ElementStackToken> stacks, Context context) {
        foreach (var eachStack in stacks) {
            AcceptStack(eachStack, context);
        }
    }

    public void RemoveStack(ElementStackToken stack) {
        _stacks.Remove(stack);
        _catalogue.NotifyStacksChanged();
    }

    public void RemoveAllStacks()
    {
        var stacksListCopy=new List<ElementStackToken>(_stacks);
        foreach (ElementStackToken s in stacksListCopy)
            RemoveStack(s);
            (s as ElementStackToken).Retire(CardVFX.None);
        }
    }

    public void NotifyStacksChanged() {
        if (_catalogue == null)
            throw new ApplicationException("StacksManager is trying to notify the catalogue, but there's no catalogue! - for stacksmanager " + Name);
        _catalogue.NotifyStacksChanged();
    }

    /// <summary>
    /// This was relevant for a refactoring of the greedy slot code; I decided to do something else
    /// but this code might still be useful elsewhere!
    /// </summary>
    /// <param name="requirement"></param>
    /// <returns></returns>
    public List<ElementStackToken> GetStacksWithAspect(KeyValuePair<string,int> requirement)
    {
        List<ElementStackToken> matchingStacks = new List<ElementStackToken>();
        var candidateStacks = GetStacks(); //room here for caching
        foreach (var stack in candidateStacks)
        {
            int aspectAtValue = stack.GetAspects(true).AspectValue(requirement.Key);
            if (aspectAtValue>=requirement.Value)
                matchingStacks.Add(stack);
        }

        return matchingStacks;
    }

    public int PurgeElement(Element element, int maxToPurge)
    {

        if (string.IsNullOrEmpty(element.DecayTo))
        {
            //nb -p.value - purge max is specified as a positive cap, not a negative, for readability
          return  ReduceElement(element.Id, -maxToPurge, new Context(Context.ActionSource.Purge));
        }
        else
        { 
            int unsatisfiedChange = maxToPurge;
            while (unsatisfiedChange > 0)
            {
                
                //nb: if we transform a stack of >1, it's possible maxToPurge/Transform will be less than the stack total - iwc it'll transform the whole stack. Probably fine.
                ElementStackToken stackToAffect = _stacks.FirstOrDefault(c => !c.Defunct && c.GetAspects().ContainsKey(element.Id));

                if (stackToAffect == null) //we haven't found either a concrete matching element, or an element with that ID.
                    //so end execution here, and return the unsatisfied change amount
                    return unsatisfiedChange;

                int originalQuantity = stackToAffect.Quantity;
                stackToAffect.Decay(-1);
                //stackToAffect.Populate(element.DecayTo, stackToAffect.Quantity, Source.Existing());
                unsatisfiedChange -= originalQuantity;
            }
            return unsatisfiedChange;
        }
        



        
    }
}

