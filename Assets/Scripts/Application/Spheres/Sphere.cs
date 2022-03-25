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
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Events;
using SecretHistories.Infrastructure;
using SecretHistories.Manifestations;
using SecretHistories.NullObjects;
using SecretHistories.States;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecretHistories.Spheres
{


    [IsEncaustableClass(typeof(SphereCreationCommand))]
    public abstract class 
        Sphere : MonoBehaviour,IEncaustable,IHasFucinePath,IHasElementTokens
    {

        [DontEncaust] public string Id => GoverningSphereSpec.Id;

            [Encaust]
        public SphereSpec GoverningSphereSpec { get; set; }
        [Encaust]
        public List<Token> Tokens
        {
            get { return new List<Token>(_tokens.Where(t=>!t.Defunct)); }
        }
        /// <summary>
        /// This is used for child thresholds, and other spheres that depend for their temporary existence on the state of another sphere.
        /// </summary>
        [Encaust]
        public string OwnerSphereIdentifier { get; set; }

        protected IHasAspects _container = NullSituation.Create();

   
        [DontEncaust]
        public bool Defunct { get; protected set; }

        [DontEncaust] public virtual bool IsValid => true;


        [DontEncaust]
        public virtual bool AllowDrag { get; private set; }
        [DontEncaust]
        public virtual bool AllowStackMerge => true;
        [DontEncaust]
        public virtual bool AllowAmbientAnimations => false;

        [DontEncaust]
        public virtual bool EmphasiseContents => false;
        [DontEncaust]
        public virtual bool UnderstateContents => false;

        [DontEncaust]
        public virtual bool PersistBetweenScenes => false;
        [DontEncaust]
        public virtual bool EnforceUniqueStacksInThisContainer => true;
        [DontEncaust]
        public virtual bool ContentsHidden => false;
        [DontEncaust]
        public virtual bool Traversable => false;
        [DontEncaust]
        public virtual float TokenHeartbeatIntervalMultiplier => 0;
        [DontEncaust]
        public abstract SphereCategory SphereCategory { get; }

        [DontEncaust]
        public virtual AbstractChoreographer Choreographer => gameObject.GetComponent<AbstractChoreographer>();


        public virtual Type GetDefaultManifestationType()
        {
            return typeof(CardManifestation);
        }

        public Vector3 WorldPosition;

        [SerializeField]
        private string _editorAbsolutePath;
        [SerializeField]
        private string _editorWildPath;


        public virtual void Awake()
        {
            var existingChoreographer = gameObject.GetComponent<AbstractChoreographer>();
            if(existingChoreographer==null)
                gameObject.AddComponent<SimpleChoreographer>();

        }

        public void Update()
        {
            if(GetRectTransform()!=null)
                WorldPosition = GetRectTransform().position;
        }


       /// <summary>
       /// This is an arbitrary list of GameObjects tagged with VisibleCharacteristic which can be used to display the presence of angels or anything else that wants to make use of them.
       /// </summary>
       [SerializeField] private List<VisibleCharacteristic> VisibleCharacteristics;
   /// <param name="angel"></param>
       public void ShowAngelPresence(IAngel angel)
       {
           angel.ShowRelevantVisibleCharacteristic(VisibleCharacteristics);
       }


       public void HideAngelPresence(IAngel angel)
       {
           angel.HideRelevantVisibleCharacteristic(VisibleCharacteristics);
       }

       public virtual HomingAngel TryCreateHomingAngelFor(Token forToken)
       {
           //override if we don't want the homing token to go back there (eg as with OutputSphere)

         var  _homingAngel = new HomingAngel(forToken);
           _homingAngel.SetWatch(this);
           AddAngel(_homingAngel);
           return _homingAngel;
       }

        public IHasAspects GetContainer()
        {
            return _container;
        }

        public string GetDeckSpecId()
        {
            return GoverningSphereSpec.ActionId;
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

            _editorAbsolutePath = GetAbsolutePath().ToString();
            _editorWildPath = GetWildPath().ToString();
        }

        public FucinePath GetAbsolutePath()
        { 
            return _container.GetAbsolutePath().AppendingSphere(Id);
        }

        public FucinePath GetWildPath()
        {
            var wildCardPath = _container.GetWildPath().AppendingSphere(Id);
            return wildCardPath;
        }
        
        public virtual bool IsValidDestinationForToken(Token tokenToSend)
        {
            if (!IsInRangeOf(tokenToSend.Sphere))
                return false;
            if (CurrentlyBlockedFor(BlockDirection.Inward))
                return false;
            if(GetMatchForTokenPayload(tokenToSend.Payload).MatchType != SlotMatchForAspectsType.Okay)
                return false;


            return true;
        }

        public virtual bool IsInRangeOf(Sphere otherSphere)
        {
            if (!this.gameObject.activeInHierarchy) //This is a pretty sensible rule, and it means that (eg) drydock thresholds are considered out of range when their parent is hidden.
            //I will likely need to modify it for specific cases, though.
                return false;
            return true;
        }



        protected readonly List<Token> _tokens = new List<Token>();
        protected AngelFlock flock = new AngelFlock();

        private readonly HashSet<ISphereEventSubscriber> _subscribers = new HashSet<ISphereEventSubscriber>();
        private AbstractChoreographer _choreographer;


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


        public virtual List<SphereSpec> GetChildSpheresSpecsToAddIfThisTokenAdded(Token t,string verbId)
        {
            var elementInToken = Watchman.Get<Compendium>().GetEntityById<Element>(t.Payload.EntityId);

            var childSlotSpecs = elementInToken.Slots.Where(cs => cs.ActionId == verbId || cs.ActionId == string.Empty).ToList();
            return childSlotSpecs;
        }


        public virtual void SetPropertiesFromSpec(SphereSpec spec)
        {
            if(string.IsNullOrEmpty(spec.Id))
                NoonUtility.LogWarning("PROBLEM: null sphere id passed in SphereSpec for sphere " + gameObject.name + " in container " + GetContainer().Id);

            AddAngelsFromSpec(spec);

            GoverningSphereSpec = spec;
        }

        public virtual void AddAngelsFromSpec(SphereSpec spec)
        {
            var angelsToAdd = spec.MakeAngels(this);
            foreach (var a in angelsToAdd)
                AddAngel(a);
        }

        public virtual void Retire(SphereRetirementType sphereRetirementType)
        {
            if (Defunct)
                return;

            Defunct = true;
            Watchman.Get<HornedAxe>().DeregisterSphere(this);
            flock.RetireAllAngels();
            //we used to add a block at this point. But if the sphere is deregistered and all the angels are inactive, that shouldn't be necessary;
            //And if we add a retirement-specific block, we then need to remove it from the Xamanek if a slot at the same path is recreated!

            DoRetirement(FinishRetirement,sphereRetirementType);

        }

        public virtual void DoRetirement(Action<SphereRetirementType> onRetirementComplete,SphereRetirementType retirementType)
        {
            HandleContentsGracefully(retirementType);
            onRetirementComplete(retirementType);
        }

        protected void HandleContentsGracefully(SphereRetirementType retirementType)
        {
            if (retirementType != SphereRetirementType.Destructive && retirementType != SphereRetirementType.Graceful)
                NoonUtility.LogWarning("Unknown sphere retirement type: " + retirementType);

            if (retirementType == SphereRetirementType.Destructive)
                RetireAllTokens();
            else
                EvictAllTokens(new Context(Context.ActionSource.ContainingSphereRetired));
        }


        protected virtual void FinishRetirement(SphereRetirementType retirementType)
        {
                HandleContentsGracefully(retirementType);
            if(Application.isPlaying) //don't destroy objects if we're in Edit Mode
                                           Destroy(gameObject,0.1f); //For some reason, destroying the sphere
                                      // when a token has just been removed from it borks the token
                                      //waiting a tenth of a second avoids this.
                                      //NOW I UNDERSTAND: in circumstances like the Egress sphere, where the destruction
                                      //happens sufficiently quickly, the token is still a child of the sphere transform.
                                      //so destroying the sphere starts destroying it, too.
                                      //We need a way to ensure this never happens, and then we can remove this tenth-of-a-second hack.

        }

        public void AddBlock(BlockDirection blockDirection,BlockReason blockReason)
        {
            if(this.CurrentlyBlockedForDirectionWithAnyReasonExcept(blockDirection,blockReason))
                return;
            //Possible edge case; what if we're blocked for All and we wanted to add something specifically for Inward, e.g.?

            var block = new SphereBlock(GetWildPath(), blockDirection, blockReason);
            Watchman.Get<Xamanek>().RegisterSphereBlock(block);
        }

        public int RemoveMatchingBlocks(BlockDirection blockDirection, BlockReason blockReason)
        {
          return  Watchman.Get<Xamanek>().RemoveMatchingBlocks(GetWildPath(), blockDirection, blockReason);
        }

        public virtual bool CurrentlyBlockedFor(BlockDirection direction)
        {
            return CurrentlyBlockedForDirectionWithAnyReasonExcept(direction, BlockReason.None);
        }

        public virtual bool CurrentlyBlockedForDirectionWithAnyReasonExcept(BlockDirection direction, BlockReason exceptReason)
        {
            
            var allBlocks = new List<SphereBlock>();
            allBlocks.AddRange(Watchman.Get<Xamanek>().GetBlocksForSphereAtPath(GetWildPath()));
            allBlocks.AddRange(flock.GetImplicitAngelBlocks());

            foreach (var b in allBlocks)
            {
                if (b.BlockDirection == direction || b.BlockDirection == BlockDirection.All)
                
                    if(b.BlockReason!=exceptReason)
                        return true;
            }

            return false;
        }


        public bool IsWorldPointInBoundingRect(Vector2 worldPoint)
        {
            var r = GetRect();
            var rt = GetRectTransform();
          
            float leftSide = rt.position.x - r.width / 2;
            float rightSide = rt.position.x + r.width / 2;
            float topSide = rt.position.y + r.height / 2;
            float bottomSide = rt.position.y - r.height / 2;

            if (worldPoint.x >= leftSide &&
                worldPoint.x <= rightSide &&
                worldPoint.y >= bottomSide &&
                worldPoint.y <= topSide)

                return true;

            return false;

        }

        public Rect GetRect()
        {
            return GetRectTransform().rect;
        }


        public RectTransform GetRectTransform()
        { 
            var rectTrans = transform as RectTransform;

            return rectTrans;
        }

        public virtual void DisplayAndPositionHere(Token token, Context context)
        {
            token.Manifest();


           token.transform.SetParent(transform,true); //'true' is the default: specifying for clarity in case I revisit

            token.transform.localRotation = Quaternion.identity;
            token.SetLocalScale(Vector3.one);
      
            Choreographer.PlaceTokenAsCloseAsPossibleToSpecifiedPosition(token,context,token.TokenRectTransform.anchoredPosition);
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
            else if(quantityChange<0)
                ReduceElement(elementId, quantityChange, context);
            else
                NoonUtility.Log($"ModifyElementQuantity {quantityChange} called for {elementId}, which might be a mistake");
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
                ElementStack stackToAffect =GetElementStacks().FirstOrDefault(c => !c.Defunct && c.GetAspects(true).ContainsKey(elementId));

                if (stackToAffect == null
                    ) //we haven't found either a concrete matching element, or an element with that ID.
                    //so end execution here, and return the unsatisfied change amount
                    return unsatisfiedChange;

                int originalQuantity = stackToAffect.Quantity;
                stackToAffect.ModifyQuantity(unsatisfiedChange, context);
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

        public List<ElementStack> GetElementStacks()
        {
            return GetElementTokens().Where(t=>t.IsValidElementStack()).Select(t => t.Payload as ElementStack).ToList();
        }

        public Token ProvisionElementToken(string elementId, int count)
        {
            var tc = new TokenCreationCommand().WithElementStack(elementId, count);
            var token=tc.Execute(Context.Unknown(), this);
            return token;
        }

        public List<string> GetUniqueStackElementIds()
        {
            var stacks = _tokens.Where(t => t.Payload.IsValidElementStack());

            return stacks.Select(s => s.Payload.EntityId).Distinct().ToList();
        }

        public List<string> GetStackElementIds()
        {
            return GetElementTokens().Select(s => s.Payload.EntityId).ToList();
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

        public bool HasAngel(Type angelType)
        {
            return flock.HasAngel(angelType);
        }

        public void RequestFlockActions(float seconds, float metaseconds)
        {
            flock.Act(seconds, metaseconds);
         
        }

        public void RequestTokensSpendTime(float seconds, float metaseconds)
        {

            var tokens = Tokens;
    
            foreach (var t in tokens)
                t.ExecuteHeartbeat(seconds, metaseconds);
        }

        public int TryPurgeStacks(Element element, int maxToPurge)
        {

            if (string.IsNullOrEmpty(element.DecayTo))
            {
                //nb -p.value - purge max is specified as a positive cap, not a negative, for readability
                //This means we also need to reverse any unsatisfied change returned, since ReduceElement deals in negatives.
                return -ReduceElement(element.Id, -maxToPurge, new Context(Context.ActionSource.Purge));
            }
            else
            {
                int unsatisfiedPurge = maxToPurge;
                while (unsatisfiedPurge > 0)
                {

                    //nb: if we transform a stack of >1, it's possible maxToPurge/Transform will be less than the stack total - iwc it'll transform the whole stack. Probably fine.
                    var elementStackTokenToAffect =
                        GetElementTokens().FirstOrDefault(c => !c.Defunct && c.GetAspects().ContainsKey(element.Id));

                    if (elementStackTokenToAffect == null
                        ) //we haven't found either a concrete matching element, or an element with that ID.
                        //so end execution here, and return the unsatisfied change amount
                        return unsatisfiedPurge;

                    int originalQuantity = elementStackTokenToAffect.Quantity;
                    elementStackTokenToAffect.ApplyExoticEffect(ExoticEffect.Purge);
                    //stackToAffect.Populate(element.DecayTo, stackToAffect.Quantity, Source.Existing());
                    unsatisfiedPurge -= originalQuantity;
                }

                return unsatisfiedPurge;
            }

        }


    public virtual void AcceptToken(Token token, Context context)
        {
            
            token.SetSphere(this, context);

            if (token.Defunct) //possibly the token was destroyed as it left the previous sphere - eg by a consuming angel.
                return;

            if(token.IsValidElementStack())
            {

                if (EnforceUniqueStacksInThisContainer)
                {
                    var dealer = new Dealer(Watchman.Get<DealersTable>());
                    if (!String.IsNullOrEmpty(token.Payload.UniquenessGroup))
                        dealer.IndicateElementInUniquenessGroupManifested(token.Payload.EntityId, token.Payload.UniquenessGroup);
                    if (token.Payload.Unique)
                        dealer.IndicateUniqueElementManifested(token.Payload.EntityId);
                }

                // Check if we're dropping a unique stack? Then kill all other copies of it on the tabletop
                if (EnforceUniqueStacksInThisContainer)
                    RemoveDuplicates(token.Payload);


            }

            //sometimes, we reassign a stack to a container where it already lives. Don't add it again!
            if (!_tokens.Contains(token))
                _tokens.Add(token);

            var args = new SphereContentsChangedEventArgs(this, context);
            args.TokenAdded = token;
            
            DisplayAndPositionHere(token, context);

            if(EmphasiseContents)
                token.Emphasise();
            if(UnderstateContents)
                token.Understate();
            

            NotifyTokensChangedForSphere(args);

        }




        public virtual bool TryAcceptToken(Token token,Context context)
        {
            if (Defunct)
                return false;
            AcceptToken(token,context);
            return true;
            
        }

        /// <summary>
        /// 'Send token away. Find somewhere suitable. Not my problem.' The default implementation hands them off to the default en route sphere, which will have routing angels.
        /// </summary>
        public virtual void EvictToken(Token token, Context context)
        {
            if (token.CurrentState.InSystemDrivenMotion())
                return; //it's already en route somewhere)

            var exitSphere = token.Payload.GetEnRouteSphere();
            exitSphere.ProcessEvictedToken(token,context);
            
        }

        /// <summary>
        /// 'What do we do with this homeless token? either accept it, or move it on.'
        /// </summary>
        public virtual bool ProcessEvictedToken(Token token, Context context)
        {
            if (token.TryFulfilGhostPromise(context))
                return true;

 
            if (flock.MinisterToEvictedToken(token, context))
                return true;
            

            var targetFreePosition= Choreographer.GetClosestFreeLocalPosition(token, token.TokenRectTransform.anchoredPosition3D);

            TokenTravelItinerary journeyToFreePosition =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, targetFreePosition)
                    .WithDestinationSpherePath(this.GetAbsolutePath()).
                    WithDuration(NoonConstants.MOMENT_TIME_INTERVAL);
            journeyToFreePosition.DestinationSpherePath = this.GetAbsolutePath();
            journeyToFreePosition.Depart(token,context);
            return true;
        }


        public void RemoveDuplicates(ITokenPayload incomingStack)
        {

            if (!incomingStack.Unique && string.IsNullOrEmpty(incomingStack.UniquenessGroup))
                return;

            foreach (var existingStack in new List<ITokenPayload>(GetElementStacks()))
            {

                if (existingStack != incomingStack && existingStack.EntityId == incomingStack.EntityId)
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
            flock.MinisterToDepartingToken(token, context);
            var args= new SphereContentsChangedEventArgs(this,context);
            args.TokenRemoved = token;
            NotifyTokensChangedForSphere(args);
        }


        public virtual void RetireAllTokens()
        {
            var listCopy = new List<Token>(_tokens);
            foreach (Token t in listCopy)
                t.Retire(RetirementVFX.None);
        }

        public IEnumerable<Token> GetTokens()
        {
            return _tokens.Where(t=>true);
        }

        public IEnumerable<Token> GetTokensWhere(Func<Token,bool> filter)
        {
            return _tokens.Where(filter);
        }

        public void RetireTokensWhere(Func<Token, bool> filter)
        {
            var tokensToRetire = new List<Token>(_tokens).Where(filter);
            foreach (Token t in tokensToRetire)
            {
                NoonUtility.Log($"Retiring {t.PayloadId} from {this.Id}");
                t.Retire(RetirementVFX.None);
            }

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


        public void NotifySphereChanged(SphereChangedArgs args)
        {
            Watchman.Get<HornedAxe>().OnAnySphereChanged(args);
            var subscribersToNotify = new HashSet<ISphereEventSubscriber>(_subscribers);
            foreach (var s in subscribersToNotify)
                if (s.Equals(null))
                    _subscribers.Remove(s);
                else
                    s.OnSphereChanged(args);

        }

        public void NotifyTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            Watchman.Get<HornedAxe>().OnTokensChangedForAnySphere(args);
            var subscribersToNotify=new HashSet<ISphereEventSubscriber>(_subscribers);
            foreach(var s in subscribersToNotify)
                if (s.Equals(null))
                    _subscribers.Remove(s);
                else
                    s.OnTokensChangedForSphere(args);
        }

        public virtual void NotifyTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            Watchman.Get<HornedAxe>().OnTokenInteractionInAnySphere(args);
            var subscribersToNotify = new HashSet<ISphereEventSubscriber>(_subscribers);
            foreach (var s in subscribersToNotify)
                if (s.Equals(null))
                    _subscribers.Remove(s);
                else
                    s.OnTokenInteractionInSphere(args);
        }

 
        public virtual TokenTravelItinerary GetItineraryFor(Token forToken)
        {
            var hereAsWorldPosition = GetRectTransform().position;

            var currentSphere = Watchman.Get<HornedAxe>().GetSphereByPath(forToken.Location.AtSpherePath);
            var otherSphereTransform = currentSphere.GetRectTransform();
            var bestGuessReferencePosition = otherSphereTransform.InverseTransformPoint(hereAsWorldPosition);

            TokenTravelItinerary itinerary = new TokenTravelItinerary(forToken.Location.Anchored3DPosition,
                    bestGuessReferencePosition)
                .WithDuration(NoonConstants.SEND_STACK_TO_SLOT_DURATION)
                .WithDestinationSpherePath(GetAbsolutePath());

            return itinerary;
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

        public virtual void Emphasise()
        {
            //
        }

        public virtual void Understate()
        {
            //
        }

        public bool IsInRoot()
        {
            return GetAbsolutePath().IsPathToSphereInRoot();
        }
        public virtual bool TryDisplayDropInteractionHere(Token forToken)
        {
 
            return false; //most spheres won't show a ghost but also won't hide it if it's extant.
        }
        public virtual bool DisplayGhostAt(Token forToken,Vector3 overridingWorldPosition)
        {
            return false; //most spheres won't show a ghost
        }

        public virtual Type GetShabdaManifestation(Situation situation)
        {
            return typeof(VerbManifestation);
        }
    }

}

