#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Assets.Scripts.Application.Entities.NullEntities;
using Assets.Scripts.Application.Fucine;
using Assets.Scripts.Application.Infrastructure.Events;
using JetBrains.Annotations;
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
using SecretHistories;
using SecretHistories.Spheres.Angels;
using SecretHistories.Spheres;
using Pathfinding;
using SecretHistories.Manifestations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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


        [Header("Display")]
        [SerializeField] protected bool shrouded;
       
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

        public bool MatchesPathTokenId(string pathTokenId)
        {
            //very easy to pass !bridge in when we mean bridge, or otherwise spaff up
            string payloadId = PayloadId;
            if (pathTokenId == payloadId)
                return true;
            if ($"!{pathTokenId}" == payloadId)
                return true;
            if (pathTokenId== $"!{payloadId}")
                return true;
            return false;
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
        private string TokenState;

        [SerializeField]
        private string FullPathAsString;

        protected void UpdateVisuals(ITokenPayload payload)
        {
            if (Sphere.ContentsHidden)
                return;
            _manifestation.UpdateVisuals(Payload, Sphere);
            if (_ghost != null)
                _ghost.UpdateVisuals(_payload, Sphere);
        }


        public virtual bool IsValid()
        {
            if (this.Equals(null)) //in process of being destroyed
                return false;

            if (Defunct)
                return false;

            return Payload.IsValid();
        }

        public bool IsValidElementStack()
        {
            return Payload.IsValidElementStack();
        }


        public bool PlacementAlreadyChronicled = false;

        private ITokenPayload _payload;

        private HomingAngel _homingAngel;

        /// <summary>
        /// could be null, remember: careful
        /// </summary>
        /// <returns></returns>
        public HomingAngel GetCurrentHomingAngel()
        {
            return _homingAngel;
        }

        public Sphere GetHomeSphere()
        {
            //I don't like using null rather than a null object as the marker for 'no home'.
            //This a a candidate for refactoring as and when we do more sophisticated things with HomingAngel
            //PS at the moment the CS behaviour for a null homingangel that's evicted is to go the Dropzone, which I do like and which I should preserve in any refactoring

            if (_homingAngel == null) //slight tangle here: references to the homingangel from here (where we need it to check home) and the homesphere itself (where the angel lives)
                return Watchman.Get<HornedAxe>().GetDefaultSphere();
            if(_homingAngel.GetWatchedSphere()==null)
                return Watchman.Get<HornedAxe>().GetDefaultSphere();

            return _homingAngel.GetWatchedSphere();

        }

        public float GetCurrentHeight()
        {
            UpdateRectTransformSizeFromManifestation();
            return TokenRectTransform.rect.height;
        }

        public float GetCurrentWidth()
        {
            UpdateRectTransformSizeFromManifestation();
            return TokenRectTransform.rect.width;
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

        UpdateRectTransformSizeFromManifestation();

            manifestationToRetire.Retire(vfxForOldManifestation, OnReplacedManifestationRetired);

            Payload.InitialiseManifestation(_manifestation);

            if (shrouded)
                _manifestation.Shroud(true);
            else
                _manifestation.Unshroud(true);

            if(_ghost!=null && !_ghost.Equals(null))
                _ghost.Retire();

            _ghost = _manifestation.CreateGhost();
            _ghost.UpdateVisuals(Payload);
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
        public bool DisplayGhostAtProjectedChoreoPosition(Sphere projectInSphere)
        {
            if (_ghost == null)
                return false;


            var projectionPositionLocalToSphere = GetProjectionPositionLocalToSphere(projectInSphere);

            var candidatePosition = projectInSphere.Choreographer.GetClosestFreeLocalPosition(this, projectionPositionLocalToSphere);

            if(projectInSphere.EmphasiseContents)
                Emphasise();
            if (projectInSphere.UnderstateContents)
                Understate();

            _ghost.ShowAt(projectInSphere, candidatePosition,TokenRectTransform);
            

            //if we're showing a ghost, then we shouldn't show a ready-to-interact glow.
            _manifestation.Unhighlight(HighlightType.WillInteract, _payload);

            return true;
        }

        public bool DisplayGhostAtSpecifiedChoreoPosition(Sphere projectInSphere, Vector3 specifyPosition)
        {
            if (_ghost == null)
                return false;


            var projectionPositionLocalToSphere = projectInSphere.GetRectTransform().InverseTransformPoint(specifyPosition);

            var candidatePosition = projectInSphere.Choreographer.GetClosestFreeLocalPosition(this, projectionPositionLocalToSphere);

            _ghost.ShowAt(projectInSphere, candidatePosition, TokenRectTransform);

            //if we're showing a ghost, then we shouldn't show a ready-to-interact glow.
            _manifestation.Unhighlight(HighlightType.WillInteract, _payload);

            return true;
        }

        public Vector3 GetProjectionPositionLocalToSphere(Sphere projectInSphere)
        {
            var tokenWorldPosition = Sphere.GetRectTransform().TransformPoint(Location.Anchored3DPosition);
            var projectionPositionLocalToSphere = projectInSphere.GetRectTransform().InverseTransformPoint(tokenWorldPosition);
            return projectionPositionLocalToSphere;
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

                if (_manifestation.GetType() != Payload.GetManifestationType(Sphere))
                {
                    Type newManifestationType = Payload.GetManifestationType(Sphere);

                    var newManifestation = Watchman.Get<PrefabFactory>()
                        .CreateManifestationPrefab(newManifestationType, this.transform);

                    ReplaceManifestation(_manifestation, newManifestation, RetirementVFX.None);
                }

                UpdateVisuals(Payload);

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
            reManifestation.Transform.SetAsFirstSibling();
            
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
              //  if (oldSphere.ContentsHidden && !newSphere.ContentsHidden)
              //      UpdateVisuals(Payload);  <--moved this to Manifest(). Leaving comment in case there was a reason I put it here.
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
        {
            StartDrag(eventData);
            Watchman.Get<Meniscate>().StartMultiDragAlong(this, eventData); //here not in startdrag, unless we like stack overflows
        }
            
        else
        {
            eventData.pointerDrag = null; //AFAICT this is a sensible precaution, but check here first if we're relying on non-drag/drag behaviour in a token somewhere
            eventData.dragging=false;

        }
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
                && !CurrentState.InPlayerDrivenMotion()
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
            
            
            return true;
        }

        public void RequestHomingAngelFromCurrentSphere()
        {
            _homingAngel = Sphere.TryCreateHomingAngelFor(this);

        }

        protected void StartDrag(PointerEventData eventData)
        {
            eventData.hovered.Clear();//why does the hover data not clear itself? I don't know, and Unity doesn't know, so here we are for now.

            //remember the original location in case the token gets evicted later
            //base behaviour is to set current location in current sphere as home, but not all spheres will do this
            RequestHomingAngelFromCurrentSphere();


            Vector3 pressPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Sphere.GetRectTransform(), eventData.pressPosition, eventData.pressEventCamera, out pressPos);
            dragOffset = (transform.position) - pressPos;
            //dragOffset = (transform.position + startParent.position) - pressPos;


            CurrentState = new BeingDraggedState();
             Watchman.Get<Meniscate>().SetCurrentlyDraggedToken(this);
            
            NotifyInteracted(new TokenInteractionEventArgs(pointerEventData: eventData, payload: Payload, token: this,
                sphere: Sphere, interaction: Interaction.OnDragBegin));
            //just picked the token up, but it hasn't yet left the origin sphere. 
            TryCalveOriginToken(_homingAngel);

 
            var enrouteSphere = Payload.GetEnRouteSphere();

            enrouteSphere.AcceptToken(this, new Context(Context.ActionSource.PlayerDrag));
            
            TokenRectTransform.SetAsLastSibling();
            _manifestation.OnBeginDragVisuals(this);

            MakeNonInteractable();

            startSiblingIndex = TokenRectTransform.GetSiblingIndex();


            SoundManager.PlaySfx("CardPickup");

 
        }

        public void StartDragAlong(PointerEventData eventData, Token primaryDragToken)
        {
            if(CanBeDragged() && primaryDragToken!=this)
                StartDrag(eventData);
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
            if(homingAngel!=null) //really need to fix this
                homingAngel.SetOriginToken(stackLeftBehind);
           
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (!CurrentlyBeingDragged())
                return;

            Vector3 originalPosition = this.transform.position;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(Sphere.GetRectTransform(),
                eventData.position, eventData.pressEventCamera, out var draggedToPosition);

            MoveObject(draggedToPosition);
            _manifestation.DoMove(eventData, TokenRectTransform);
            NotifyInteracted(new TokenInteractionEventArgs(pointerEventData: eventData, payload: Payload, token: this,
                sphere: Sphere, interaction: Interaction.OnDrag));
            Watchman.Get<Meniscate>().OnMultiDragAlong(originalPosition, this);
        }

        public void ContinueDragAlong(Vector3 originalPrimaryDragTokenPosition, Token primaryDragToken )
        {
            
            Vector3 offsetVector = primaryDragToken.transform.position - originalPrimaryDragTokenPosition;
            Vector3 newPosition = this.transform.position + offsetVector;
            MoveObject(newPosition);

        }


        public void MoveObject(Vector3 toPosition)
        {
            
            // Potentially change this so it is using UI coords and the RectTransform?
            //  rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

         TokenRectTransform.position = toPosition + dragOffset; ///aaaahh it's *position* not anchoredposition3D because we're getting the world point from the click

         Payload.OnTokenMoved(Location);
            

        }


        public  void OnEndDrag(PointerEventData eventData)
        {
            //This is called after OnDrop. So if the token has been dropped on something else, it may already have 
            //been accepted by a new sphere and stabilised.
            //So it may in fact be in a Docked state already.
            //Our job here is to notify DragEnd;
            //call CompleteDrag if it hasn't already been called;
            //and tidy up any MultiDrag (which maybe should go in FinishDrag
            NotifyInteracted(new TokenInteractionEventArgs(pointerEventData: eventData, payload: Payload, token: this,
                sphere: Sphere, interaction: Interaction.OnDragEnd));

                CompleteDrag();
            Watchman.Get<Meniscate>().OnMultiEndDrag(eventData,this);

        }

        public void EndDragAlong(PointerEventData eventData,Token primaryDragToken)
        {

        
                NotifyInteracted(new TokenInteractionEventArgs(pointerEventData: eventData, payload: Payload,
                    token: this, sphere: Sphere, interaction: Interaction.OnDragEnd));
                CompleteDrag();
              
        }

        public void ForceEndDrag()
        {

                CompleteDrag();
        }

        public void CompleteDrag()
        {
            Watchman.Get<Meniscate>().ClearCurrentlyDraggedToken();

            //FinishDrag tidies everything up. It's called from OnEndDrag() but it can also be called 
            //externally if we want to cancel the drag.
            MakeInteractable();
            
            if (!CurrentState.Docked() && !CurrentState.InSystemDrivenMotion())
                //evict the token before hiding the ghost. If the ghost is still active, it'll give the evicted token a place to go.
                this.Sphere.EvictToken(this,new Context(Context.ActionSource.PlayerDrag));

            //Commented this out: we're now hiding a ghost when a token travel itinerary completes instead.
            //This is because FinishDrag() is also used for path itineraries.
            //If the change causes problems, we can fork logic here instead?
            // HideGhost(); 
            _manifestation.OnEndDragVisuals(this);
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

        public void OnCompletedTravelItinerary()
        {
            CurrentState = new TravelledToSphere();
            HideGhost();
        }

        public void OnCompletedPathItinerary()
        {
            HideGhost();
        }

        public OccupiesSpaceAs OccupiesSpaceAs()
        {
            return _manifestation.OccupiesSpaceAs();
        }

        public bool OccupiesSameSpaceAs(Token otherToken)
        {
            //e.g, Dropzones are intangibles and can share space with whatever they like
            if (otherToken.OccupiesSpaceAs() == OccupiesSpaceAs())
                return true;

            return false;
        }

        public  void OnDrop(PointerEventData eventData)
        {
            //Inevitable but endlessly confusing. OnDrop is 'something has been dropped on me' and not 'I have been dropped on something'
            //So the incomingtoken / potentialusurper is what is currently being dragged
            //and the token on which this method has been called is the token that is currently in situ
            var incomingToken = eventData.pointerDrag.GetComponent<Token>();
            if (incomingToken == null)
                return;

            if (!OccupiesSameSpaceAs(incomingToken))
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
            NotifyInteracted(new TokenInteractionEventArgs(token: this, payload: Payload, sphere: Sphere,
                pointerEventData: eventData, interaction: Interaction.OnReceivedADrop));

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
                ForceEndDrag();
                return this;
            }

            var calvedStackCreationCommand = new ElementStackCreationCommand(Payload.EntityId, quantityToLeaveBehind)
            {
                Mutations = Payload.Mutations,
                LifetimeRemaining = Payload.GetTimeshadow().LifetimeRemaining,
                Illuminations = Payload.GetIlluminations()
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
            //multiselect not yet stable
            if (Keyboard.current.shiftKey.isPressed)
            {
                Watchman.Get<Meniscate>().ToggleMultiSelectedToken(this);
                return;
            }

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
                NotifyInteracted(new TokenInteractionEventArgs(payload: Payload, token: this, sphere: Sphere,
                    pointerEventData: eventData, interaction: Interaction.OnDoubleClicked));

            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                NotifyInteracted(new TokenInteractionEventArgs(payload: Payload, token: this, sphere: Sphere,
                    pointerEventData: eventData, interaction: Interaction.OnRightClicked));
            }
            else
            {
                if (shrouded)
                {
                    Unshroud(false);
                }
                else
                {
                    NotifyInteracted(new TokenInteractionEventArgs(payload: Payload, token: this, sphere: Sphere,
                        pointerEventData: eventData, interaction: Interaction.OnClicked));
                }

                // this moves the clicked sibling on top of any other nearby cards.
                //Lots of brittle special-casing here! TODO: when do we actually want this behaviour?
                if (Sphere.GetType() != typeof(ThresholdSphere) 
                    && Sphere.GetType() != typeof(ExhibitCardsSphere)
                    && Sphere.GetType() != typeof(SituationStorageSphere))
                    transform.SetAsLastSibling();

                previousClickTime = eventData.clickTime;
            }

        }

        public void GoAway(Context context)
        {
            CurrentState = new EvictedState();
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

            List<ISphereCatalogueEventSubscriber> subscribersAttached =
                this.gameObject.GetComponents<ISphereCatalogueEventSubscriber>().ToList();

            foreach (var s in subscribersAttached)
            {
                Watchman.Get<HornedAxe>().Unsubscribe(s);
            }


            //The bit above should take care of anything like TokenMovementReactionDecorator; but it's still very important that OnCurrentManifestationRetired is called. If it's not, the token may continue to exist as a shell of an object, and blow up various subscriptions.
            _manifestation.Retire(vfx, OnCurrentManifestationRetired);
            _payload.Retire(vfx);
            var args=new SphereContentsChangedEventArgs(Sphere, new Context(Context.ActionSource.Retire));
            args.TokenRemoved = this;
            Sphere.NotifyTokensChangedForSphere(args);

            SetSphere(Watchman.Get<Limbo>(), new Context(Context.ActionSource.Retire));

            _ghost.Retire();


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
            try
            {

            if (!eventData.dragging && !Watchman.Get<Meniscate>().IsMultiSelected(this))
                _manifestation.Unhighlight(HighlightType.Hover, _payload);

            NotifyInteracted(new TokenInteractionEventArgs(payload: Payload, token: this, sphere: Sphere,
                pointerEventData: eventData, interaction: Interaction.OnPointerExited));

            }
            catch (Exception e)
            {
             NoonUtility.Log($"Catch for 'Varley', which should no longer occur now we've removed the OnPointerExit->ExecuteEvent from CardManifestation OnPointerExit... but let's be safe. Exception: {e}");
            }

        }

        public void TravelTo(TokenTravelItinerary itinerary,Context context)
        {
          itinerary.Depart(this,context);
        }

        private void OnPayloadChanged(TokenPayloadChangedArgs args)
        {
            if (args.ChangeType == PayloadChangeType.Fundamental)
                Remanifest(RetirementVFX.CardTransformWhite);
            else if (args.ChangeType == PayloadChangeType.Update)
            {
                UpdateVisuals(_payload);
                
                PlacementAlreadyChronicled = false; //should really only do this if the element has changed
                var sphereContentsChangedArgs = new SphereContentsChangedEventArgs(Sphere, args.Context);
                sphereContentsChangedArgs.TokenChanged = this;
                Sphere.NotifyTokensChangedForSphere(sphereContentsChangedArgs);
            }
            else if (args.ChangeType == PayloadChangeType.Retirement)
                Retire(args.VFX);

        }

        public void Select()
        {
            _manifestation.Highlight(HighlightType.Selected,Payload);
        }

        public void Deselect()
        {
            _manifestation.Unhighlight(HighlightType.Selected, Payload);
        }

        public void Understate()
        {
            //in the case of items which can be examined more closely, like books, Understate displays the storage version of the item
            _manifestation.Understate();
            UpdateRectTransformSizeFromManifestation();

            if (_ghost != null)
                _ghost.Understate();
        }

        public void Emphasise()
        {
            //in the case of items which can be examined more closely, like books, Emphasise displays the expanded version of the item
            _manifestation.Emphasise();
            UpdateRectTransformSizeFromManifestation();
            if (_ghost != null)
                _ghost.Emphasise();
        }

        private void UpdateRectTransformSizeFromManifestation()
        {
            TokenRectTransform.sizeDelta = new Vector2(_manifestation.RectTransform.sizeDelta.x,
                _manifestation.RectTransform.sizeDelta.y);
            TokenRectTransform.anchorMin = _manifestation.RectTransform.anchorMax;
            TokenRectTransform.anchorMax = _manifestation.RectTransform.anchorMax;
            TokenRectTransform.pivot = _manifestation.RectTransform.pivot;
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

        public void StopShowingPossibleInteractions()
        {
            _manifestation.Unhighlight(HighlightType.PotentiallyRelevant, _payload);
        }

        public void ApplyExoticEffect(ExoticEffect exoticEffect)
        {
            Payload.ApplyExoticEffect(exoticEffect);

        }



        public void Update()
        {
            WorldPosition = TokenRectTransform.position;
            TokenState = CurrentState.ToString();

        }

    }
}