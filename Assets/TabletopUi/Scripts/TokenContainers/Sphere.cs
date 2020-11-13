using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure {


    public enum BlockReason
    {
      StackEnRouteToContainer
    }

    public enum BlockDirection
    {
        None,
        Inward,
        Outward,
        All
    }

    /// <summary>
    /// blocking entry/exit
    /// </summary>
    public class ContainerBlock
    {
        public BlockDirection BlockDirection { get; }
        public BlockReason BlockReason { get; }

        public ContainerBlock(BlockDirection direction, BlockReason reason)
        {
            BlockDirection = direction;
            BlockReason = reason;
        }
    }

    public abstract class Sphere : MonoBehaviour, ISphereEventSubscriber
    {
        public virtual Type DropzoneType => typeof(DropzoneManifestation);
        public virtual Type ElementManifestationType => typeof(CardManifestation);

        public virtual bool AllowDrag { get; private set; }
        public virtual bool AllowStackMerge => true;
        public virtual bool AlwaysShowHoverGlow => true;
        public virtual bool PersistBetweenScenes => false;
        public virtual bool EnforceUniqueStacksInThisContainer => true;
        public virtual bool ContentsHidden => false;
        public virtual bool IsGreedy => false;
        public abstract SphereCategory SphereCategory { get; }
        public SlotSpecification GoverningSlotSpecification { get; set; }

        [Tooltip("Use this to specify the SpherePath in the editor")]
        [SerializeField] protected string pathIdentifier;

        public SphereCatalogue Catalogue
        {
            get
            {
                if (_catalogue == null)
                {
                    _catalogue = Registry.Get<SphereCatalogue>();
                    _catalogue.RegisterSphere(this);
                }

                return _catalogue;
            }
        }


        protected List<INotifier> _notifiersForContainer = new List<INotifier>();
        public bool Defunct { get; protected set; }
        protected HashSet<ContainerBlock> _currentContainerBlocks = new HashSet<ContainerBlock>();
        private SphereCatalogue _catalogue;
        private List<Token> _tokens = new List<Token>();
        private readonly HashSet<ISphereEventSubscriber> _subscribers = new HashSet<ISphereEventSubscriber>();

        public void Subscribe(ISphereEventSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(ISphereEventSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }


        public virtual void OnEnable()
        {
            Catalogue.RegisterSphere(
                this); //this is a double call - we already subscribe above. This should be fine because it's a hashset, and because we may want to disable then re-enable. But FYI, future AK.
        }

        public virtual void OnDisable()
        {
            Catalogue.DeregisterSphere(this);
        }


        public virtual bool Retire()
        {
            RetireAllTokens();
            Destroy(gameObject);
            Defunct = true;
            return true;
        }

        public virtual bool CurrentlyBlockedFor(BlockDirection direction)
        {
            var currentBlockDirection = CurrentBlockDirection();
            return (currentBlockDirection == BlockDirection.All ||
                    currentBlockDirection == direction);
        }

        public BlockDirection CurrentBlockDirection()
        {
            bool inwardblock = _currentContainerBlocks.Any(cb => cb.BlockDirection == BlockDirection.Inward);
            bool outwardBlock = _currentContainerBlocks.Any(cb => cb.BlockDirection == BlockDirection.Outward);
            bool allBlock = _currentContainerBlocks.Any(cb => cb.BlockDirection == BlockDirection.All);

            if (allBlock || (inwardblock && outwardBlock))
                return BlockDirection.All;

            if (inwardblock)
                return BlockDirection.Inward;
            if (outwardBlock)
                return BlockDirection.Outward;

            return BlockDirection.None;
        }

        public bool AddBlock(ContainerBlock block)
        {
            return _currentContainerBlocks.Add(block);
        }

        public int RemoveBlock(ContainerBlock blockToRemove)
        {
            if (blockToRemove.BlockDirection == BlockDirection.All)
                return _currentContainerBlocks.RemoveWhere(cb => cb.BlockReason == blockToRemove.BlockReason);
            else
                return _currentContainerBlocks.RemoveWhere(cb =>
                    cb.BlockDirection == blockToRemove.BlockDirection && cb.BlockReason == blockToRemove.BlockReason);

        }

        public Token ProvisionStackFromCommand(StackCreationCommand stackCreationCommand,
            Source stackSource, Context context)
        {
            var token = ProvisionElementStackToken(stackCreationCommand.ElementId, stackCreationCommand.ElementQuantity,
                stackSource, context);
            foreach (var m in stackCreationCommand.Mutations)
                token.ElementStack.SetMutation(m.Key, m.Value, false);

            token.ElementStack.IlluminateLibrarian = new IlluminateLibrarian(stackCreationCommand.Illuminations);

            if (stackCreationCommand.LifetimeRemaining > 0)
                token.ElementStack.LifetimeRemaining = stackCreationCommand.LifetimeRemaining;

            if (stackCreationCommand.MarkedForConsumption)
                token.ElementStack.MarkedForConsumption = true;

            token.LastTablePos = stackCreationCommand.Location.Position;

            return token;
        }


        public virtual Token ProvisionElementStackToken(string elementId, int quantity)
        {
            return ProvisionElementStackToken(elementId, quantity, Source.Existing(),
                new Context(Context.ActionSource.Unknown));
        }


        public Token ProvisionElementStackToken(string elementId, int quantity, Source stackSource,
            Context context)
        {

            var gameobject =new GameObject(elementId);
           var stack=gameobject.AddComponent<ElementStack>();
            
            stack.Populate(elementId, quantity, stackSource);

            var token = Registry.Get<PrefabFactory>().CreateLocally<Token>(transform);
            stack.AttachToken(token);

            AcceptToken(token, context);

            return token;
        }


        public virtual void DisplayHere(Token token, Context context)
        {
            token.Manifest(this);
            token.transform.SetParent(transform);
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;

        }

        public virtual void TryMoveAsideFor(Token potentialUsurper, Token incumbent,
            out bool incumbentMoved)
        {
            // By default: do no move-aside
            incumbentMoved = false;
        }


        public virtual SpherePath GetPath()
        {
            return new SpherePath(pathIdentifier);
        }

        public virtual void OnDestroy()
        {
            Registry.Get<SphereCatalogue>().DeregisterSphere(this);
        }

        public void ModifyElementQuantity(string elementId, int quantityChange, Source stackSource, Context context)
        {
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
        public int ReduceElement(string elementId, int quantityChange, Context context)
        {
            CheckQuantityChangeIsNegative(elementId, quantityChange);

            int unsatisfiedChange = quantityChange;
            while (unsatisfiedChange < 0)
            {
                Token tokenToAffect =
                    _tokens.FirstOrDefault(c => !c.Defunct && c.ElementStack.GetAspects().ContainsKey(elementId));

                if (tokenToAffect == null
                    ) //we haven't found either a concrete matching element, or an element with that ID.
                    //so end execution here, and return the unsatisfied change amount
                    return unsatisfiedChange;

                int originalQuantity = tokenToAffect.ElementStack.Quantity;
                tokenToAffect.ElementStack.ModifyQuantity(unsatisfiedChange, context);
                unsatisfiedChange += originalQuantity;

            }

            return unsatisfiedChange;
        }


        public static void CheckQuantityChangeIsNegative(string elementId, int quantityChange)
        {
            if (quantityChange >= 0)
                throw new ArgumentException("Tried to call ReduceElement for " + elementId + " with a >=0 change (" +
                                            quantityChange + ")");
        }

        public int IncreaseElement(string elementId, int quantityChange, Source stackSource, Context context,
            string locatorid = null)
        {

            if (quantityChange <= 0)
                throw new ArgumentException("Tried to call IncreaseElement for " + elementId + " with a <=0 change (" +
                                            quantityChange + ")");

            var newStack = ProvisionElementStackToken(elementId, quantityChange, stackSource, context);
            AcceptToken(newStack, context);
            return quantityChange;
        }

        public List<Token> GetStackTokens()
        {
            return _tokens.Where(s => !s.Defunct && s.ElementStack.IsValidElementStack()).ToList();
        }

        public List<ElementStack> GetElementStacks()
        {
            return GetStackTokens().Select(t => t.ElementStack).ToList();
        }

        public List<string> GetUniqueStackElementIds()
        {
            var stacks = _tokens.Where(t => t.ElementStack.IsValidElementStack());

            return stacks.Select(s => s.Element.Id).Distinct().ToList();
        }

        public List<string> GetStackElementIds()
        {
            return GetStackTokens().Select(s => s.Element.Id).ToList();
        }


        /// <summary>
        /// All the aspects in all the stacks, summing the aspects
        /// </summary>
        /// <returns></returns>
        public AspectsDictionary GetTotalAspects(bool includingSelf = true)
        {
            var stacks = _tokens.Where(t => t.ElementStack.IsValidElementStack());

            return AspectsDictionary.GetFromStacks(stacks.Select(s=>s.ElementStack), includingSelf);
        }


        public int GetTotalStacksCount()
        {
            return GetTotalElementsCount(x => true);
        }

        public int GetTotalStacksCountWithFilter(Func<ElementStack, bool> filter)
        {

            return GetStackTokens().Select(t=>t.ElementStack).Where(filter).Count();
        }

        public int GetTotalElementsCount()
        {
            return GetTotalElementsCount(x => true);

        }

        public int GetTotalElementsCount(Func<ElementStack, bool> filter)
        {
            return GetStackTokens().Select(t=>t.ElementStack).Where(filter).Sum(stack => stack.Quantity);

        }



        public int TryPurgeStacks(Element element, int maxToPurge)
        {

            if (string.IsNullOrEmpty(element.DecayTo))
            {
                //nb -p.value - purge max is specified as a positive cap, not a negative, for readability
                return ReduceElement(element.Id, -maxToPurge, new Context(Context.ActionSource.Purge));
            }
            else
            {
                int unsatisfiedChange = maxToPurge;
                while (unsatisfiedChange > 0)
                {

                    //nb: if we transform a stack of >1, it's possible maxToPurge/Transform will be less than the stack total - iwc it'll transform the whole stack. Probably fine.
                    ElementStack stackToAffect =
                        GetElementStacks().FirstOrDefault(c => !c.Defunct && c.GetAspects().ContainsKey(element.Id));

                    if (stackToAffect == null
                        ) //we haven't found either a concrete matching element, or an element with that ID.
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



    public virtual void AcceptToken(Token token, Context context)
        {
            
            token.SetSphere(this, context);

            if(token.ElementStack.IsValidElementStack())
            {

                if (EnforceUniqueStacksInThisContainer)
                {
                    var dealer = new Dealer(Registry.Get<Character>());
                    if (!String.IsNullOrEmpty(token.ElementStack.UniquenessGroup))
                        dealer.RemoveFromAllDecksIfInUniquenessGroup(token.ElementStack.UniquenessGroup);
                    if (token.ElementStack.Unique)
                        dealer.IndicateUniqueCardManifested(token.Element.Id);
                }

                // Check if we're dropping a unique stack? Then kill all other copies of it on the tabletop
                if (EnforceUniqueStacksInThisContainer)
                    RemoveDuplicates(token.ElementStack);

                // Check if the stack's elements are decaying, and split them if they are
                // Decaying stacks should not be allowed
                while (token.ElementStack.Decays && token.ElementStack.Quantity > 1)
                {
                    AcceptToken(token.SplitOffNCardsToNewStack(token.Quantity - 1, context), context);
                }

            }




            //sometimes, we reassign a stack to a container where it already lives. Don't add it again!
            if (!_tokens.Contains(token))
                _tokens.Add(token);

            DisplayHere(token, context);
            NotifyTokensChangedForContainer(new TokenEventArgs { Container = this });

        }


        public bool TryAcceptTokenAsThreshold(Token token)
        {

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForStack(token.ElementStack);

            if (match.MatchType != SlotMatchForAspectsType.Okay)
            {
                token.SetXNess(TokenXNess.DoesntMatchSlotRequirements);
                token.ReturnToStartPosition();

                var notifier = Registry.Get<INotifier>();

                var compendium = Registry.Get<ICompendium>();

                if (notifier != null)
                    notifier.ShowNotificationWindow(Registry.Get<ILocStringProvider>().Get("UI_CANTPUT"), match.GetProblemDescription(compendium), false);
            }
            else if (token.ElementQuantity != 1)
            {
                // We're dropping more than one?
                // set main stack to be returned to start position
                token.SetXNess(TokenXNess.ReturningSplitStack);
                // And we split a new one that's 1 (leaving the returning card to be n-1)
                var newStack = token.SplitOffNCardsToNewStack(token.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
                // And we put that into the slot
                AcceptToken(newStack, new Context(Context.ActionSource.PlayerDrag));
            }
            else
            {
                //it matches. Now we check if there's a token already there, and replace it if so:
                var currentOccupant = GetStackTokens().FirstOrDefault();

                // if we drop in the same slot where we came from, do nothing.
                if (currentOccupant == token)
                {
                    token.SetXNess(TokenXNess.ReturnedToStartingSlot);
                    return false;
                }

                if (currentOccupant != null)
                    NoonUtility.LogWarning("There's still a card in the slot when this reaches the slot; it wasn't intercepted by being dropped on the current occupant. Rework.");
                //currentOccupant.ReturnToTabletop();

                //now we put the token in the slot.
                token.SetXNess(TokenXNess.PlacedInSlot);
                AcceptToken(token, new Context(Context.ActionSource.PlayerDrag));
                SoundManager.PlaySfx("CardPutInSlot");
            }

            return true;
        }


        public void RemoveDuplicates(ElementStack incomingStack)
        {

            if (!incomingStack.Unique && string.IsNullOrEmpty(incomingStack.UniquenessGroup))
                return;

            foreach (var existingStack in new List<ElementStack>(GetElementStacks()))
            {

                if (existingStack != incomingStack && existingStack._element.Id == incomingStack._element.Id)
                {
                    NoonUtility.Log(
                        "Not the stack that got accepted, but has the same ID as the stack that got accepted? It's a copy!");
                    existingStack.Retire(RetirementVFX.CardHide);
                    return; // should only ever be one stack to retire!
                    // Otherwise this crashes because Retire changes the collection we are looking at
                }
                else if (existingStack != incomingStack && !string.IsNullOrEmpty(incomingStack.UniquenessGroup))
                {
                    if (existingStack.UniquenessGroup == incomingStack.UniquenessGroup)
                        existingStack.Retire(RetirementVFX.CardHide);

                }
            }
        }

        public void AcceptTokens(IEnumerable<Token> tokens, Context context)
        {
            foreach (var token in tokens)
            {
                AcceptToken(token, context);
            }
        }

        /// <summary>
        /// removes the stack from this stack manager; doesn't retire the stack
        /// </summary>
        /// <param name="stack"></param>
        public virtual void RemoveToken(Token token)
        {
            _tokens.Remove(token);
            NotifyTokensChangedForContainer(new TokenEventArgs {Container = this});
        }

        /// <summary>
        /// removes the stacks from this stack manager; doesn't retire the stack
        /// </summary>
        public void RemoveAllStacks()
        {
            var tokensListCopy = new List<Token>(_tokens);
            foreach (Token s in tokensListCopy)
                RemoveToken(s);
        }

        public void RetireAllTokens()
        {
            var listCopy = new List<Token>(_tokens);
            foreach (Token t in listCopy)
                t.Retire(RetirementVFX.None);
        }

        public void RetireTokensWhere(Func<Token, bool> filter)
        {
            var tokensToRetire = new List<Token>(_tokens).Where(filter);
            foreach (Token t in tokensToRetire)
                t.Retire(RetirementVFX.None);
        }


        /// <summary>
        /// This was relevant for a refactoring of the greedy slot code; I decided to do something else
        /// but this code might still be useful elsewhere!
        /// </summary>
        /// <param name="requirement"></param>
        /// <returns></returns>
        public List<ElementStack> GetStacksWithAspect(KeyValuePair<string, int> requirement)
        {
            List<ElementStack> matchingStacks = new List<ElementStack>();
            var candidateStacks = new List<ElementStack>(_tokens); //room here for caching
            foreach (var stack in candidateStacks)
            {
                int aspectAtValue = stack.GetAspects(true).AspectValue(requirement.Key);
                if (aspectAtValue >= requirement.Value)
                    matchingStacks.Add(stack);
            }

            return matchingStacks;
        }

        public virtual void ActivatePreRecipeExecutionBehaviour()
        {
            //eg slot consumptions

        }

        public ContainerMatchForStack GetMatchForStack(ElementStack stack)
        {
            if (!stack.IsValidElementStack())
                return new ContainerMatchForStack(new List<string>(), SlotMatchForAspectsType.ForbiddenAspectPresent);
            if (GoverningSlotSpecification == null)
                return ContainerMatchForStack.MatchOK();
            else
                return GoverningSlotSpecification.GetSlotMatchForAspects(stack.GetAspects());
        }

        public void NotifyTokensChangedForContainer(TokenEventArgs args)
        {
            Catalogue.NotifyTokensChangedForContainer(args);
            foreach(var s in _subscribers)
                s.NotifyTokensChangedForContainer(args);
        }

        public void OnTokenClicked(TokenEventArgs args)
        {
            Catalogue.OnTokenClicked(args);
            foreach (var s in _subscribers)
                s.OnTokenClicked(args);
        }

        public void OnTokenReceivedADrop(TokenEventArgs args)
        {
            Catalogue.OnTokenReceivedADrop(args);
            foreach (var s in _subscribers)
                s.OnTokenReceivedADrop(args);
        }

        public void OnTokenPointerEntered(TokenEventArgs args)
        {
            Catalogue.OnTokenPointerEntered(args);
            foreach (var s in _subscribers)
                s.OnTokenPointerEntered(args);
        }

        public void OnTokenPointerExited(TokenEventArgs args)
        {
            Catalogue.OnTokenPointerExited(args);
            foreach (var s in _subscribers)
                s.OnTokenPointerExited(args);
        }

        public void OnTokenDoubleClicked(TokenEventArgs args)
        {
            Catalogue.OnTokenDoubleClicked(args);
            foreach (var s in _subscribers)
                s.OnTokenDoubleClicked(args);
        }

        public void OnTokenDragged(TokenEventArgs args)
        {
            Catalogue.OnTokenDragged(args);
            foreach(var s in _subscribers)
                s.OnTokenDragged(args);
        }
    }

}

