#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.States;
using SecretHistories.Constants;
using SecretHistories.States.TokenStates;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres.Angels;
using SecretHistories.Spheres;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace SecretHistories.UI {

    [RequireComponent(typeof(RectTransform))]
    public class Token : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler,
        IPointerExitHandler, ISituationSubscriber, IInteractsWithTokens
    {
        private float previousClickTime = 0f;



        [Header("Location")]
        [SerializeField] public RectTransform TokenRectTransform;
        public RectTransform ManifestationRectTransform => _manifestation.RectTransform;
        public TokenLocation Location => new TokenLocation(TokenRectTransform.anchoredPosition3D, Sphere.GetPath());
        public Sphere Sphere;
        protected Sphere OldSphere; // Used to tell OldContainsTokens that this thing was dropped successfully


        [Header("Movement")]
        public bool PauseAnimations;
        protected float
            dragHeight = -8f; // Draggables all drag on a specifc height and have a specific "default height"


        public TokenTravelItinerary CurrentItinerary { get; set; }

        [Header("Display")]
        [SerializeField] protected bool shrouded;
        [SerializeField] protected bool rotateOnDrag = true;
        protected float perlinRotationPoint = 0f;
        protected int startSiblingIndex;
        protected Vector3 dragOffset;
        protected CanvasGroup canvasGroup;
        [SerializeField] protected IManifestation _manifestation;

        [Header("Logic")]
        protected Situation _attachedToSituation = NullSituation.Create();
        //set true when the Chronicler notices it's been placed on the desktop. This ensures we don't keep spamming achievements / Lever requests. It isn't persisted in saves! which is probably fine.

        public bool PlacementAlreadyChronicled = false;

        public virtual IVerb Verb { get; private set; }
        public virtual ElementStack ElementStack { get; protected set; }
        public int ElementQuantity => ElementStack.Quantity;
        public Element Element => ElementStack.Element;

        public bool IsValidElementStack()
        {
            if (ElementStack == null)
                return false;
            return ElementStack.IsValidElementStack();
        }


        public UnityEvent OnStart;
        public UnityEvent OnCollect;
        public UnityEvent OnWindowClosed;
        public OnSphereAddedEvent OnSphereAdded;
        public OnSphereRemovedEvent OnSphereRemoved;


        public virtual void Awake()
        {
            //if (Sphere == null)
            //    Sphere = Registry.Get<Limbo>();

            TokenRectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            CurrentItinerary = TokenTravelItinerary.StayExactlyWhereYouAre(this);
            _manifestation = Watchman.GetOrInstantiate<NullManifestation>(TokenRectTransform); 
            ElementStack =Watchman.GetOrInstantiate<NullElementStack>(TokenRectTransform);

            SetState(new DroppedInSphereState());

        }

        public void StartArtAnimation()
        {
            if (!CanAnimateArt())
                return;
            _manifestation.BeginIconAnimation();

        }

        
        public bool CanAnimateArt()
        {
            if (gameObject == null)
                return false;

            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated


            return _manifestation.CanAnimateIcon();
        }

        public bool IsInMotion { get; set; }
        public bool Defunct { get; protected set; }
        public bool NoPush => _manifestation.NoPush;

        private TokenState CurrentState;


        public AnchorDurability Durability
        {
            get
            {
                if (Verb.Transient)
                    return AnchorDurability.Transient;
                else
                    return AnchorDurability.Enduring;
            }
        }


        public void SetVerb(IVerb verb)
        {
            Verb = verb;

   

            name = Verb.Id + "_verbtoken";
        }

        public void AttachedTo(Situation situation)
        {
            _attachedToSituation = situation;
        }


        public virtual void Populate(ElementStack elementStack)
        {
            ElementStack = elementStack;
            name = elementStack.Element.Id + "_stacktoken";
        }

        private void ReplaceManifestation(IManifestation oldManifestation, IManifestation newManifestation,
            RetirementVFX vfxForOldManifestation)
        {
            var manifestationToRetire = oldManifestation;
            _manifestation = newManifestation;

            //This makes me nervous: I still only have a nebulous understanding of anchor positioning stuff
            //but! I needed to set it because GridLayout overrides anchor positions, and there's a GridLayout in storage
            TokenRectTransform.sizeDelta = new Vector2(_manifestation.RectTransform.sizeDelta.x,
                _manifestation.RectTransform.sizeDelta.y);
            TokenRectTransform.anchorMin = _manifestation.RectTransform.anchorMax;
            TokenRectTransform.anchorMax = _manifestation.RectTransform.anchorMax;

            manifestationToRetire.Retire(vfxForOldManifestation, OnReplacedManifestationRetired);

            if (ElementStack.IsValidElementStack())
                InitialiseElementManifestation();
            else
                InitialiseVerbManifestation();

        }

        private void OnReplacedManifestationRetired()
        {
            //
        }

        public virtual void Manifest()
        {

            if (IsValidElementStack()) 
            {

                if (_manifestation.GetType() != ElementStack.GetManifestationType(Sphere.SphereCategory))
                {
                    var newManifestation = Watchman.Get<PrefabFactory>()
                        .CreateManifestationPrefab(ElementStack.GetManifestationType(Sphere.SphereCategory),
                            this.transform);
                    ReplaceManifestation(_manifestation, newManifestation, RetirementVFX.None);
                }
            }
            else if (Verb!=null) //YUK
                if (_manifestation.GetType() != Verb.GetManifestationType(Sphere.SphereCategory))
                {
                    var newManifestation = Watchman.Get<PrefabFactory>()
                        .CreateManifestationPrefab(Verb.GetManifestationType(Sphere.SphereCategory), this.transform);
                    ReplaceManifestation(_manifestation, newManifestation, RetirementVFX.None);
                    InitialiseVerbManifestation();
                }
                else
                {
                    NoonUtility.LogWarning("Token with neither a valid verb nor a valid stack: " +
                                           gameObject.name);
                }
        }

        /// <summary>
        /// replaces one manifestation with an identical manifestation - so for example we can do a vfx retiring the old one
        /// </summary>
        /// <param name="vfx"></param>
        public virtual void Remanifest(RetirementVFX vfx)
        {
            var reManifestation = Watchman.Get<PrefabFactory>()
                .CreateManifestationPrefab(_manifestation.GetType(), this.transform);

            reManifestation.Transform.position = _manifestation.Transform.position;

            // Put it behind the old card that we're about to destroy showily
            reManifestation.Transform.SetSiblingIndex(_manifestation.Transform.GetSiblingIndex() - 1);

            ReplaceManifestation(_manifestation, reManifestation, vfx);

            Manifest();
        }

        private void InitialiseVerbManifestation()
        {
            _manifestation.InitialiseVisuals(Verb);
        }

        private void InitialiseElementManifestation()
        {
            _manifestation.InitialiseVisuals(ElementStack.Element);
            _manifestation.UpdateVisuals(ElementStack.Element, ElementStack.Quantity);
            _manifestation.UpdateTimerVisuals(Element.Lifetime, ElementStack.LifetimeRemaining, 0f, Element.Resaturate,
                EndingFlavour.None);

            if(shrouded)
                _manifestation.Shroud(true);
            else
            _manifestation.Reveal(true);
        }



        public void SetState(TokenState state)
        {
            CurrentState = state;
        }



        public bool CurrentlyBeingDragged()
        {
            return  
                CurrentState.InPlayerDrivenMotion(this);
        }
    


    public void SetSphere(Sphere newSphere, Context context)
        {
            OldSphere = Sphere;
            Sphere = newSphere;

            if (OldSphere != null && OldSphere != newSphere)
            {
                OldSphere.RemoveToken(this,context);
                if (OldSphere.ContentsHidden && !newSphere.ContentsHidden)
                    _manifestation.UpdateVisuals(Element,ElementQuantity);
            }

            Sphere = newSphere;
        }

        public bool IsInContainer(Sphere compareContainer, Context context)
        {
            return compareContainer == Sphere;
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            NoonUtility.Log("Beginning drag for " + this.name,0,VerbosityLevel.SystemChatter);

            if (CanBeDragged())
                StartDrag(eventData);
            

        }
        /// <summary>
        /// can move manually
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
       public bool CanBeDragged()
        {
            return
                Sphere.AllowDrag 
                && !CurrentState.InSystemDrivenMotion(this)
                && !Defunct
                && !shrouded
                && !_manifestation.RequestingNoDrag;
        }


        /// <summary>
        /// can be grabbed by a greedy angel
        /// </summary>
        /// <returns></returns>
      public bool CanBePulled()
        {
            if (Defunct)
                return false;
            if (CurrentState.InSystemDrivenMotion(this))
                return false;

            var allowExploits = Watchman.Get<Config>().GetConfigValueAsInt(NoonConstants.BIRDWORMSLIDER);
            if (allowExploits != null || allowExploits > 0)
            {

                if (CurrentlyBeingDragged())
                {
                    NoonUtility.Log("WORM enabled: dragging defeats pulling");
                     return false; // don't pull cards being dragged if Worm is set On}
                }
            }

            return true;
        }

        protected void StartDrag(PointerEventData eventData)
        {
            //remember the original location in case the token gets evicted later
            var homingAngel = new HomingAngel(this);
            homingAngel.SetWatch(Sphere);
            Sphere.AddAngel(homingAngel);

            SetState(new BeingDraggedState());
            
            
            NotifyInteracted(new TokenInteractionEventArgs { PointerEventData = eventData, Token = this, Sphere = Sphere, Interaction = Interaction.OnDragBegin });
            if (!Keyboard.current.shiftKey.wasPressedThisFrame)
            {
                if (ElementStack.IsValidElementStack() && ElementQuantity > 1)
                  homingAngel.SetOriginToken(CalveToken(ElementQuantity - 1, new Context(Context.ActionSource.PlayerDrag)));

            }


            var enrouteContainer = Watchman.Get<SphereCatalogue>().GetSphereByPath(
                new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultEnRouteSpherePath));

            enrouteContainer.AcceptToken(this, new Context(Context.ActionSource.PlayerDrag));
            
            TokenRectTransform.SetAsLastSibling();
            _manifestation.OnBeginDragVisuals();

            canvasGroup.blocksRaycasts = false;

            startSiblingIndex = TokenRectTransform.GetSiblingIndex();



            //commented out because I *might* not need it; but if I do, we can probably calculate it on the fly.
            //if (this.EntityId=="dropzone")
            //{
            //    Vector3 pressPos;
            //    RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Get<IDraggableHolder>().RectTransform, eventData.pressPosition, eventData.pressEventCamera, out pressPos);
            //    dragOffset = (startPosition + startParent.position) - pressPos;
            //}
            //else
            //{
            dragOffset = Vector3.zero;
            //  }

            SoundManager.PlaySfx("CardPickup");

        }



        public void OnDrag(PointerEventData eventData)
        {
            if (!CurrentlyBeingDragged())
                return;

            MoveObject(eventData);


            NotifyInteracted(new TokenInteractionEventArgs {PointerEventData = eventData,Token=this,Sphere= Sphere,Interaction = Interaction.OnDrag});

        }



        public void MoveObject(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Sphere.GetRectTransform(),
                eventData.position, eventData.pressEventCamera, out var draggedToPosition);

            
            // Potentially change this so it is using UI coords and the RectTransform?
            //  rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

         TokenRectTransform.position = draggedToPosition; ///aaaahh it's *position* not anchoredposition3D because we're getting the world point from the click

            
            _manifestation.DoMove(ManifestationRectTransform);

            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f)
            {
                // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation =
                    Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }



        }


        public  void OnEndDrag(PointerEventData eventData)
        {
            NotifyInteracted(new TokenInteractionEventArgs { PointerEventData = eventData, Token = this, Sphere = Sphere,Interaction = Interaction.OnDragEnd});
            
            FinishDrag();
        }


        public  void FinishDrag()
        {
            canvasGroup.blocksRaycasts = true;
            if (!CurrentState.Docked(this))
                   this.Sphere.EvictToken(this,new Context(Context.ActionSource.Unknown));
            
        }

        public  void OnDrop(PointerEventData eventData)
        {

            var incomingToken = eventData.pointerDrag.GetComponent<Token>();
            if (incomingToken == null)
                return;

            if (CanInteractWithToken(incomingToken))
                InteractWithIncomingToken(incomingToken, eventData);
            else
            {
                this.Sphere.TryMoveAsideFor(this, incomingToken, out bool moveAsideFor);

                if (moveAsideFor)
                    SetState(new DroppedOnTokenWhichMovedAsideState());
                else
                    SetState(new RejectedBySituationState());
            }
        }

 
        private void InteractWithIncomingToken(Token incomingToken, PointerEventData eventData)
        {
            NotifyInteracted(new TokenInteractionEventArgs
            {
                Token = this,
                Sphere = Sphere,
                PointerEventData = eventData,
                Interaction = Interaction.OnReceivedADrop
            });

            if (ElementStack.IsValidElementStack() && incomingToken.ElementStack.IsValidElementStack())
            {
                if (ElementStack.CanMergeWith(incomingToken.ElementStack))
                    ElementStack.AcceptIncomingStackForMerge(incomingToken.ElementStack);
                else

                    ElementStack.ShowNoMergeMessage(incomingToken.ElementStack);
            }
            
            _attachedToSituation.InteractWithSituation(incomingToken);

        }

        private void TokenEntrance(Token incomingToken)
        {

            if (incomingToken.ElementStack.IsValidElementStack())
            {
                _attachedToSituation.TryPushDraggedStackIntoThreshold(incomingToken);


                if (!_attachedToSituation.IsOpen)
                    _attachedToSituation.OpenAtCurrentLocation();
            }
            else
            {
                //something has gone awryy
                SetState(new RejectedBySituationState());
            }
        }

        public Token CalveToken(int quantityToLeaveBehind, Context context)
        {

            if (quantityToLeaveBehind <= 0) //for some reason we're trying to leave an empty stack behind..
                return Sphere.ProvisionElementStackToken(NullElement.NULL_ELEMENT_ID, 0,new Context(Context.ActionSource.CalvedStack,new TokenLocation(this)));

            if (ElementQuantity <= quantityToLeaveBehind
            ) //we're trying to leave everything behind. Abort the drag and return the original token, ie this token
            {
                FinishDrag();
                return this;
            }


            var calvedToken =
                Sphere.ProvisionElementStackToken(Element.Id, ElementQuantity - 1, new Context(Context.ActionSource.CalvedStack, new TokenLocation(this)), ElementStack.GetCurrentMutations());


            ElementStack.SetQuantity(ElementQuantity - quantityToLeaveBehind, context);

            // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
            calvedToken.transform.position = transform.position;

            // Accepting stack may put it to pos Vector3.zero, so this is last
            calvedToken.transform.position = transform.position;
            return calvedToken;

        }

        public void OnPointerClick(PointerEventData eventData)
        {

            if (_manifestation.HandlePointerDown(eventData, this))
                return;

            //Manifestation didn't handle click
            Watchman.Get<DebugTools>().SetInput(_attachedToSituation.RecipeId);

            if (!_attachedToSituation.IsOpen)
                _attachedToSituation.OpenAtCurrentLocation();
            else
                _attachedToSituation.Close();

            float timeSincePreviousClick = eventData.clickTime - previousClickTime;

            float doubleClickInterval = 0.5f;

            if (timeSincePreviousClick<doubleClickInterval)
            {
                previousClickTime = 0f;
                NotifyInteracted(new TokenInteractionEventArgs
                {
                    Element = ElementStack.Element,
                    Token = this,
                    Sphere = Sphere,
                    PointerEventData = eventData,
                    Interaction = Interaction.OnDoubleClicked
                });

            }
            else
            {
                if (shrouded)
                {
                    Unshroud(false);
                }
                else
                {
                    NotifyInteracted(new TokenInteractionEventArgs
                    {
                        Element = ElementStack.Element,
                        Token = this,
                        Sphere = Sphere,
                        PointerEventData = eventData,
                        Interaction = Interaction.OnClicked
                    });
                }

                // this moves the clicked sibling on top of any other nearby cards.
                if (Sphere.GetType() != typeof(ThresholdSphere) && Sphere.GetType() != typeof(ExhibitCards))
                    transform.SetAsLastSibling();

                previousClickTime = eventData.clickTime;
            }

        }

        public void GoAway(Context context)
        {
            Sphere.EvictToken(this,context);
        }


        public void DisplayOverrideIcon(string icon)
        {
            _manifestation.OverrideIcon(icon);
        }

        protected virtual void NotifyInteracted(TokenInteractionEventArgs args)
        {
            Sphere.OnTokenInThisSphereInteracted(args);
            Watchman.Get<Chronicler>()?.TokenPlacedOnTabletop(this);
        }

        public bool Retire()
        {
            return Retire(RetirementVFX.None);
        }

        public virtual bool Retire(RetirementVFX vfx)
        {
            if (Defunct)
                return false;
            Defunct = true;
            FinishDrag(); // Make sure we have the drag aborted in case we're retiring mid-drag (merging stack frex)

            _manifestation.Retire(vfx, OnManifestationRetired);
            ElementStack.Retire(vfx);
            var args=new SphereContentsChangedEventArgs(Sphere, new Context(Context.ActionSource.Retire));
            args.TokenRemoved = this;
            Sphere.NotifyTokensChangedForSphere(args);

            SetSphere(Watchman.Get<Limbo>(), new Context(Context.ActionSource.Retire));

            return true;
        }

        private void OnManifestationRetired()
        {

            Destroy(this.gameObject);
        }



        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _manifestation.Highlight(HighlightType.Hover);

            var tabletopManager = Watchman.Get<TabletopManager>();
            if (tabletopManager != null
            ) //eg we might have a face down card on the credits page - in the longer term, of course, this should get interfaced
            {
                if (!shrouded && ElementStack.IsValidElementStack())
                    tabletopManager.SetHighlightedElement(Element.Id, ElementQuantity);
                else
                    tabletopManager.SetHighlightedElement(null);
            }


        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _manifestation.Unhighlight(HighlightType.Hover);

            NotifyInteracted(new TokenInteractionEventArgs
            {
                Element = Element,
                Token = this,
                Sphere = Sphere,
                PointerEventData = eventData,
                Interaction = Interaction.OnPointerExited
            });


            var ttm = Watchman.Get<TabletopManager>();
            if (ttm != null)
            {
                Watchman.Get<TabletopManager>().SetHighlightedElement(null);
            }

        }

        public void BurnImageUnderToken(string burnImage)
        {
            Watchman.Get<INotifier>()
                .ShowImageBurn(burnImage, this, 20f, 2f,
                    TabletopImageBurner.ImageLayoutConfig.CenterOnToken);
        }

        public void TravelTo(TokenTravelItinerary itinerary,Context context)
        {
            CurrentItinerary = itinerary;
          itinerary.Depart(this,context);
        }

        public virtual void SituationStateChanged(Situation situation)
        {
            _manifestation.DisplaySpheres(situation.GetSpheresActiveForCurrentState());
        }

        public void TimerValuesChanged(Situation situation)
        {
            _manifestation.UpdateTimerVisuals(situation.Warmup, situation.TimeRemaining,
                situation.IntervalForLastHeartbeat, false, situation.CurrentPrimaryRecipe.SignalEndingFlavour);

        }

        public virtual void onElementStackQuantityChanged(ElementStack stack,Context context)
        {

            _manifestation.UpdateVisuals(stack.Element,stack.Quantity);
            _manifestation.UpdateTimerVisuals(stack.Element.Lifetime,stack.LifetimeRemaining,stack.IntervalForLastHeartbeat,Element.Resaturate,EndingFlavour.None);
            PlacementAlreadyChronicled = false; //should really only do this if the element has changed
            var args=new SphereContentsChangedEventArgs(Sphere,context);
            Sphere.NotifyTokensChangedForSphere(args);
            
        }

    public void SituationSphereContentsUpdated(Situation situation)
        {
            _manifestation.DisplaySpheres(situation.GetSpheresActiveForCurrentState());
        }

        public void ReceiveNotification(INotification n)
        {
           NoonUtility.Log("ReceiveNotification on Token: use it or lose it");
        }

        public void Understate()
        {
            _manifestation.Understate();
        }

        public void Emphasise()
        {
            _manifestation.Emphasise();
        }


        public void Unshroud(bool instant = false)
        {
            shrouded = false;
            _manifestation.Reveal(instant);
        }

        public void Shroud(bool instant = false)
        {
            shrouded = true;
            _manifestation.Shroud(instant);

        }

        public bool Shrouded()
        {
            return shrouded;
        }


        public bool CanInteractWithToken(Token incomingToken)
        {
            if (Defunct)
                return false;
            //can we merge tokens?
            if (ElementStack.CanMergeWith(incomingToken.ElementStack))
                return true;

            //can we put a stack in a threshold associated with this token?
            if (_attachedToSituation.GetAvailableThresholdsForStackPush(incomingToken.ElementStack).Count>0)
             return true;

            return false;
        }

        public void ShowPossibleInteractionWithToken(Token token)
        {
            if (Defunct)
                return;

            _manifestation.Highlight(HighlightType.CanInteractWithOtherToken);

        }

        public void StopShowingPossibleInteractionWithToken(Token token)
        {
            _manifestation.Unhighlight(HighlightType.CanInteractWithOtherToken);

        }

    }
}