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
    public List<ElementStackToken> _stacks;
    private TokenContainersCatalogue _catalogue;

    public string Name { get; set; }
    public bool EnforceUniqueStacksInThisStackManager { get; set; }

    public ElementStacksManager(ITokenContainer container, string name) {
        Name = name;
        _tokenContainer = container;

        _stacks = new List<ElementStackToken>();
        _catalogue = Registry.Get<TokenContainersCatalogue>();
        _catalogue.RegisterTokenContainer(_tokenContainer);
    }

    public void Deregister() {
        var catalogue = Registry.Get<TokenContainersCatalogue>();
        if (catalogue != null)
            catalogue.DeregisterTokenContainer(_tokenContainer);
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

        if(stack.TokenContainer==null)
            stack.SetTokenContainer(_tokenContainer,context); //the SetTokenCOntainer and AcceptStack call should really be in the same place all the time. But I don't want to mess too much with the live branch.

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

        //sometimes, we reassign a stack to a container where it already lives. Don't add it again!
        if(!_stacks.Contains(stack))
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

    /// <summary>
    /// removes the stack from this stack manager; doesn't retire the stack
    /// </summary>
    /// <param name="stack"></param>
    public void RemoveStack(ElementStackToken stack) {
        _stacks.Remove(stack);
        _catalogue.NotifyStacksChanged();
    }

    /// <summary>
    /// removes the stacks from this stack manager; doesn't retire the stack
    /// </summary>
    public void RemoveAllStacks()
    {
        var stacksListCopy=new List<ElementStackToken>(_stacks);
        foreach (ElementStackToken s in stacksListCopy)
            RemoveStack(s);
    }

    public void RetireAllStacks()
    {
        var stacksListCopy = new List<ElementStackToken>(_stacks);
        foreach (ElementStackToken s in stacksListCopy)
            s.Retire(CardVFX.None);
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
        var candidateStacks = new List<ElementStackToken>(_stacks); //room here for caching
        foreach (var stack in candidateStacks)
        {
            int aspectAtValue = stack.GetAspects(true).AspectValue(requirement.Key);
            if (aspectAtValue>=requirement.Value)
                matchingStacks.Add(stack);
        }

        return matchingStacks;
    }

  
}

