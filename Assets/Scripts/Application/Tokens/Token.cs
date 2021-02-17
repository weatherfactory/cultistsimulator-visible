#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Entities.NullEntities;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.States;
using SecretHistories.Constants;
using SecretHistories.States.TokenStates;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Constants.Events;
using SecretHistories.Core;
using SecretHistories.Spheres.Angels;
using SecretHistories.Spheres;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace SecretHistories.UI {

    [IsEncaustableClass(typeof(TokenCreationCommand))]
    [RequireComponent(typeof(RectTransform))]
    public class Token : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler,
        IPointerExitHandler, IInteractsWithTokens,IEncaustable
    {
        private float previousClickTime = 0f;



        [Header("Location")]
        [SerializeField] public RectTransform TokenRectTransform;
        [DontEncaust]
        public RectTransform ManifestationRectTransform => _manifestation.RectTransform;
        [Encaust]
        public TokenLocation Location {
            get
            {
                if (TokenRectTransform !=null && !TokenRectTransform.Equals(null))
                    return new TokenLocation(TokenRectTransform.anchoredPosition3D, Sphere.GetPath());
                else
                    return TokenLocation.Default();
            }
        }
        [DontEncaust]
        public Sphere Sphere { get; set; }
        [DontEncaust]
        protected Sphere OldSphere  { get; set; }// Used to tell OldContainsTokens that this thing was dropped successfully


        [Header("Movement")]
        public bool PauseAnimations;
        protected float
            dragHeight = -8f; // Draggables all drag on a specific height and have a specific "default height"

        [Encaust]
        public TokenTravelItinerary CurrentItinerary { get; set; }

        [Header("Display")]
        [SerializeField] protected bool shrouded;
        [SerializeField] protected bool rotateOnDrag = true;
        protected float perlinRotationPoint = 0f;
        protected int startSiblingIndex;
        protected Vector3 dragOffset;
        protected CanvasGroup canvasGroup;
        [SerializeField] protected IManifestation _manifestation;


        //set true when the Chronicler notices it's been placed on the desktop. This ensures we don't keep spamming achievements / Lever requests. It isn't persisted in saves! which is probably fine.
        [Encaust]
        public bool Defunct { get; protected set; }
        [DontEncaust]
        public bool NoPush => _manifestation.NoPush;


        public bool PlacementAlreadyChronicled = false;

        private ITokenPayload _payload;

        [Encaust]
        public virtual ITokenPayload Payload
        {
            get
            {
                if (_payload!= null)
                    return _payload;

                else
                {
                    NoonUtility.LogWarning($"Unknown payload type in token {gameObject.name}: setting to Null Payload");
                    _payload=new NullElementStack();
                    return _payload;
                }
            }
        }

        [DontEncaust]
        public int Quantity => Payload.Quantity;

        public bool IsValidElementStack()
        {
            return Payload.IsValidElementStack();
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
            _payload = new NullElementStack();

            SetState(new DroppedInSphereState());

        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
        if(!command.ExecuteOn(this))
                Payload.ExecuteTokenEffectCommand(command);
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

        

        private TokenState CurrentState;


        public void SetPayload(ITokenPayload payload)
        {
            _payload = payload;
            _payload.OnChanged += OnPayloadChanged;
            name = _payload.Id + "_token";
        }


        public AspectsDictionary GetAspects(bool includeSelf = true)
        {
            return Payload.GetAspects(includeSelf);
        }

        public Dictionary<string, int> GetCurrentMutations()
        {
            return Payload.Mutations;
        }

        public void ExecuteHeartbeat(float interval)
        {
        Payload.ExecuteHeartbeat(interval);
     
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

            Payload.InitialiseManifestation(_manifestation);

            if (shrouded)
                _manifestation.Shroud(true);
            else
                _manifestation.Reveal(true);

        }

        private void OnReplacedManifestationRetired()
        {
            //
        }

        public virtual void Manifest()
        {
            //I believe this only happens in automated test scenarios. but it's a bear sorting out the lifecycle!
            if (_manifestation == null)
                _manifestation = Watchman.GetOrInstantiate<NullManifestation>(TokenRectTransform);

            if (_manifestation.GetType() != Payload.GetManifestationType(Sphere.SphereCategory))
            {
                Type newManifestationType = Payload.GetManifestationType(Sphere.SphereCategory);

                var newManifestation = Watchman.Get<PrefabFactory>().CreateManifestationPrefab(newManifestationType, this.transform);

                ReplaceManifestation(_manifestation, newManifestation, RetirementVFX.None);
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
                    _manifestation.UpdateVisuals(Payload);
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
            
            
            NotifyInteracted(new TokenInteractionEventArgs { PointerEventData = eventData, Payload = Payload, Token = this, Sphere = Sphere, Interaction = Interaction.OnDragBegin });
            if (!Keyboard.current.shiftKey.wasPressedThisFrame)
            {
                if (Payload.IsValidElementStack() && Quantity > 1)
                  homingAngel.SetOriginToken(CalveToken(Quantity - 1, new Context(Context.ActionSource.PlayerDrag)));

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


            NotifyInteracted(new TokenInteractionEventArgs {PointerEventData = eventData, Payload = Payload, Token =this,Sphere= Sphere,Interaction = Interaction.OnDrag});

        }



        public void MoveObject(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Sphere.GetRectTransform(),
                eventData.position, eventData.pressEventCamera, out var draggedToPosition);

            
            // Potentially change this so it is using UI coords and the RectTransform?
            //  rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

         TokenRectTransform.position = draggedToPosition; ///aaaahh it's *position* not anchoredposition3D because we're getting the world point from the click

         Payload.OnTokenMoved(Location);  


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
            NotifyInteracted(new TokenInteractionEventArgs { PointerEventData = eventData, Payload = Payload, Token = this, Sphere = Sphere,Interaction = Interaction.OnDragEnd});
            
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
                Payload = Payload,
                Sphere = Sphere,
                PointerEventData = eventData,
                Interaction = Interaction.OnReceivedADrop
            });

            if (Payload.CanInteractWith(incomingToken.Payload))
                Payload.InteractWithIncoming(incomingToken);
            else

                Payload.ShowNoMergeMessage(incomingToken.Payload);

        }


        public Token CalveToken(int quantityToLeaveBehind, Context context)
        {

            if (quantityToLeaveBehind <= 0) //for some reason we're trying to leave an empty stack behind..
                return Sphere.ProvisionElementStackToken(NullElement.NULL_ELEMENT_ID, 0,new Context(Context.ActionSource.CalvedStack,new TokenLocation(this)));

            if (Quantity <= quantityToLeaveBehind
            ) //we're trying to leave everything behind. Abort the drag and return the original token, ie this token
            {
                FinishDrag();
                return this;
            }

            var calvedTokenCreationCommand = new ElementStackCreationCommand(Payload.Id, Quantity - 1)
            {
                Mutations = Payload.Mutations
            };
            var calvingContext = new Context(Context.ActionSource.CalvedStack);
            calvingContext.TokenDestination = new TokenLocation(this);

            var calvedToken=Sphere.ProvisionElementStackToken(calvedTokenCreationCommand,calvingContext);
            
            
            Payload.SetQuantity(Quantity - quantityToLeaveBehind, context);


            //I think the commented code below is redundant now, because we specify the position in the TokenLocation passed in the context? <--which ultimately should go into a comand, yah
            // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
            //calvedToken.transform.position = transform.position;
            // Accepting stack may put it to pos Vector3.zero, so this is last
            //calvedToken.transform.position = transform.position;
            return calvedToken;

        }

        public void OnPointerClick(PointerEventData eventData)
        {

            if (_manifestation.HandlePointerDown(eventData, this))
                return;

            if (!Payload.IsOpen)
                Payload.OpenAt(Location);
            else
                Payload.Close();

            float timeSincePreviousClick = eventData.clickTime - previousClickTime;

            float doubleClickInterval = 0.5f;

            if (timeSincePreviousClick<doubleClickInterval)
            {
                previousClickTime = 0f;
                NotifyInteracted(new TokenInteractionEventArgs
                {
                    Payload = Payload,
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
                        Payload = Payload,
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
            _payload.OnChanged -= OnPayloadChanged;
            FinishDrag(); // Make sure we have the drag aborted in case we're retiring mid-drag (merging stack frex)

            _manifestation.Retire(vfx, OnManifestationRetired);
            _payload.Retire(vfx);
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
                if (!shrouded && Payload.IsValidElementStack())
                    tabletopManager.SetHighlightedElement(Payload.Id, Quantity);
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
               Payload = Payload,
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

        public void TravelTo(TokenTravelItinerary itinerary,Context context)
        {
            CurrentItinerary = itinerary;
          itinerary.Depart(this,context);
        }

        private void OnPayloadChanged(TokenPayloadChangedArgs args)
        {
            if (args.ChangeType == PayloadChangeType.Fundamental)
                Remanifest(RetirementVFX.CardTransformWhite);
            else if (args.ChangeType == PayloadChangeType.Update)
            {
                _manifestation.UpdateVisuals(_payload);
                PlacementAlreadyChronicled = false; //should really only do this if the element has changed
                var sphereContentsChangedArgs = new SphereContentsChangedEventArgs(Sphere, args.Context);
                Sphere.NotifyTokensChangedForSphere(sphereContentsChangedArgs);
            }
            else if (args.ChangeType == PayloadChangeType.Retirement)
                Retire(args.VFX);

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
            if(CanMergeWithToken(incomingToken))
                return true;

            if(Payload.CanInteractWith(incomingToken.Payload))

                 return true;

            return false;
        }

        public bool CanMergeWithToken(Token incomingToken)
        {
            if (!Sphere.AllowStackMerge)
                return false;

            return (Payload.CanMergeWith(incomingToken.Payload));
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

        public void Purge()
        {
            if (Payload.GetTimeshadow().Transient)
                Payload.ExecuteHeartbeat(Payload.GetTimeshadow().LifetimeRemaining + 1); //make it decay to its next form
            else
                Payload.Retire(RetirementVFX.CardLight);
        }
    }
}