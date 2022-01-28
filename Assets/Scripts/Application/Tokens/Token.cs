#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Entities.NullEntities;
using Assets.Scripts.Application.Fucine;
using Assets.Scripts.Application.Infrastructure.Events;
using Newtonsoft.Json.Bson;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
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
using SecretHistories.Constants.Events;
using SecretHistories.Core;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Spheres.Angels;
using SecretHistories.Spheres;
using Pathfinding;
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



        [DontEncaust]
        public RectTransform TokenRectTransform => GetComponent<RectTransform>();

        [DontEncaust]
        public RectTransform ManifestationRectTransform => _manifestation.RectTransform;
        [Encaust]
        public TokenLocation Location {
            get
            {
                FucinePath locationPath;
                if (Sphere == null)
                    locationPath = new NullFucinePath();
                else
                    locationPath = Sphere.GetAbsolutePath();
                if (TokenRectTransform !=null && !TokenRectTransform.Equals(null))
                    return new TokenLocation(TokenRectTransform.anchoredPosition3D, locationPath);
                else
                    return TokenLocation.Default(locationPath);
            }
        }
        /// <summary>
        /// This gives the anchorPosition3D run through Transform.TransformPoint()
        /// </summary>
        /// <returns></returns>
        public Vector3 AnchoredPosInWorld()
        {

            if (TokenRectTransform == null)
                return Vector3.zero;
            if (TokenRectTransform == null)
                return Vector3.zero;

            Vector3 worldPos = TokenRectTransform.parent.TransformPoint(TokenRectTransform.anchoredPosition3D);

            return worldPos;
        }

        [Encaust]
        public AbstractTokenState CurrentState
        {
            get => _currentState;
            set => _currentState = value;
        }


        [DontEncaust]
        public virtual Sphere Sphere { get; set; }

        [Header("Movement")]
        public bool PauseAnimations;
        protected float
            dragHeight = -8f; // Draggables all drag on a specific height and have a specific "default height"

        [DontEncaust] //maybe phase this out now it exists in Xamanek.
        public TokenItinerary CurrentItinerary { get; set; }

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


        [Encaust]
        public ITokenPayload Payload
        {
            get
            {
                if (_payload!= null)
                    return _payload;

                else
                {
                    _payload=new MinimalPayload("");
                    return _payload;
                }
            }
        }

        [DontEncaust]
        public int Quantity => Payload.Quantity;

        [DontEncaust]
        public string PayloadId
        {
            get
            {
                if (Payload == null)
                    return string.Empty;
                return Payload.Id;
            }
        }

        [DontEncaust] public string PayloadEntityId
        {
            get
            {
                if (Payload == null)
                    return string.Empty;
                return Payload.EntityId;
            }
        }

        [DontEncaust] public string PayloadTypeName => _payload.GetType().Name;

        [SerializeField]
        private Vector3 WorldPosition;

        [SerializeField]
        private string FullPathAsString;

  


        public bool IsValid()
        {
            return Payload.IsValid();
        }

        public bool IsValidElementStack()
        {
            return Payload.IsValidElementStack();
        }


        public bool PlacementAlreadyChronicled = false;

        private ITokenPayload _payload;

        private HomingAngel _homingAngel;

        public Sphere GetHomeSphere()
        {
            if (_homingAngel == null)
                return Watchman.Get<HornedAxe>().GetDefaultSphere();
            if(_homingAngel.GetWatchedSphere()==null)
                return Watchman.Get<HornedAxe>().GetDefaultSphere();

            return _homingAngel.GetWatchedSphere();

        }

        public UnityEvent OnWindowClosed;
        public OnSphereAddedEvent OnSphereAdded;
        public OnSphereRemovedEvent OnSphereRemoved;


        public virtual void Awake()
        {
            
            canvasGroup = GetComponent<CanvasGroup>();

            _manifestation = Watchman.GetOrInstantiate<NullManifestation>(TokenRectTransform);
            _payload = NullElementStack.Create();

           Stabilise();

        }

        //Sets the stable version of the token's current state. At time of writing, this is always 'DroppedInSphere'
        //It's called by Dropcatchers and also explicitly by TabletopSphere - but not explicitly by ThresholdSpheres.
        //This probably makes sense, because things return to TabletopSphere rather than being explicitly dropped, but it may not make sense in 
        //BH, where we effectively have multiple tabletops.
        public void Stabilise()
        {
            CurrentState = CurrentState.GetDefaultStableState();
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

        public Rect GetRectInWorldSpace()
        {
            var rect = TokenRectTransform.rect;

            var rectInWorldSpace = new Rect((Vector2)AnchoredPosInWorld() - rect.size / 2f, rect.size); //This assumes a centre pivot. We can address that here if nec.
            return rectInWorldSpace;
        }
        /// <summary>
        /// Helper function for rect to compare with rects in another sphere
        /// </summary>
        /// <param name="otherSphere">The other sphere is expected to be at the same z position</param>
        /// <returns></returns>
        public Rect GetRectInOtherSphere(Sphere otherSphere)
        {
            var rect = TokenRectTransform.rect;

            var tokenWorldPosition = Sphere.GetRectTransform().TransformPoint(Location.Anchored3DPosition);
            var projectionPosition = otherSphere.GetRectTransform().InverseTransformPoint(tokenWorldPosition);
            var rectInOtherSphere = new Rect((Vector2)projectionPosition - rect.size / 2f, rect.size); //This assumes a centre pivot. We can address that here if nec.
            return rectInOtherSphere;
        }
        public Rect GetRectInCurrentSphere()
        {
            var rect = TokenRectTransform.rect; //this is in the rect in the token's own transform space
            var rectInSphere=new Rect(TokenRectTransform.anchoredPosition-rect.size/2f,rect.size); //This assumes a centre pivot. We can address that here if nec.
            return rectInSphere;
        }

        public Rect GetRectFromPosition(Vector2 assumingPosition)
        {
            var rect = TokenRectTransform.rect; //this is in the rect in the token's own transform space
            var rectAssumingPosition = new Rect(assumingPosition - rect.size / 2f, rect.size); //This assumes a centre pivot. We can address that here if nec.
            return rectAssumingPosition;
        }

        public void SetPayload(ITokenPayload payload)
        {
            _payload = payload;
            _payload.OnChanged += OnPayloadChanged;
            _payload.SetToken(this);
            name = _payload.Id + "_token";
  
            Manifest(); //just changed this from Remanifest. Surely if we've set the payload, there shouldn't be a manifestation?
            //so we don't need this overhead and leftover objects? but check again if this breaks CS

            //Remanifest(RetirementVFX.None); //Originally I said: 'Remanifest not manifest. If we've just set a new payload, the manifestation type is very likely already the correct type for that sphere.'
        }

        public void SetPayloadWithExistingManifestation(ITokenPayload payload,IManifestation alreadyExtantManifestation)
        {
            _manifestation = alreadyExtantManifestation;
            //doing this in one method gets us round awkward initialisation order problems
            SetPayload(payload);
            
        }


        public AspectsDictionary GetAspects(bool includeSelf = true)
        {
            return Payload.GetAspects(includeSelf);
        }

        public Dictionary<string, int> GetCurrentMutations()
        {
            return Payload.Mutations;
        }

        public void ExecuteHeartbeat(float seconds, float metaseconds)
        {
            //call to TokenTravelAnimation used to occur here, until I realised that animations go better in frame update!
            Payload.ExecuteHeartbeat(seconds, metaseconds);
     
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
                _manifestation.Unshroud(true);

            if(_ghost!=null && !_ghost.Equals(null))
                _ghost.Retire();

            _ghost = _manifestation.CreateGhost();
            HideGhost();


        }

        private void OnReplacedManifestationRetired()
        {
            //
        }

        private IGhost _ghost;
        private AbstractTokenState _currentState = new UnknownState();

        public IGhost GetCurrentGhost()
        {
            if (_ghost == null)
                return NullGhost.Create(_manifestation);

            return _ghost;
        }
        public bool DisplayGhostAtChoreographerDrivenPosition(Sphere projectInSphere)
        {
            if (_ghost == null)
                return false;


            var tokenWorldPosition = Sphere.GetRectTransform().TransformPoint(Location.Anchored3DPosition);
            var projectionPosition = projectInSphere.GetRectTransform().InverseTransformPoint(tokenWorldPosition);
            
            var candidatePosition = projectInSphere.Choreographer.GetFreeLocalPosition(this, projectionPosition);

            _ghost.ShowAt(projectInSphere, candidatePosition,TokenRectTransform);


            //if we're showing a ghost, then we shouldn't show a ready-to-interact glow.
            _manifestation.Unhighlight(HighlightType.WillInteract, _payload);

            return true;
        }

        public bool DisplayGhostAtWorldPosition(Vector3 atPosition, Sphere projectInSphere)
        {
            if (_ghost == null)
                return false;

            
            _ghost.ShowAt(projectInSphere, atPosition,TokenRectTransform);


            //if we're showing a ghost, then we shouldn't show a ready-to-interact glow.
            _manifestation.Unhighlight(HighlightType.WillInteract, _payload);

            return true;
        }

        public void HideGhost()
        {
            if(_ghost!=null)
                _ghost.HideIn(this);
        }

        /// <summary>
        /// Apply an itinerary that will move the token from its current position to its ghost's position.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool TryFulfilGhostPromise(Context context)
        {
            
            return   _ghost.TryFulfilPromise(this,context);
        }

 
        public virtual void Manifest()
        {
            //I believe this only happens in automated test scenarios. but it's a bear sorting out the lifecycle!
            if (_manifestation == null)
                _manifestation = Watchman.GetOrInstantiate<NullManifestation>(TokenRectTransform);

            if (Sphere != null) //OKAY JUST THIS ONCE WE'RE DOING A NULL TEST. It's a headache trying to get a null sphere into the mix.
            {

                if (_manifestation.GetType() != Payload.GetManifestationType(Sphere.SphereCategory))
                {
                    Type newManifestationType = Payload.GetManifestationType(Sphere.SphereCategory);

                    var newManifestation = Watchman.Get<PrefabFactory>()
                        .CreateManifestationPrefab(newManifestationType, this.transform);

                    ReplaceManifestation(_manifestation, newManifestation, RetirementVFX.None);
                }

            }


        }


        /// <summary>
        /// replaces one manifestation with an identical manifestation - so for example we can do a vfx retiring the old one
        /// </summary>
        /// <param name="vfx"></param>
        public virtual void Remanifest(RetirementVFX vfx)
        {
            //I believe this only happens in automated test scenarios. but it's a bear sorting out the lifecycle!
            if (_manifestation == null)
                _manifestation = Watchman.GetOrInstantiate<NullManifestation>(TokenRectTransform);

            var reManifestation = Watchman.Get<PrefabFactory>()
                .CreateManifestationPrefab(_manifestation.GetType(), this.transform);

            reManifestation.Transform.position = _manifestation.Transform.position;

            // Put it behind the old card that we're about to destroy showily
            reManifestation.Transform.SetSiblingIndex(_manifestation.Transform.GetSiblingIndex() - 1);

            ReplaceManifestation(_manifestation, reManifestation, vfx);

            Manifest();
        }


        
        public bool CurrentlyBeingDragged()
        {
            return  
                CurrentState.InPlayerDrivenMotion();
        }
    


    public void SetSphere(Sphere newSphere, Context context)
        {
            var oldSphere = Sphere;
            Sphere = newSphere;

            if (oldSphere != null && oldSphere != newSphere)
            {
                oldSphere.RemoveToken(this,context);
                if (oldSphere.ContentsHidden && !newSphere.ContentsHidden)
                    _manifestation.UpdateVisuals(Payload);
            }
            FullPathAsString = new FucinePath(newSphere.GetAbsolutePath() + PayloadId).ToString();
        }

    //also sets scale for Manifestation
    public void SetLocalScale(Vector3 newScale)
    {
        TokenRectTransform.localScale = newScale;
        _manifestation.UpdateLocalScale(newScale);
    }

    public void OnBeginDrag(PointerEventData eventData)
        {

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
                && !CurrentState.InSystemDrivenMotion()
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
            if (CurrentState.InSystemDrivenMotion())
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

        public bool RequestingNoDirectDrop()
        {
            return _manifestation.RequestingNoDirectDrop;
        }

        protected void StartDrag(PointerEventData eventData)
        {
            //remember the original location in case the token gets evicted later
             _homingAngel = new HomingAngel(this);
            _homingAngel.SetWatch(Sphere);
            Sphere.AddAngel(_homingAngel);

            CurrentState=new BeingDraggedState();
           
            
            NotifyInteracted(new TokenInteractionEventArgs { PointerEventData = eventData, Payload = Payload, Token = this, Sphere = Sphere, Interaction = Interaction.OnDragBegin });
            //just picked the token up, but it hasn't yet left the origin sphere. 
            TryCalveOriginToken(_homingAngel);


            var enrouteSphere = Payload.GetEnRouteSphere();

            enrouteSphere.AcceptToken(this, new Context(Context.ActionSource.PlayerDrag));
            
            TokenRectTransform.SetAsLastSibling();
            _manifestation.OnBeginDragVisuals();

            MakeNonInteractable();

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

        private void TryCalveOriginToken(HomingAngel homingAngel)
        {
            if (_manifestation.RequestingNoSplit)
                return;
            if (Keyboard.current.shiftKey.isPressed)
                return;

            if (Quantity <= 1)
                return;
            var stackLeftBehind = CalveToken(Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
            homingAngel.SetOriginToken(stackLeftBehind);
           
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
                // This needs some tweaking so that it feels more responsive, physical. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation =
                    Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }

        }


        public  void OnEndDrag(PointerEventData eventData)
        {
            //This is called after OnDrop. So if the token has been dropped on something else, it will already have 
            //been accepted by the new sphere and potentially stabilised.
            NotifyInteracted(new TokenInteractionEventArgs { PointerEventData = eventData, Payload = Payload, Token = this, Sphere = Sphere,Interaction = Interaction.OnDragEnd});
            
            FinishDrag();
        }


        public void FinishDrag()
        {
            MakeInteractable();

            if (!CurrentState.Docked() && !CurrentState.InSystemDrivenMotion())
                //evict the token before hiding the ghost. If the ghost is still active, it'll give the evicted token a place to go.
                this.Sphere.EvictToken(this,new Context(Context.ActionSource.PlayerDrag));

            //Commented this out: we're now hiding a ghost when a token travel itinerary completes instead.
            //This is because FinishDrag() is also used for path itineraries.
            //If the change causes problems, we can fork logic here instead?
            // HideGhost(); 

        }

        public void MakeInteractable()
        {
            
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = true;
        }

        public void MakeNonInteractable()
        {
        if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
        }

        

        public void MakeVisible()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 1f; //This is a bit of a blunt instrument; what if we're fading? Manifestations don't know about this kind of thing as standard,
            //but maybe this behaviour should be moved to there and they should have CanvasGroupFaders if necessary. OTOH, this seems to be the same kind of behaviour as MakeInteractable, which lives in Token

            MakeInteractable();
        }

        public void MakeInvisible()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            MakeNonInteractable();
        }

        public  void OnDrop(PointerEventData eventData)
        {
            //Inevitable but endlessly confusing. OnDrop is 'something has been dropped on me' and not 'I have been dropped on something'
            //So the incomingtoken / potentialusurper is what is currently being dragged
            //and the token on which this method has been called is the token that is currently in situ
            var incomingToken = eventData.pointerDrag.GetComponent<Token>();
            if (incomingToken == null)
                return;

            if (incomingToken.CurrentState.Docked()) //OnDrop can be called by the event system even if the token has rejected a drag. Filter out false alarms like this.
                return;

            if (CanInteractWithToken(incomingToken))
                InteractWithIncomingToken(incomingToken, eventData);
            else
            {
                this.Sphere.TryMoveAsideFor(incomingToken,this, out bool moveAsideFor);

                if (moveAsideFor)
                    incomingToken.CurrentState =new DroppedOnTokenWhichMovedAsideState();
                else
                    incomingToken.CurrentState = new RejectedByTokenState();
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

            _manifestation.Unhighlight(HighlightType.All, _payload); //whatever's just happened, we don't want to keep predicting interactions

            if (Payload.CanInteractWith(incomingToken.Payload))
                Payload.InteractWithIncoming(incomingToken);
            else
                Payload.ShowNoMergeMessage(incomingToken.Payload);

        }


        public Token CalveToken(int quantityToLeaveBehind, Context context)
        {

            if (quantityToLeaveBehind <= 0) //for some reason we're trying to leave an empty stack behind..
            {
                var elementStackCreationCommand=new ElementStackCreationCommand(NullElement.NULL_ELEMENT_ID, 0);
                var emptyTokenCommand=new TokenCreationCommand(elementStackCreationCommand,new TokenLocation(this));
                return emptyTokenCommand.Execute(new Context(Context.ActionSource.CalvedStack, new TokenLocation(this)), this.Sphere);
            }

            if (Quantity <= quantityToLeaveBehind
            ) //we're trying to leave everything behind. Abort the drag and return the original token, ie this token
            {
                FinishDrag();
                return this;
            }

            var calvedStackCreationCommand = new ElementStackCreationCommand(Payload.EntityId, quantityToLeaveBehind)
            {
                Mutations = Payload.Mutations,
                LifetimeRemaining = Payload.GetTimeshadow().LifetimeRemaining
            };
            var calvingContext = new Context(Context.ActionSource.CalvedStack);
            calvingContext.TokenDestination = new TokenLocation(this);

            var calvedTokenCommand = new TokenCreationCommand(calvedStackCreationCommand,new TokenLocation(this));
            calvedTokenCommand.CurrentState=new PlacedAssertivelyBySystemState(); //we don't want this calved token to argue with the token that's leaving it behind, and right now they occupy the same space.
            var calvedToken = calvedTokenCommand.Execute(calvingContext, Sphere);
            Payload.SetQuantity(Quantity - quantityToLeaveBehind, context);


            //I think the commented code below is redundant now, because we specify the position in the TokenLocation passed in the context? <--which ultimately should go into a command, yah
            // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
            //calvedToken.transform.position = transform.position;
            // Accepting stack may put it to pos Vector3.zero, so this is last
            //calvedToken.transform.position = transform.position;
            return calvedToken;

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            bool handled = _manifestation.HandlePointerClick(eventData, this);

            if (handled)
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
                if (Sphere.GetType() != typeof(ThresholdSphere) && Sphere.GetType() != typeof(ExhibitCardsSphere))
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
            Sphere.NotifyTokenInThisSphereInteracted(args);
            Watchman.Get<IChronicler>()?.TokenPlacedInWorld(this);
        }

        public bool Retire()
        {
            MakeNonInteractable();
            return Retire(RetirementVFX.None);
        }

        public virtual bool Retire(RetirementVFX vfx)
        {
            if (Defunct)
                return false;
            Defunct = true;
            _payload.OnChanged -= OnPayloadChanged;
          //  FinishDrag(); // Make sure we have the drag aborted in case we're retiring mid-drag (merging stack frex) <-- finishdrag fires other behaviour we might not want. Check next if we can still merge OK

            _manifestation.Retire(vfx, OnCurrentManifestationRetired);
            _payload.Retire(vfx);
            var args=new SphereContentsChangedEventArgs(Sphere, new Context(Context.ActionSource.Retire));
            args.TokenRemoved = this;
            Sphere.NotifyTokensChangedForSphere(args);

            SetSphere(Watchman.Get<Limbo>(), new Context(Context.ActionSource.Retire));

            return true;
        }

        private void OnCurrentManifestationRetired()
        {
            if(Application.isPlaying)  //destroy doesn't work in edit mode / will destroy things permanently
                Destroy(this.gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _manifestation.Highlight(HighlightType.Hover, _payload);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _manifestation.Unhighlight(HighlightType.Hover, _payload);

            NotifyInteracted(new TokenInteractionEventArgs
            {
               Payload = Payload,
                Token = this,
                Sphere = Sphere,
                PointerEventData = eventData,
                Interaction = Interaction.OnPointerExited
            });


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
                sphereContentsChangedArgs.TokenChanged = this;
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
            var sphereContentsChangedArgs = new SphereContentsChangedEventArgs(Sphere, new Context(Context.ActionSource.Unshroud));
            sphereContentsChangedArgs.TokenChanged = this;
            Sphere.NotifyTokensChangedForSphere(sphereContentsChangedArgs);
            _manifestation.Unshroud(instant);
        }

        public void Shroud(bool instant = false)
        {
            shrouded = true;
            var sphereContentsChangedArgs = new SphereContentsChangedEventArgs(Sphere, new Context(Context.ActionSource.Shroud));
            sphereContentsChangedArgs.TokenChanged = this;
            Sphere.NotifyTokensChangedForSphere(sphereContentsChangedArgs);
            _manifestation.Shroud(instant);

        }

        public bool Shrouded()
        {
            return shrouded;
        }

        public void ShowReadyToInteract()
        {
            HideGhost();
            _manifestation.Highlight(HighlightType.WillInteract, _payload);
        }
   

        public bool TryShowPredictedInteractionIfDropped(Token incomingToken)
        {
            if(CanInteractWithToken(incomingToken))
            {
                incomingToken.ShowReadyToInteract();
                _manifestation.Highlight(HighlightType.WillInteract, _payload);
                return true;
            }

            return false;
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

        public void ShowPossibleInteraction()
        {
            if (Defunct)
                return;

            _manifestation.Highlight(HighlightType.PotentiallyRelevant, _payload);
        }

        public void ShowPossibleInteractionWithToken(Token token)
        {
            if (Defunct)
                return;

            _manifestation.Highlight(HighlightType.PotentiallyRelevant, _payload);

        }

        public void StopShowingPossibleInteractionWithToken(Token token)
        {
            _manifestation.Unhighlight(HighlightType.PotentiallyRelevant, _payload);
        }

        public void ApplyExoticEffect(ExoticEffect exoticEffect)
        {
            Payload.ApplyExoticEffect(exoticEffect);

        }

        public void StartWalk(TokenPathItinerary pathItinerary)
        {
            var seeker = GetComponent<Seeker>();

           Path p= seeker.StartPath(pathItinerary.Anchored3DStartPosition, pathItinerary.Anchored3DEndPosition, OnPathComplete);
               p.BlockUntilCalculated();

            
        }

        public void OnPathComplete(Path path)
        {
      
        }

        public void Update()
        {
            WorldPosition = TokenRectTransform.position;

        }
    }
}