using Assets.Logic;
using Assets.Scripts.Application.Fucine;
using Assets.Scripts.Application.Infrastructure.SimpleJsonGameDataImport;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.NullObjects;
using Steamworks;
using UnityEngine;

namespace SecretHistories.Spheres
{


    public enum BlockReason
    {
      InboundTravellingStack
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

    [IsEncaustableClass(typeof(SphereCreationCommand))]
    public abstract class 
        Sphere : MonoBehaviour,IEncaustable,IHasFucinePath
    {

        [DontEncaust] public string Id => GoverningSphereSpec.Id;

            [Encaust]
        public SphereSpec GoverningSphereSpec { get; set; }
        [Encaust]
        public List<Token> Tokens
        {
            get { return new List<Token>(_tokens); }
        }
        /// <summary>
        /// This is used for child thresholds, and other spheres that depend for their temporary existence on the state of another sphere.
        /// </summary>
        [Encaust]
        public string OwnerSphereIdentifier { get; set; }

        private IHasAspects _container = NullSituation.Create();

   
        [DontEncaust]
        public bool Defunct { get; protected set; }
        [DontEncaust]
        public virtual bool AllowDrag { get; private set; }
        [DontEncaust]
        public virtual bool AllowStackMerge => true;
        [DontEncaust]
        public virtual bool PersistBetweenScenes => false;
        [DontEncaust]
        public virtual bool EnforceUniqueStacksInThisContainer => true;
        [DontEncaust]
        public virtual bool ContentsHidden => false;
        [DontEncaust]
        public virtual bool IsGreedy => false;
        [DontEncaust]
        public virtual float TokenHeartbeatIntervalMultiplier => 0;
        [DontEncaust]
        public abstract SphereCategory SphereCategory { get; }
        [DontEncaust]
        public virtual IChoreographer Choreographer { get; set; } = new SimpleChoreographer();

        public Sphere GetEnRouteSphere()
        {
            if(GoverningSphereSpec.EnRouteSpherePath.IsValid())
                return Watchman.Get<HornedAxe>().GetSphereByPath(GoverningSphereSpec.EnRouteSpherePath);

            return _container.Token.Sphere.GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            if (GoverningSphereSpec.EnRouteSpherePath.IsValid())
                return Watchman.Get<HornedAxe>().GetSphereByPath(GoverningSphereSpec.WindowsSpherePath);

            return _container.Token.Sphere.GetWindowsSphere();
        }


        public GameObject GreedyIcon;
        public GameObject ConsumingIcon;

        public IHasAspects GetContainer()
        {

            return _container;
        }


        public void SetContainer(IHasAspects newContainer)
        {
            if(newContainer==null)
                NoonUtility.LogWarning($"We're trying to set null as a container for sphere {Id} / {gameObject.name}");

            var oldContainer = _container;
            if (oldContainer == newContainer)
                return;
            
            _container = newContainer;
            oldContainer.DetachSphere(this);
        }

        public FucinePath GetAbsolutePath()
        {
            return _container.GetAbsolutePath().AppendSphere(Id);
        }


        public virtual bool IsInRangeOf(Sphere otherSphere)
        {
            return true;
        }

        protected HashSet<ContainerBlock> _currentContainerBlocks = new HashSet<ContainerBlock>();

        protected readonly List<Token> _tokens = new List<Token>();
        protected AngelFlock flock = new AngelFlock();

        private readonly HashSet<ISphereEventSubscriber> _subscribers = new HashSet<ISphereEventSubscriber>();


        private Dictionary<FucinePath, Vector3> referencePositions=new Dictionary<FucinePath, Vector3>();



        public void Subscribe(ISphereEventSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(ISphereEventSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public void AddAngel(IAngel angel)
        {
            flock.AddAngel(angel);
        }

        public void RemoveAngel(IAngel angel)
        {
            flock.RemoveAngel(angel);
        }

        public virtual List<SphereSpec> GetChildSpheresSpecsToAddIfThisTokenAdded(Token t,string verbId)
        {
            var elementInToken = Watchman.Get<Compendium>().GetEntityById<Element>(t.Payload.Id);

            var childSlotSpecs = elementInToken.Slots.Where(cs => cs.ActionId == verbId || cs.ActionId == string.Empty).ToList();
            return childSlotSpecs;
        }


        public virtual void ApplySpec(SphereSpec spec)
        {
            if(string.IsNullOrEmpty(spec.Id))
                NoonUtility.LogWarning("PROBLEM: null sphere id passed in SphereSpec for sphere " + gameObject.name + " in container " + GetContainer().Id);

            GoverningSphereSpec = spec;
        }

        public virtual bool Retire(SphereRetirementType sphereRetirementType)
        {
            if(sphereRetirementType!=SphereRetirementType.Destructive && sphereRetirementType!=SphereRetirementType.Graceful)
                NoonUtility.LogWarning("Unknown sphere retirement type: " + sphereRetirementType);

            if(sphereRetirementType==SphereRetirementType.Destructive)
                RetireAllTokens();
            else
                EvictAllTokens(new Context(Context.ActionSource.ContainingSphereRetired));

            Watchman.Get<HornedAxe>().DeregisterSphere(this);

                Defunct = true;
            Destroy(gameObject);
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
      

        

        // Returns a rect for use by the Choreographer
        public Rect GetRect()
        {
            return GetRectTransform().rect;
        }


        public RectTransform GetRectTransform()
        {
            var rectTrans = transform as RectTransform;
            if (rectTrans == null)
                NoonUtility.LogWarning("Tried to get a recttransform for " + name + ", but it doesn't have one.");
            return rectTrans;
        }

        public virtual void DisplayAndPositionHere(Token token, Context context)
        {
            token.Manifest();
            token.transform.SetParent(transform,true); //this is the default: specifying for clarity in case I revisit
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;

        }

        public virtual void TryMoveAsideFor(Token potentialUsurper, Token incumbent,
            out bool incumbentMoved)
        {
            // By default: do no move-aside
            incumbentMoved = false;
        }

        public virtual void OnDestroy()
        {
            Watchman.Get<HornedAxe>().DeregisterSphere(this);
        }

        public void ModifyElementQuantity(string elementId, int quantityChange, Context context)
        {
            if (quantityChange > 0)
                IncreaseElement(elementId, quantityChange, context);
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
                    _tokens.FirstOrDefault(c => !c.Defunct && c.Payload.GetAspects(true).ContainsKey(elementId));

                if (tokenToAffect == null
                    ) //we haven't found either a concrete matching element, or an element with that ID.
                    //so end execution here, and return the unsatisfied change amount
                    return unsatisfiedChange;

                int originalQuantity = tokenToAffect.Payload.Quantity;
                tokenToAffect.Payload.ModifyQuantity(unsatisfiedChange, context);
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

        public virtual int IncreaseElement(string elementId, int quantityChange, Context context)
        {

            if (quantityChange <= 0)
                throw new ArgumentException("Tried to call IncreaseElement for " + elementId + " with a <=0 change (" +
                                            quantityChange + ")");

            var t=new TokenCreationCommand().WithElementStack(elementId,quantityChange);
            t.Execute(context,this);

            
            return quantityChange;
        }

        public bool IsEmpty()
        {
            return _tokens.Count <= 0;
        }



        public List<Token> GetElementTokens()
        {
            return _tokens.Where(s => !s.Defunct && s.IsValidElementStack()).ToList();
        }

        public List<ITokenPayload> GetElementStacks()
        {
            return GetElementTokens().Where(t=>t.IsValidElementStack()).Select(t => t.Payload).ToList();
        }

        public List<string> GetUniqueStackElementIds()
        {
            var stacks = _tokens.Where(t => t.Payload.IsValidElementStack());

            return stacks.Select(s => s.Payload.Id).Distinct().ToList();
        }

        public List<string> GetStackElementIds()
        {
            return GetElementTokens().Select(s => s.Payload.Id).ToList();
        }


        /// <summary>
        /// All the aspects in all the stacks, summing the aspects
        /// </summary>
        /// <returns></returns>
        public AspectsDictionary GetTotalAspects(bool includingSelf = true)
        {
            var stacks = _tokens.Where(t => t.Payload.IsValidElementStack());

            return AspectsDictionary.GetFromStacks(stacks.Select(s=>s.Payload), includingSelf);
        }


        public int GetTotalStacksCount()
        {
            return GetTotalElementsCount(x => true);
        }

        public int GetTotalStacksCountWithFilter(Func<ITokenPayload, bool> filter)
        {

            return GetElementTokens().Select(t=>t.Payload).Where(filter).Count();
        }
        /// <summary>
        /// total of (stacks*quantity of each stack)
        /// </summary>
        public int GetTotalElementsCount()
        {
            return GetTotalElementsCount(x => true);

        }
        /// <summary>
        /// total of (stacks*quantity of each stack)
        /// </summary>
        public int GetTotalElementsCount(Func<ITokenPayload, bool> filter)
        {
            return GetElementTokens().Select(t=>t.Payload).Where(filter).Sum(stack => stack.Quantity);

        }


        public void RequestFlockActions(float interval)
        {
            flock.Act(interval);
        }

        public void RequestTokensSpendTime(float interval)
        {

            var tokens = Tokens;
    
            foreach (var d in tokens)
                d.ExecuteHeartbeat(interval);
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
                    var elementStackTokenToAffect =
                        GetElementTokens().FirstOrDefault(c => !c.Defunct && c.GetAspects().ContainsKey(element.Id));

                    if (elementStackTokenToAffect == null
                        ) //we haven't found either a concrete matching element, or an element with that ID.
                        //so end execution here, and return the unsatisfied change amount
                        return unsatisfiedChange;

                    int originalQuantity = elementStackTokenToAffect.Quantity;
                    elementStackTokenToAffect.Purge();
                    //stackToAffect.Populate(element.DecayTo, stackToAffect.Quantity, Source.Existing());
                    unsatisfiedChange -= originalQuantity;
                }

                return unsatisfiedChange;
            }



        }


    public virtual void AcceptToken(Token token, Context context)
        {
            
            token.SetSphere(this, context);

            if(token.IsValidElementStack())
            {

                if (EnforceUniqueStacksInThisContainer)
                {
                    var dealer = new Dealer(Watchman.Get<Stable>().Protag());
                    if (!String.IsNullOrEmpty(token.Payload.UniquenessGroup))
                        dealer.RemoveFromAllDecksIfInUniquenessGroup(token.Payload.UniquenessGroup);
                    if (token.Payload.Unique)
                        dealer.IndicateUniqueCardManifested(token.Payload.Id);
                }

                // Check if we're dropping a unique stack? Then kill all other copies of it on the tabletop
                if (EnforceUniqueStacksInThisContainer)
                    RemoveDuplicates(token.Payload);

                // Check if the stack's elements are decaying, and split them if they are
                // Decaying stacks should not be allowed
                while (token.Payload.GetTimeshadow().Transient && token.Quantity > 1)
                {
                    AcceptToken(token.CalveToken(1,context),context);
                }

            }

            //sometimes, we reassign a stack to a container where it already lives. Don't add it again!
            if (!_tokens.Contains(token))
                _tokens.Add(token);

            var args = new SphereContentsChangedEventArgs(this, context);
            args.TokenAdded = token;
            NotifyTokensChangedForSphere(args);
            DisplayAndPositionHere(token, context);

        }


        public virtual bool TryAcceptToken(Token token,Context context)
        {
            AcceptToken(token,context);
            return true;
        }

        /// <summary>
        /// 'Send token away. Find somewhere suitable. Not my problem.' The default implementation hands them off to the default en route sphere, which will have routing angels.
        /// </summary>
        public virtual void EvictToken(Token token, Context context)
        {
            var exitSphere = GetEnRouteSphere();
            exitSphere.ProcessEvictedToken(token,context);
       
        }

        /// <summary>
        /// 'What do we do with this homeless token? either accept it, or move it on.'
        /// </summary>
        public virtual bool ProcessEvictedToken(Token token, Context context)
        {
            if (flock.MinisterToEvictedToken(token, context))
                return true;

            var existingElementTokens = GetElementTokens();

            //check if there's an existing stack of that type to merge with
            foreach (var elementToken in existingElementTokens)
            {
                if (token.Payload.CanMergeWith(elementToken.Payload))
                {
                    elementToken.Payload.InteractWithIncoming(token);
                    return true;
                }
            }

            var targetFreePosition= Choreographer.GetFreeLocalPosition(token, token.TokenRectTransform.anchoredPosition3D);

            TokenTravelItinerary journeyToFreePosition =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, targetFreePosition)
                    .WithSphereRoute(token.Sphere,this).
                    WithDuration(NoonConstants.MOMENT_TIME_INTERVAL);
            journeyToFreePosition.DestinationSphere = this;
            journeyToFreePosition.Depart(token,context);
            return true;
        }


        public void RemoveDuplicates(ITokenPayload incomingStack)
        {

            if (!incomingStack.Unique && string.IsNullOrEmpty(incomingStack.UniquenessGroup))
                return;

            foreach (var existingStack in new List<ITokenPayload>(GetElementStacks()))
            {

                if (existingStack != incomingStack && existingStack.Id == incomingStack.Id)
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
        public virtual void RemoveToken(Token token,Context context)
        {
            _tokens.Remove(token);
            var args= new SphereContentsChangedEventArgs(this,context);
            args.TokenRemoved = token;
            NotifyTokensChangedForSphere(args);
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

        public void EvictAllTokens(Context context)
        {
            var listCopy = new List<Token>(_tokens);
            foreach (Token t in listCopy)
                t.GoAway(context);
        }


        public ContainerMatchForStack GetMatchForTokenPayload(ITokenPayload payload)
        {

            if (GoverningSphereSpec == null)
                return ContainerMatchForStack.MatchOK();
            else
                return GoverningSphereSpec.CheckPayloadAllowedHere(payload);
        }

        public void NotifyTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            Watchman.Get<HornedAxe>().OnTokensChangedForSphere(args);
            var subscribersToNotify=new HashSet<ISphereEventSubscriber>(_subscribers);

            foreach(var s in subscribersToNotify)
                s.OnTokensChangedForSphere(args);
        }

        public virtual void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            Watchman.Get<HornedAxe>().OnTokenInteractionInSphere(args);

            var subscribersToNotify = new HashSet<ISphereEventSubscriber>(_subscribers);
            foreach (var s in subscribersToNotify)
                s.OnTokenInteractionInSphere(args);
        }

        /// <summary>
        /// Reference positions: positions in other spheres that correspond to this one.
        /// eg, the position in TabletopSphere that tokens send, so we know what point thresholds are connected to
        /// </summary>
        /// <param name="referenceLocation"></param>
        public void SetReferencePosition(TokenLocation referenceLocation)
        {
            if (referenceLocation.AtSpherePath != this.GetAbsolutePath())
            {
                if (referencePositions.ContainsKey(referenceLocation.AtSpherePath))
                    referencePositions[referenceLocation.AtSpherePath] = referenceLocation.Anchored3DPosition;
                else
                    referencePositions.Add(referenceLocation.AtSpherePath, referenceLocation.Anchored3DPosition);
            }
        }

        public Vector3 GetReferencePosition(FucinePath atPath)
        {

            if (referencePositions.TryGetValue(atPath, out Vector3 referencePosition))
                return referencePosition;

            var here = GetRectTransform().anchoredPosition3D;

            var hereAsWorldPosition = GetRectTransform().TransformPoint(here);
            var otherSphere = Watchman.Get<HornedAxe>().GetSphereByPath(atPath);
            var bestGuessReferencePosition = otherSphere.GetRectTransform().InverseTransformPoint(hereAsWorldPosition);
            
            return bestGuessReferencePosition;
        }

        public void Shroud()
        {
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        public void Reveal()
        {
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }


        public bool IsInRoot()
        {
            return GetAbsolutePath().IsPathToSphereInRoot();
        }
    }

}

