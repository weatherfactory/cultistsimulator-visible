#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.TokenContainers;
using Noon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.CS.TabletopUI {

    public enum TokenXNess
    {
        NoValidDestination,
        ValidDestination,
        DivertedByGreedySlot,
        DoesntMatchSlotRequirements,
        DroppedOnTableContainer,
        ReturningSplitStack,
        ReturnedToStartingSlot,
        PlacedInSlot,
        ElementDroppedOnTokenButCannotInteractWithIt,
        DroppedOnTokenWhichMovedAside,
        DroppedOnTokenWhichWontMoveAside,
        MergedIntoStack
    }

        [RequireComponent(typeof(RectTransform))]
    public class Token : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,ISituationSubscriber
    {
        protected bool singleClickPending = false;


        protected bool shrouded = false;
        protected Situation _situation;
        public IVerb Verb
        {
            get { return _situation.Verb; }
        }

        public ElementStack ElementStack { get; private set; }

        public int ElementQuantity => ElementStack.Quantity;

        public Element Element => ElementStack._element;


        public RectTransform rectTransform;
        [SerializeField] protected bool rotateOnDrag = true;
        
        [HideInInspector] public Vector2? LastTablePos = null; // if it was pulled from the table, save that position

        protected  IManifestation _manifestation;

        protected Transform startParent;
        protected Vector3 startPosition;
        protected int startSiblingIndex;
        protected Vector3 dragOffset;
        protected RectTransform rectCanvas;
        protected CanvasGroup canvasGroup;
        private Token originToken = null; // if it was pulled from a stack, save that stack!


        //set true when the Chronicler notices it's been placed on the desktop. This ensures we don't keep spamming achievements / Lever requests. It isn't persisted in saves! which is probably fine.
        public bool PlacementAlreadyChronicled = false;

        protected float perlinRotationPoint = 0f;
        protected float dragHeight = -8f; // Draggables all drag on a specifc height and have a specific "default height"

        public Sphere Sphere;
        protected Sphere OldSphere; // Used to tell OldContainsTokens that this thing was dropped successfully

        public RectTransform RectTransform
        {
            get { return rectTransform; }
        }

        public TokenLocation Location => new TokenLocation(transform.position, Sphere.GetPath());

        protected virtual void Awake() {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
                Sphere= Registry.Get<NullContainer>();

                _manifestation=new NullManifestation();
                ElementStack=new NullElementStack();
        }

        public void StartArtAnimation()
        {
            if (!CanAnimateArt())
                return;
            _manifestation.BeginArtAnimation();

        }
        public bool CanAnimateArt()
        {
            if (gameObject == null)
                return false;

            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated


            return _manifestation.CanAnimate();
        }

        public bool IsInMotion { get; set; }
        public bool Defunct { get; protected set; }
        public  bool NoPush => _manifestation.NoPush;
        public bool _currentlyBeingDragged { get; protected set; }
        protected bool _draggingEnabled = true;

        private TokenXNess TokenXNess { get; set; }



        protected AnchorDurability _durability;


        public AnchorDurability Durability
        {
            get { return _durability; }
        }

        public void Populate(Situation situation)
        {
            _situation = situation;
            _manifestation = Registry.Get<PrefabFactory>().CreateManifestationPrefab(situation.Species.AnchorManifestationType, this.transform);


            if (Verb.Transient)
                _durability = AnchorDurability.Transient;
            else
                _durability = AnchorDurability.Enduring;

            name = "Verb_" + Verb.Id;


            _manifestation.InitialiseVisuals(Verb);
            _manifestation.DisplaySpheres(new List<Sphere>());
            _manifestation.SetParticleSimulationSpace(transform);

        }

        private void SwapOutManifestation(IManifestation oldManifestation, IManifestation newManifestation, RetirementVFX vfxForOldManifestation)
        {
            var manifestationToRetire = oldManifestation;
            _manifestation = newManifestation;

            manifestationToRetire.Retire(vfxForOldManifestation, OnSwappedOutManifestationRetired);

        }

        private void OnSwappedOutManifestationRetired()
        {
            //
        }

        public void Manifest(Sphere forContainer)
        {
            if (Element.Id == "dropzone")
            {
                _manifestation = Registry.Get<PrefabFactory>().CreateManifestationPrefab(nameof(DropzoneManifestation), this.transform);
                return;
            }


            if (_manifestation.GetType() != forContainer.ElementManifestationType)
            {

                var newManifestation = Registry.Get<PrefabFactory>().CreateManifestationPrefab(forContainer.ElementManifestationType.Name, this.transform);
                SwapOutManifestation(_manifestation, newManifestation, RetirementVFX.None);
            }

            _manifestation.InitialiseVisuals(ElementStack._element);
            _manifestation.UpdateVisuals(ElementStack._element, ElementStack.Quantity);

        }


        public void TryReturnToOriginalPosition()
        {
            if (LastTablePos != null)
                transform.localPosition = new Vector3(LastTablePos.Value.x,LastTablePos.Value.y);
        }



            public void Start()
        {
            if(Sphere.GetType()!=typeof(CardsPile))
               Registry.Get<LocalNexus>().TokenInteractionEvent.AddListener(ReactToDraggedToken);

            SetXNess(TokenXNess.NoValidDestination);

        }

        public void SetXNess(TokenXNess xness)
        {
            TokenXNess = xness;
        }


        public bool ShouldReturnToStart()
        {
            return TokenXNess == TokenXNess.NoValidDestination ||
                TokenXNess==TokenXNess.DivertedByGreedySlot ||
                TokenXNess==TokenXNess.ReturningSplitStack ||
                TokenXNess == TokenXNess.ReturnedToStartingSlot ||
                TokenXNess==TokenXNess.DroppedOnTokenWhichWontMoveAside;
        }

        protected virtual bool AllowsDrag() {
            return !IsInMotion &&  !shrouded && !_manifestation.RequestingNoDrag; ;
        }



        /// <summary>
        /// This is an underscore-separated x, y localPosition in the current transform/containsTokens
        /// but could be anything
        /// </summary>
        public string SaveLocationInfo {
            set {
                var locs = value.Split('_');
                if (float.TryParse(locs[0], out float x) && float.TryParse(locs[1], out float y))
                {
                    rectTransform.localPosition = new Vector3(x, y);
                }
                //if not, then we specified the location as eg 'slot'

            }
            get {
                return Sphere.GetPath() + "_" + Guid.NewGuid();
            }
        }




        public virtual void SnapToGrid()
		{
			transform.localPosition = Registry.Get<Choreographer>().SnapToGrid( transform.localPosition );
		}

        public virtual void SetSphere(Sphere newSphere, Context context) {
            OldSphere = Sphere;
            Sphere = newSphere;

            if (OldSphere != null && OldSphere != newSphere)
            {
                OldSphere.RemoveToken(this);
                if (OldSphere.ContentsHidden && !newSphere.ContentsHidden)
                    _manifestation.UpdateVisuals(_element, Quantity);
            }

            Sphere = newSphere;
        }

        public bool IsInContainer(Sphere compareContainer, Context context) {
            return compareContainer == Sphere;
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CanDrag(eventData))
            {
                _currentlyBeingDragged = true;
                Registry.Get<LocalNexus>().SignalTokenBeginDrag(this, eventData);
                StartDrag(eventData);
            }

        }

        bool CanDrag(PointerEventData eventData) {

            if (!_draggingEnabled)
                return false;

            if (!Sphere.AllowDrag || !AllowsDrag())
                return false;


            return true;
        }

        protected virtual void StartDrag(PointerEventData eventData) {


            //if (rectCanvas == null)
            //    rectCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            if (!Keyboard.current.shiftKey.wasPressedThisFrame)
            {
               ElementStack.SplitOffNCardsToNewStack(1, new Context(Context.ActionSource.PlayerDrag));
            }


            _currentlyBeingDragged = true;

            var enrouteContainer = Registry.Get<SphereCatalogue>().GetContainerByPath(
                new SpherePath(Registry.Get<ICompendium>().GetSingleEntity<Dictum>().DefaultEnRouteSpherePath));
            enrouteContainer.AcceptToken(this, new Context(Context.ActionSource.PlayerDrag));
            _manifestation.OnBeginDragVisuals();


            TokenXNess = TokenXNess.NoValidDestination;
            canvasGroup.blocksRaycasts = false;

     
            startPosition = rectTransform.localPosition;
            startParent = rectTransform.parent;
            startSiblingIndex = rectTransform.GetSiblingIndex();

			if (rectTransform.anchoredPosition.sqrMagnitude > 0.0f)	// Never store 0,0 as that's a slot position and we never auto-return to slots - CP
			{
	            LastTablePos = rectTransform.anchoredPosition;
			}

            rectTransform.SetParent(Registry.Get<IDraggableHolder>().RectTransform);
            rectTransform.SetAsLastSibling();

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
			TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.Drag, true );

        }



        public void OnDrag(PointerEventData eventData)
        {
            if (!_currentlyBeingDragged)
                return;

            if (_draggingEnabled)
			{
                Sphere.OnTokenDragged(new TokenEventArgs{PointerEventData = eventData});
                MoveObject(eventData);
            }
            else
			{
                eventData.pointerDrag = null; // cancel the drag
                OnEndDrag(eventData);
            }
        }



        public void MoveObject(PointerEventData eventData)
        {
            Vector3 dragPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Get<IDraggableHolder>().RectTransform, eventData.position, eventData.pressEventCamera, out dragPos);

            // Potentially change this so it is using UI coords and the RectTransform?
          //  rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

            _manifestation.DoMove(rectTransform);

            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f)
            {
                // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }



        }


        public virtual void OnEndDrag(PointerEventData eventData) {
            Registry.Get<LocalNexus>().SignalTokenEndDrag(this,eventData);
            FinishDrag();
        }


        public virtual void FinishDrag() {
            if (_currentlyBeingDragged)
            {
                _currentlyBeingDragged = false;
                canvasGroup.blocksRaycasts = true;

                if (ShouldReturnToStart())
                    ReturnToStartPosition();

                TabletopManager.RequestNonSaveableState(TabletopManager.NonSaveableType.Drag, false);   // There is also a failsafe to catch unexpected aborts of Drag state - CP
                
            }
        }

        public void ReturnToStartPosition() {
            if (startParent == null) {
                //newly created token! If we try to set it to startposition, it'll disappear into strange places
                ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
                return; // no sound on new token
            }

            SoundManager.PlaySfx("CardDragFail");
            var tabletopContainer = startParent.GetComponent<TabletopSphere>();
            
            // Token was from tabletop - return it there. This auto-merges it back in case of ElementStacks
            // The map is not the tabletop but inherits from it, so we do the IsTabletop check
            if (tabletopContainer != null && tabletopContainer.IsTabletop) {
                ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }
            else {
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.SetParent(startParent);
                rectTransform.SetSiblingIndex(startSiblingIndex);
                rectTransform.localPosition = startPosition;
            }
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            
            var incomingToken = eventData.pointerDrag.GetComponent<Token>();
            if (incomingToken == null)
                return;

            if(CanInteractWithIncomingToken(incomingToken))
                InteractWithIncomingToken(incomingToken,eventData);
            else
            {
                this.Sphere.TryMoveAsideFor(this, incomingToken, out bool moveAsideFor);

            if (moveAsideFor)
                SetXNess(TokenXNess.DroppedOnTokenWhichMovedAside);
            else
                SetXNess(TokenXNess.DroppedOnTokenWhichWontMoveAside);
            }
        }

        public bool CanInteractWithIncomingToken(Token incomingToken)
        {
            //can we merge tokens?
            if (incomingToken.ElementStack.CanMergeWith(incomingToken.ElementStack))
                return true;

            //can we put a stack in a threshold associated with this token?
            if (_situation.GetFirstAvailableThresholdForStackPush(incomingToken.ElementStack).SphereCategory ==
                SphereCategory.Threshold)
                return true;

            return false;
        }

        private void InteractWithIncomingToken(Token incomingToken,PointerEventData eventData)
        {
            Sphere.OnTokenReceivedADrop(new TokenEventArgs
            {
                Token = this,
                Container = Sphere,
                PointerEventData = eventData
            });
            if (ElementStack.IsValidElementStack() && incomingToken.ElementStack.IsValidElementStack())
            {
                if(incomingToken.ElementStack.CanMergeWith(incomingToken.ElementStack))
                    AcceptIncomingStackForMerge(incomingStack);
                else
                {
                    ElementStack.ShowNoMergeMessage(incomingToken.ElementStack);

                    incomingToken.Sphere.TryMoveAsideFor(this, incomingToken, out bool moveAsideFor);

                    if (moveAsideFor)
                        SetXNess(TokenXNess.DroppedOnTokenWhichMovedAside);
                }
            }

            else if (incomingToken.ElementStack.IsValidElementStack())
            {

                _situation.PushDraggedStackIntoThreshold(incomingToken.ElementStack);

                // Then we open the situation (cause this closes other situations and this may return the stack we try to move
                // back onto the tabletop - if it was in its starting slots. - Martin
                if (!_situation.IsOpen)
                    _situation.OpenAtCurrentLocation();
                return;
            }
            else
            {
                // We can't interact? Then dump us on the tabletop
                incomingToken.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }

        }



        public virtual void OnPointerClick(PointerEventData eventData)
        {



            if (!_manifestation.HandleClick(eventData, this))
            {
                //Manifestation didn't handle click
                Registry.Get<DebugTools>().SetInput(_situation.RecipeId);

                if (!_situation.IsOpen)
                    _situation.OpenAtCurrentLocation();
                else
                    _situation.Close();
            }



            if (eventData.clickCount > 1)
            {
                // Double-click, so abort any pending single-clicks
                singleClickPending = false;
                Sphere.OnTokenDoubleClicked(new TokenEventArgs
                {
                    Element = ElementStack._element,
                    Token = this,
                    Container = Sphere,
                    PointerEventData = eventData
                });


                SendStackToNearestValidSlot();
            }
            else
            {
                // Single-click BUT might be first half of a double-click
                // Most of these functions are OK to fire instantly - just the ShowCardDetails we want to wait and confirm it's not a double
                singleClickPending = true;


                if (shrouded)
                {
                    Unshroud(false);

                }
                else
                {
                    Sphere.OnTokenClicked(new TokenEventArgs
                    {
                        Element = ElementStack._element,
                        Token = this,
                        Container = Sphere,
                        PointerEventData = eventData
                    });
                }

                // this moves the clicked sibling on top of any other nearby cards.
                if (Sphere.GetType() != typeof(RecipeSlot) && Sphere.GetType() != typeof(ExhibitCards))
                    transform.SetAsLastSibling();
            }

        }


        public virtual void ReturnToTabletop(Context context)
        {
            Registry.Get<Choreographer>().ArrangeTokenOnTable(this, context);

            bool stackBothSides = true;

            //if we have an origin stack and the origin stack is on the tabletop, merge it with that.
            //We might have changed the element that a stack is associated with... so check we can still merge it
            if (originToken != null && originToken.Sphere.SphereCategory==SphereCategory.World && ElementStack.CanMergeWith(originToken.ElementStack))
            {
                originStack.AcceptIncomingStackForMerge(this);
                return;
            }
            else
            {
                var tabletop = Registry.Get<TabletopManager>()._tabletop;
                var existingStacks = tabletop.GetElementTokens();

    //check if there's an existing stack of that type to merge with
                    foreach (var stack in existingStacks)
                    {
                        if (CanMergeWith(stack))
                        {
                            var elementStack = stack as ElementStack;
                            elementStack.AcceptIncomingStackForMerge(this);
                            return;
                        }
                    }
                

                if (LastTablePos == null)   // If we've never been on the tabletop, use the drop zone
                {
                    // If we get here we have a new card that won't stack with anything else. Place it in the "in-tray"
                    LastTablePos = GetDropZoneSpawnPos();
                    stackBothSides = false;
                }
            }

            Registry.Get<Choreographer>().ArrangeTokenOnTable(this, context, LastTablePos, false, stackBothSides);	// Never push other cards aside - CP

        }


        public void DisplayOverrideIcon(string icon)
        {
            _manifestation.OverrideIcon(icon);
        }

        public virtual void DisplayAtTableLevel() {
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0f);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
			LastTablePos = rectTransform.anchoredPosition3D;
            NotifyChroniclerPlacedOnTabletop();
        }

        protected void NotifyChroniclerPlacedOnTabletop()
        {
            Registry.Get<Chronicler>()?.TokenPlacedOnTabletop(this);
        }

        public virtual bool Retire()
        {
          return  Retire(RetirementVFX.None);
        }
        public virtual bool Retire(RetirementVFX vfx)
        {
            Defunct = true;
            Destroy(gameObject);
            return true;
        }




        public virtual void ReactToDraggedToken(TokenInteractionEventArgs args)
        {
            if (args.TokenInteractionType == TokenInteractionType.BeginDrag)
            {

                var stack = args.Token;

                if (!_situation.GetFirstAvailableThresholdForStackPush(stack).CurrentlyBlockedFor(BlockDirection.Inward))
                {
                    _manifestation.Highlight(HighlightType.CanInteractWithOtherToken);
                }
            }
            if (args.TokenInteractionType == TokenInteractionType.EndDrag)
                _manifestation.Unhighlight(HighlightType.CanInteractWithOtherToken);

        }

        public virtual void HighlightPotentialInteractionWithToken(bool show)
        {
            if (show)
                _manifestation.Highlight(HighlightType.CanInteractWithOtherToken);
            else
                _manifestation.Unhighlight(HighlightType.CanInteractWithOtherToken);


        }


        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _manifestation.Highlight(HighlightType.Hover);

            var tabletopManager = Registry.Get<TabletopManager>();
            if (tabletopManager != null) //eg we might have a face down card on the credits page - in the longer term, of course, this should get interfaced
            {
                if (!shrouded && ElementStack.IsValidElementStack())
                    tabletopManager.SetHighlightedElement(Element.Id, ElementQuantity);
                else
                    tabletopManager.SetHighlightedElement(null);
            }


        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _manifestation.Unhighlight(HighlightType.Hover);

            Sphere.OnTokenPointerExited(new TokenEventArgs
            {
                Element = Element,
                Token = this,
                Container = Sphere,
                PointerEventData = eventData
            });


            var ttm = Registry.Get<TabletopManager>();
            if (ttm != null)
            {
                Registry.Get<TabletopManager>().SetHighlightedElement(null);
            }

        }

        public void BurnImageUnderToken(string burnImage)
        {
            Registry.Get<INotifier>()
                .ShowImageBurn(burnImage, this, 20f, 2f,
                    TabletopImageBurner.ImageLayoutConfig.CenterOnToken);
        }

        public virtual void AnimateTo(float duration, Vector3 startPos, Vector3 endPos, Action<Token> animDone, float startScale = 1f, float endScale = 1f)
        {
            _manifestation.AnimateTo(this, duration, startPos, endPos, animDone, startScale, endScale);
        }


        public void DisplaySituationState(Situation situation)
        {
            switch (situation.State)
            {
                case SituationState.Unstarted:
                    _manifestation.SendNotification(new Notification(string.Empty, "-1"));
                    _manifestation.DisplaySpheres(new List<Sphere>());
                    break;
                case SituationState.Ongoing:
                    _manifestation.SendNotification(new Notification(string.Empty, "-1"));
                    if (situation.CurrentBeginningEffectCommand != null)
                    {
                        //if (situation.CurrentBeginningEffectCommand.OngoingSlots.Any())
                        //    _manifestation.DisplaySpheres(situation.CurrentBeginningEffectCommand.OngoingSlots[0].Greedy);
                        if (!string.IsNullOrEmpty(situation.CurrentBeginningEffectCommand.BurnImage))
                            BurnImageUnderToken(situation.CurrentBeginningEffectCommand.BurnImage);
                    }

                    _manifestation.UpdateTimerVisuals(situation.Warmup, situation.TimeRemaining, situation.intervalForLastHeartbeat, false, situation.currentPrimaryRecipe.SignalEndingFlavour);
                    break;

                case SituationState.Complete:
                    _manifestation.UpdateTimerVisuals(situation.Warmup, situation.TimeRemaining, situation.intervalForLastHeartbeat, false, situation.currentPrimaryRecipe.SignalEndingFlavour);

                    int completionCount = situation.GetStacks(SphereCategory.Output).Select(s => s.Quantity).Sum();

                    _manifestation.SendNotification(new Notification(string.Empty, completionCount.ToString()));
                    break;
            }
        }

        public void ContainerContentsUpdated(Situation situation)
        {
            var thresholdSpheresWithStacks = situation.GetSpheresByCategory(SphereCategory.Threshold)
                .Where(sphere => sphere.GetElementTokens().Count() == 1);

            _manifestation.DisplaySpheres(thresholdSpheresWithStacks);


            int completionCount = situation.GetStacks(SphereCategory.Output).Select(s => s.Quantity).Sum();
            _manifestation.SendNotification(new Notification(string.Empty, completionCount.ToString()));
        }

        public void ReceiveNotification(INotification n)
        {
            throw new NotImplementedException();
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

            //if a card has just been turned face up in a situation, it's now an existing, established card
            if (ElementStack.StackSource.SourceType == SourceType.Fresh)
                ElementStack.StackSource = Source.Existing();

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

    }
}