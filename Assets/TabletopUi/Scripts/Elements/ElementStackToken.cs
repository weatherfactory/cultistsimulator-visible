#pragma warning disable 0649
#define DROPZONE

using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Services;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.UI;
using Noon;
using UnityEngine.InputSystem;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI {
    public class ElementStackToken : AbstractToken, IGlowableView,IAnimatable
    {
        public const float SEND_STACK_TO_SLOT_DURATION = 0.2f;

        public event System.Action<ElementStackToken> onTurnFaceUp; // only used in the map to hide the other cards
        public event System.Action<float> onDecay;

 
        private IElementManifestation _manifestation;
        private Element _element;
        private int _quantity;

		// Cache aspect lists because they are EXPENSIVE to calculate repeatedly every frame - CP
		private IAspectsDictionary _aspectsDictionaryInc;		// For caching aspects including self 
		private IAspectsDictionary _aspectsDictionaryExc;		// For caching aspects excluding self
		private bool _aspectsDirtyInc = true;
		private bool _aspectsDirtyExc = true;

        // Interaction handling - CP
		protected bool singleClickPending = false;

        public float LifetimeRemaining { get; set; }
        private bool isFront = true;
        public Source StackSource { get; set; }

        private Coroutine turnCoroutine;
        

        private ElementStackToken originStack = null; // if it was pulled from a stack, save that stack!
        private Dictionary<string,int> _currentMutations; //not strictly an aspects dictionary; it can contain negatives
        private IlluminateLibrarian _illuminateLibrarian;
    

        private HashSet<ITokenObserver> observers=new HashSet<ITokenObserver>();

        //set true when the Chronicler notices it's been placed on the desktop. This ensures we don't keep spamming achievements / Lever requests. It isn't persisted in saves! which is probably fine.
        public bool PlacementAlreadyChronicled=false;


        public bool AddObserver(ITokenObserver observer)
        {
            if (observers.Contains(observer))
                return false;

            observers.Add(observer);
            return true;
        }

        public bool RemoveObserver(ITokenObserver observer)
        {
            if(!observers.Contains(observer))
                return false;
            observers.Remove(observer);
            return true;

        }

        public override string EntityId {
            get { return _element == null ? null : _element.Id; }
        }
        virtual public string Label
        {
            get { return _element == null ? null : _element.Label; }
        }

        virtual public string Icon
        {
            get { return _element == null ? null : _element.Icon; }
        }

        public string EntityWithMutationsId 
        {
	        // Generate a unique ID for a combination of entity ID and mutations
	        // IDs will look something like: entity_id?mutation_1=2&mutation_2=-1
	        get
	        {
		        var mutations = GetCurrentMutations();
		        return EntityId + "?" + string.Join(
			               "&", 
			               mutations.Keys
				               .Where(m => mutations[m] != 0)
				               .OrderBy(x => x)
				               .Select(m => $"{m}={mutations[m]}"));
	        }
        }

        virtual public bool Unique
        {
            get
            {
                if (_element == null)
                    return false;
                return _element.Unique;
            }
        }

        virtual public string UniquenessGroup
        {
            get { return _element == null ? null : _element.UniquenessGroup; }
        }

        virtual public bool Decays
		{
            get { return _element.Decays; }
        }

        virtual public int Quantity {
            get { return Defunct ? 0 : _quantity; }
        }

        virtual public Vector2? LastTablePos
		{
            get { return lastTablePos; }
			set { lastTablePos = value; }
        }

        virtual public bool MarkedForConsumption { get; set; }

        virtual public IlluminateLibrarian IlluminateLibrarian
        {
            get { return _illuminateLibrarian; }
            set { _illuminateLibrarian = value; }
        }


        protected override void Awake()
		{
            base.Awake();
        }

        protected void OnDisable()
		{
            // this resets any animation frames so we don't get stuck when deactivating mid-anim
         
           _manifestation.ResetAnimations();

            // we're turning? Just set us to the target
            if (turnCoroutine != null) {
                turnCoroutine = null;
                Flip(isFront, true); // instant to set where it wants to go
            }
        }




        public void SetQuantity(int quantity, Context context)
		{
			_quantity = quantity;
			if (quantity <= 0)
			{
			    if (context.actionSource == Context.ActionSource.Purge)
			        Retire(CardVFX.CardLight);
                else
				    Retire(CardVFX.CardBurn);
                return;
			}

			if (quantity > 1 && (Unique || !string.IsNullOrEmpty(UniquenessGroup)))
			{
				_quantity = 1;
			}
			_aspectsDirtyInc = true;
            if(!TokenContainer.ContentsHidden)
			    _manifestation.UpdateText(_element,quantity);

            TokenContainer.NotifyStacksChanged();
        }

        public void ModifyQuantity(int change,Context context) {
            SetQuantity(_quantity + change, context);
        }



        virtual public Dictionary<string, int> GetCurrentMutations()
        {
            return new Dictionary<string, int>(_currentMutations);
        }
        virtual public Dictionary<string, string> GetCurrentIlluminations()
        {
            return IlluminateLibrarian.GetCurrentIlluminations();
        }

        virtual public void SetMutation(string aspectId, int value,bool additive)
        {
            if (_currentMutations.ContainsKey(aspectId))
            {
                if (additive)
                    _currentMutations[aspectId] += value;
                else
					_currentMutations[aspectId] = value;

                if (_currentMutations[aspectId] == 0)
                    _currentMutations.Remove(aspectId);
            }
            else if (value != 0)
			{
				_currentMutations.Add(aspectId,value);
			}
			_aspectsDirtyExc = true;
			_aspectsDirtyInc = true;
        }




        virtual public Dictionary<string, List<MorphDetails>> GetXTriggers() {
            return _element.XTriggers;
        }

        virtual public IAspectsDictionary GetAspects(bool includeSelf = true)
        {
            //if we've somehow failed to populate an element, return empty aspects, just to exception-proof ourselves
    
            
            var tabletop = Registry.Get<TabletopManager>(false) as TabletopManager;

            if (_element == null || tabletop==null)
                return new AspectsDictionary();
            
			if (!tabletop._enableAspectCaching)
			{
				_aspectsDirtyInc = true;
				_aspectsDirtyExc = true;
			}

			if (includeSelf)
			{
				if (_aspectsDirtyInc)
				{
					if (_aspectsDictionaryInc==null)
						_aspectsDictionaryInc=new AspectsDictionary();
					else
						_aspectsDictionaryInc.Clear();	// constructor is expensive

					_aspectsDictionaryInc.CombineAspects(_element.AspectsIncludingSelf);
					_aspectsDictionaryInc[_element.Id] = _aspectsDictionaryInc[_element.Id] * Quantity; //This might be a stack. In this case, we always want to return the multiple of the aspect of the element itself (only).

					_aspectsDictionaryInc.ApplyMutations(_currentMutations);

					if (tabletop._enableAspectCaching)
						_aspectsDirtyInc = false;

					tabletop.NotifyAspectsDirty();
				}
				return _aspectsDictionaryInc;
			}
			else
			{
				if (_aspectsDirtyExc)
				{
					if (_aspectsDictionaryExc==null)
						_aspectsDictionaryExc=new AspectsDictionary();
					else
						_aspectsDictionaryExc.Clear();	// constructor is expensive

					_aspectsDictionaryExc.CombineAspects(_element.Aspects);

					_aspectsDictionaryExc.ApplyMutations(_currentMutations);

					if (tabletop._enableAspectCaching)
						_aspectsDirtyExc = false;

					tabletop.NotifyAspectsDirty();
				}
				return _aspectsDictionaryExc;
			}
        }

        virtual public List<SlotSpecification> GetChildSlotSpecificationsForVerb(string forVerb) {
            return _element.Slots.Where(cs=>cs.ActionId==forVerb || cs.ActionId==string.Empty).ToList();
        }

        virtual public bool HasChildSlotsForVerb(string verb) {
            return _element.HasChildSlotsForVerb(verb);
        }

        private void InitialiseIfStackIsNew()
        {
            //these things should only be initialised if we've just created the stack
            //if we're repopulating, they'll already exist

            if (_currentMutations == null)
                _currentMutations = new Dictionary<string, int>();
            if (_illuminateLibrarian == null)
                _illuminateLibrarian = new IlluminateLibrarian();
            
            //add any observers that we can find in the context
            var debugTools = Registry.Get<DebugTools>(false);
            if (debugTools != null)
                AddObserver(debugTools);

            _manifestation = TokenContainer.CreateManifestation(this);
        }


        /// <summary>
        /// This is uses both for population and for repopulation - eg when an xtrigger transforms a stack
        /// Note that it (intentionally) resets the timer.
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="quantity"></param>
        /// <param name="source"></param>
        public void Populate(string elementId, int quantity, Source source)
		{
            
            _element = Registry.Get<ICompendium>().GetEntityById<Element>(elementId);
            if (_element==null)
			{
				NoonUtility.Log("Trying to create nonexistent element! - '" + elementId + "'");
                this.Retire();
                return;
            }

            InitialiseIfStackIsNew();


            try
            {
                SetQuantity(quantity, new Context(Context.ActionSource.Unknown)); // this also toggles badge visibility through second call

                _manifestation.DisplayVisuals(_element);
                _manifestation.UpdateText(_element,quantity);
                
           
          
                ShowGlow(false, false);
                SetCardDecay(0f);
                LifetimeRemaining = _element.Lifetime;
                PlacementAlreadyChronicled = false; //element has changed, so we want to relog placement
                MarkedForConsumption = false; //If a stack has just been transformed into another element, all sins are forgiven. It won't be consumed.
			
				_aspectsDirtyExc = true;
				_aspectsDirtyInc = true;

                StackSource = source;

				#if DROPZONE
				// Refactored for safety. Custom class was crashing all over the shop because it didn't reliably inherit from ElementStackToken
				// and many card properties are just expected to be valid. Instead, we create a normal but useless card ("dropzone" in tools.json)
				// and just customise it's appearance.
				if (elementId == "dropzone")
				{
					CustomizeDropZone();
				}
#endif
            }
            catch (Exception e)
            {
                NoonUtility.Log("Couldn't create element with ID " + elementId + " - " + e.Message + "(This might be an element that no longer exists being referenced in a save file?)");
                Retire(CardVFX.None);
            }
        }








        public override void ReturnToTabletop(Context context)
		{
			bool stackBothSides = true;

            //if we have an origin stack and the origin stack is on the tabletop, merge it with that.
            //We might have changed the element that a stack is associated with... so check we can still merge it
            if (originStack != null && originStack.IsOnTabletop() && CanMergeWith(originStack))
			{
                originStack.MergeIntoStack(this);
                return;
            }
            else
			{
                var tabletop = Registry.Get<TabletopManager>()._tabletop;
                var existingStacks = tabletop.GetStacks();

				if (!_element.Unique)            // If we're not unique, auto-merge us!
				{
					//check if there's an existing stack of that type to merge with
					foreach (var stack in existingStacks)
					{
						if (CanMergeWith(stack))
						{
							var elementStack = stack as ElementStackToken;
							elementStack.MergeIntoStack(this);
							return;
						}
					}
				}
			
				if (lastTablePos == null)	// If we've never been on the tabletop, use the drop zone
				{
				#if DROPZONE
					// If we get here we have a new card that won't stack with anything else. Place it in the "in-tray"
					lastTablePos = GetDropZoneSpawnPos();
					stackBothSides = false;
				#endif
				}
			}

            Registry.Get<Choreographer>().ArrangeTokenOnTable(this, context, lastTablePos, false, stackBothSides);	// Never push other cards aside - CP
        }

		public Vector2 GetDropZoneSpawnPos()
		{
			var tabletop = Registry.Get<TabletopManager>()._tabletop;
			var existingStacks = tabletop.GetStacks();

			Vector2 spawnPos = Vector2.zero;
			AbstractToken	dropZoneObject = null;
			Vector3			dropZoneOffset = new Vector3(0f,0f,0f);

			foreach (var stack in existingStacks)
			{
				AbstractToken tok = stack as AbstractToken;
				if (tok!=null && tok.EntityId == "dropzone")
				{
					dropZoneObject = tok;
					break;
				}
			}
			if (dropZoneObject == null)
			{
				dropZoneObject = CreateDropZone();		// Create drop zone now and add to stacks
			}

			if (dropZoneObject != null)	// Position card near dropzone
			{
				spawnPos = Registry.Get<Choreographer>().GetTablePosForWorldPos(dropZoneObject.transform.position + dropZoneOffset);
			}
			
			return spawnPos;	
		}

		private AbstractToken CreateDropZone()
		{
			var tabletop = Registry.Get<TabletopManager>() as TabletopManager;

            var dropZone = tabletop._tabletop.ProvisionElementStack("dropzone", 1, Source.Fresh(),
                new Context(Context.ActionSource.Loading)) as ElementStackToken;

            dropZone.Populate("dropzone", 1, Source.Fresh());

            // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
            dropZone.transform.position = Vector3.zero;
            
            dropZone.transform.localScale = Vector3.one;
            return dropZone as AbstractToken;
		}

		private void CustomizeDropZone()		// Customises self!
		{
			// Customize appearance of card to make it distinctive
			// First hide normal card elements
			Transform oldcard = transform.Find( "Card" );
			Transform oldglow = transform.Find( "Glow" );
			Transform oldshadow = transform.Find( "Shadow" );
			if (oldcard)
			{
				oldcard.gameObject.SetActive( false );
			}
			if (oldglow)
			{
				oldglow.gameObject.SetActive( false );
			}
			if (oldshadow)
			{
				oldshadow.gameObject.SetActive( false );
			}

			// Now create an instance of the dropzone prefab parented to this card
			// This way any unused references are still pointing at the original card data, so no risk of null refs.
			// It's a bit hacky, but it's now a live project so refactoring the entire codebase to make it safe is high-risk.
			TabletopManager tabletop = Registry.Get<TabletopManager>() as TabletopManager;

			GameObject zoneobj = GameObject.Instantiate( tabletop._dropZoneTemplate, transform );
			Transform newcard = zoneobj.transform.Find( "Card" );
			Transform newglow = zoneobj.transform.Find( "Glow" );
			Transform newshadow = zoneobj.transform.Find( "Shadow" );

			_manifestation.glowImage = newglow.gameObject.GetComponent<GraphicFader>() as GraphicFader;
			newglow.gameObject.SetActive( false );
            _manifestation.shadow = newshadow.gameObject;

			// Modify original card settings
			useDragOffset = true;	// It's huge and we can only grab it at the corner
			LayoutElement lay = GetComponent<LayoutElement>() as LayoutElement;
			if (lay)
			{
				lay.preferredWidth = 0f;	// Do not want this zone to interact with cards at all
				lay.preferredHeight = 0f;
			}
			NoPush = true;
		}

        private bool IsOnTabletop() {
            return transform.parent.GetComponent<TabletopTokenContainer>() != null;
        }

        public void MergeIntoStack(ElementStackToken merge) {
            SetQuantity(Quantity + merge.Quantity,new Context(Context.ActionSource.Merge));
            merge.Retire(CardVFX.None);
        }



        // Called from TokenContainer, usually after StacksManager told it to
        public override void SetTokenContainer(ITokenContainer newTokenContainer, Context context) {
            OldTokenContainer = TokenContainer;

            if (OldTokenContainer != null && OldTokenContainer != newTokenContainer)
            {
                OldTokenContainer.SignalStackRemoved(this, context);
                if(OldTokenContainer.ContentsHidden && !newTokenContainer.ContentsHidden)
                 _manifestation.UpdateText(_element,Quantity);
            }

            TokenContainer = newTokenContainer;

            if (newTokenContainer != null)
                newTokenContainer.SignalStackAdded(this, context);

      

        }

        protected override void NotifyChroniclerPlacedOnTabletop()
        {
            Registry.Get<Chronicler>()?.TokenPlacedOnTabletop(this);
        }

        public override bool Retire()
		{
            		if (Defunct)
				return false;

            var hlc = Registry.Get<HighlightLocationsController>(false);
            if(hlc!=null)
                hlc.DeactivateMatchingHighlightLocation(_element?.Id);

            var tabletop = Registry.Get<TabletopManager>(false);
            if(tabletop!=null)
			    tabletop.NotifyAspectsDirty();	// Notify tabletop that aspects will need recompiling
            
            SetTokenContainer(null, new Context(Context.ActionSource.Retire)); // notify the view container that we're no longer here

            //now take care of the Unity side of things.

            Defunct = true;
            FinishDrag(); // Make sure we have the drag aborted in case we're retiring mid-drag (merging stack frex)

            return _manifestation.Retire(canvasGroup);
        }


        public bool Retire(CardVFX vfxName)
        {

            _manifestation.SetVfx(vfxName);

            return Retire();
        }


        protected override bool AllowsDrag() {
            return isFront && turnCoroutine == null; // no dragging while not front or busy turning
        }

        protected override bool ShouldShowHoverGlow() {
            // interaction is always possible on facedown cards to turn them back up
            return !isFront || base.ShouldShowHoverGlow();
        }

        virtual public bool AllowsIncomingMerge() {
            if (Decays || _element.Unique || IsBeingAnimated || IsInAir || TokenContainer.GetType()==typeof(RecipeSlot))
                return false;
            else
                return TokenContainer.AllowStackMerge;
        }

        virtual public bool AllowsOutgoingMerge() {
            if (Decays || _element.Unique || IsBeingAnimated)
                return false;
            else
                return true;	// If outgoing, it doesn't matter what its current container is - CP
        }
        

		private List<TabletopUi.TokenAndSlot> FindValidSlot( IList<RecipeSlot> slots, TabletopUi.SituationController situation )
		{
			List<TabletopUi.TokenAndSlot> results = new List<TabletopUi.TokenAndSlot>();

			foreach (RecipeSlot slot in slots)
			{
				if (!slot.IsGreedy &&
					slot.GetTokenInSlot() == null &&
					slot.GetSlotMatchForStack(this).MatchType == SlotMatchForAspectsType.Okay)
				{
					// Create token/slot pair
					var tokenSlotPair = new TabletopUi.TokenAndSlot()
					{
						Token = situation.situationAnchor as VerbAnchor,
						RecipeSlot = slot as RecipeSlot
					};

					results.Add( tokenSlotPair );
				}
			}
			return results;
		}

		private void SendStackToNearestValidSlot()
		{
			if (TabletopManager.IsInMansus())	// Prevent SendTo while in Mansus
				return;
			if (!(TokenContainer.GetType()==typeof(TabletopTokenContainer)))
				return;

			// Compile list of valid slots
			List<TabletopUi.TokenAndSlot> targetSlots = new List<TabletopUi.TokenAndSlot>();
			var registeredSits = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
			foreach(TabletopUi.SituationController situ in registeredSits)
			{
				if (situ.Situation.State != SituationState.Complete)
				{
					if (situ.IsOngoing)
					{
						// Check for ongoing slots only
						var ongoingSlots = situ.GetOngoingSlots();
						targetSlots.AddRange( FindValidSlot( ongoingSlots, situ ) );
					}
					else if (!situ.situationAnchor.IsTransient && situ.Situation.State == SituationState.Unstarted)
					{
						// Look for starting slots (most common case)
						var startSlots = situ.situationWindow.GetStartingSlots();
						targetSlots.AddRange( FindValidSlot( startSlots, situ ) );
					}
				}
			}

			// Now find the best target from that list
			if (targetSlots.Count > 0)
			{
				TabletopUi.TokenAndSlot selectedSlot = null;
				float selectedSlotDist = float.MaxValue;

				// Find closest token to stack
				foreach (TabletopUi.TokenAndSlot tokenpair in targetSlots)
				{
					Vector3 dist = tokenpair.Token.transform.position - transform.position;
					//Debug.Log("Dist to " + tokenpair.Token.EntityId + " = " + dist.magnitude );
					if (tokenpair.Token.SituationController.IsOpen)
						dist = Vector3.zero;	// Prioritise open windows above all else
					if (dist.sqrMagnitude < selectedSlotDist)
					{
						selectedSlotDist = dist.sqrMagnitude;
						selectedSlot = tokenpair;
					}
				}

				if (selectedSlot != null && selectedSlot.RecipeSlot !=null)
				{
					if (selectedSlot.RecipeSlot.IsBeingAnimated)
					{
						//Debug.Log("Already sending something to " + selectedSlot.Token.EntityId);
					}
					else
					{
						//Debug.Log("Sending " + this.EntityId + " to " + selectedSlot.Token.EntityId);
						var choreo = Registry.Get<Choreographer>();
						SplitAllButNCardsToNewStack(1, new Context(Context.ActionSource.DoubleClickSend));
						choreo.PrepareElementForSendAnim( this, selectedSlot.Token ); // this reparents the card so it can animate properly
						choreo.MoveElementToSituationSlot( this, selectedSlot, choreo.ElementSendAnimDone,SEND_STACK_TO_SLOT_DURATION);
					}
				}
			}
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
            foreach (var o in observers)
            {
                o.OnStackPointerEntered(this, eventData);
            }

            base.OnPointerEnter(eventData);
			var tabletopManager = Registry.Get<TabletopManager>(false);
            if(tabletopManager!=null ) //eg we might have a face down card on the credits page - in the longer term, of course, this should get interfaced
            {
                if (isFront)
                    tabletopManager.SetHighlightedElement(EntityId, Quantity);
                else
                    tabletopManager.SetHighlightedElement(null);

                if (!eventData.dragging)
                { 
                    //Display any HighlightLocations tagged for this element, unless we're currently dragging something else
                    var hlc = Registry.Get<HighlightLocationsController>();
                    hlc.ActivateOnlyMatchingHighlightLocation(_element.Id);
                }
            }
        }

		public override void OnPointerExit(PointerEventData eventData)
		{
            foreach (var o in observers)
            {
                o.OnStackPointerExited(this, eventData);
            }

            base.OnPointerExit(eventData);
            var ttm = Registry.Get<TabletopManager>(false);
                if(ttm!=null)
                {
                Registry.Get<TabletopManager>().SetHighlightedElement(null);

                    //No longer display any HighlightLocations tagged for this element
                    if(!_currentlyBeingDragged)
                    { 
                        var hlc = Registry.Get<HighlightLocationsController>();
                        hlc.DeactivateMatchingHighlightLocation(_element.Id);
                    }
                }
        }

        public override void ReactToDraggedToken(TokenInteractionEventArgs args)
        {
            if(args.TokenInteractionType==TokenInteractionType.BeginDrag)
            {
                ElementStackToken stack = args.Token as ElementStackToken;
                ;
                if (Defunct || stack == null)
                    return;

                
                if (stack.CanMergeWith(this))
                {
                    SetGlowColor(UIStyle.TokenGlowColor.Default);
                    ShowGlow(true, false);
                }
            }
            else if (args.TokenInteractionType == TokenInteractionType.EndDrag)
            {
                ShowGlow(false,false);
            }
            
        }

        public override void ShowGlow(bool glowState, bool instant = false)
        {
            _manifestation.ShowGlow(glowState,instant);
    
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
    
            if (eventData.clickCount > 1)
			{
				// Double-click, so abort any pending single-clicks
				singleClickPending = false;
                foreach (var o in observers)
                {
                    o.OnStackDoubleClicked(this, eventData, this._element);
                }


                SendStackToNearestValidSlot();
			}
			else
			{
				// Single-click BUT might be first half of a double-click
				// Most of these functions are OK to fire instantly - just the ShowCardDetails we want to wait and confirm it's not a double
				singleClickPending = true;

    


                if (isFront)
				{
                    foreach (var o in observers)
                    {
                        o.OnStackClicked(this, eventData, this._element);
                    }

				}
				else
				{
					FlipToFaceUp(false);

					if (onTurnFaceUp != null)
						onTurnFaceUp(this);
				}

				// this moves the clicked sibling on top of any other nearby cards.
				if (TokenContainer.GetType() != typeof(RecipeSlot) && TokenContainer.GetType()!=typeof(ExhibitCards) )
					transform.SetAsLastSibling();
			}
        }

        public override void OnDrop(PointerEventData eventData) {
            foreach (var o in observers)
            {
                o.OnStackDropped(this, eventData);
            }

            InteractWithTokenDroppedOn(eventData.pointerDrag);

        }



        public bool CanMergeWith(ElementStackToken intoStack)
		{
            return	intoStack.EntityId == this.EntityId &&
					(intoStack as ElementStackToken) != this &&
					intoStack.AllowsIncomingMerge() &&
					this.AllowsOutgoingMerge() &&
					intoStack.GetCurrentMutations().IsEquivalentTo(GetCurrentMutations());
        }

        public override bool CanInteractWithTokenDroppedOn(ElementStackToken stackDroppedOn)
        {
            //element dropped on element
            return CanMergeWith(stackDroppedOn);
        }

        public override void InteractWithTokenDroppedOn(ElementStackToken stackDroppedOn) {
            //element dropped on element
            if (CanInteractWithTokenDroppedOn(stackDroppedOn)) {
                stackDroppedOn.SetQuantity(stackDroppedOn.Quantity + this.Quantity,new Context(Context.ActionSource.Unknown));
                SetReturn(false, "was merged");
                SoundManager.PlaySfx("CardPutOnStack");

                var token = stackDroppedOn as AbstractToken;

                if (token != null) // make sure the glow is done in case we highlighted this
                    token.ShowGlow(false, true);

                this.Retire(CardVFX.None);
            }
            else {
                ShowNoMergeMessage(stackDroppedOn);

                var droppedOnToken = stackDroppedOn as AbstractToken;
                bool moveAsideFor = false;
                droppedOnToken.TokenContainer.TryMoveAsideFor(this, droppedOnToken, out moveAsideFor);

                if (moveAsideFor)
                    SetReturn(false, "was moved aside for");
            }
        }

        public override bool CanInteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn)
        {
            //verb dropped on element - FIXED
            return false; // a verb anchor can't be dropped on anything
        }

        public override void InteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn)
        {
            //Verb dropped on element - FIXED
            
            this.TokenContainer.TryMoveAsideFor(this, tokenDroppedOn, out bool  moveAsideFor);

            if (moveAsideFor)
                SetReturn(false, "was moved aside for");
            else
                SetReturn(true);
        }



        void ShowNoMergeMessage(ElementStackToken stackDroppedOn) {
            if (stackDroppedOn.EntityId != this.EntityId)
                return; // We're dropping on a different element? No message needed.

            if (stackDroppedOn.Decays)
			{
                Registry.Get<Notifier>().ShowNotificationWindow(Registry.Get<ILocStringProvider>().Get("UI_CANTMERGE"), Registry.Get<ILocStringProvider>().Get("UI_DECAYS"), false);
            }
        }

        public ElementStackToken SplitAllButNCardsToNewStack(int n, Context context) {
            if (Quantity > n)
            {
                var cardLeftBehind =
                    TokenContainer.ProvisionElementStack(EntityId, Quantity - n, Source.Existing(), context) as ElementStackToken;
                foreach (var m in GetCurrentMutations())
	                cardLeftBehind.SetMutation(m.Key, m.Value, false); //brand new mutation, never needs to be additive

                originStack = cardLeftBehind;

                //goes weird when we pick things up from a slot. Do we need to refactor to Accept/Gateway in order to fix?
                SetQuantity(n,context);

                // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
                cardLeftBehind.transform.position = transform.position;
                
                // Accepting stack may put it to pos Vector3.zero, so this is last
                cardLeftBehind.transform.position = transform.position;
                return cardLeftBehind;
            }

            return null;
        }

        protected override void StartDrag(PointerEventData eventData)
        {
            _currentlyBeingDragged = true;

            IsInAir = true; // This makes sure we don't consider it when checking for overlap
            ShowCardShadow(true); // Ensure we always have a shadow when dragging

            // A bit hacky, but it works: DID NOT start dragging from badge? Split cards
			// Now also allowing both shift keys to drag entire stack - CP
            if (_manifestation.stackBadge != null &&
                _manifestation.stackBadge.IsHovering() == false &&
                !Keyboard.current.shiftKey.wasPressedThisFrame)
			{
                SplitAllButNCardsToNewStack(1, new Context(Context.ActionSource.PlayerDrag));
			}
            base.StartDrag(eventData); // To ensure all events fire at the end
        }

      


        public void Decay(float interval) {
            //passing a negative interval overrides and ensures it'll always decay
            if (!Decays && interval>=0)
			    return;
            

			var stackAnim = this.gameObject.GetComponent<TokenAnimationToSlot>();
			if (stackAnim)
			{
				return;	// Do not decay while being dragged into greedy slot (#1335) - CP
			}


            LifetimeRemaining = LifetimeRemaining - interval;

            if (LifetimeRemaining <= 0 || interval<0) {
                // We're dragging this thing? Then return it?
                if (_currentlyBeingDragged) {
                    // Set our table pos based on our current world pos
                    lastTablePos = Registry.Get<Choreographer>().GetTablePosForWorldPos(transform.position);
                    // Then cancel our drag, which will return us to our new pos
                    FinishDrag();
                }

                // If we DecayTo, then do that. Otherwise straight up retire the card
                if (string.IsNullOrEmpty(_element.DecayTo))
                    Retire(CardVFX.CardBurn);
                else
                    DecayTo(_element.DecayTo);
            }

            if (!isFront)
                FlipToFaceUp(true); //never leave a decaying card face down.


		    _manifestation.UpdateDecayVisuals(LifetimeRemaining,_element,interval,_currentlyBeingDragged );

          if (onDecay != null)
                onDecay(LifetimeRemaining);
        }

		



 

        public void ShowCardShadow(bool show)
		{
            _manifestation.shadow.gameObject.SetActive(show);
        }
        


        public bool DecayTo(string elementId)
		{
            // Save this, since we're retiring and that sets quantity to 0
            int quantity = Quantity;

           var cardLeftBehind= TokenContainer.ProvisionElementStack(elementId, quantity, Source.Existing(),
                new Context(Context.ActionSource.ChangeTo)) as ElementStackToken;

            foreach(var m in this.GetCurrentMutations())
               cardLeftBehind.SetMutation(m.Key,m.Value,false); //brand new mutation, never needs to be additive
            cardLeftBehind.lastTablePos = lastTablePos;
            cardLeftBehind.originStack = null;

            // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
            cardLeftBehind.transform.position = transform.position;

            // Put it behind the card being burned
            cardLeftBehind.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);

            // Accepting stack may put it to pos Vector3.zero, so this is last
            cardLeftBehind.transform.position = transform.position;

            Retire(CardVFX.CardTransformWhite);

            return true;
        }

        public void Understate()
        {
            canvasGroup.alpha = 0.3f;
        }

        public void Emphasise()
        {
            canvasGroup.alpha = 1f;
        }


        public void FlipToFaceUp(bool instant = false)
        {
            if (!instant)
                SoundManager.PlaySfx("CardTurnOver");

            Flip(true, instant);
        }

        public void FlipToFaceDown(bool instant = false) {
            Flip(false, instant);
        }

        virtual public void Flip(bool state, bool instant = false) {
            if (isFront == state && !instant) // if we're instant, ignore this to allow forcing of pos
                return;

            ShowGlow(!state); // disable face-down hover-effect

            isFront = state;
            //if a card has just been turned face up in a situation, it's now an existing, established card
            if (isFront && StackSource.SourceType == SourceType.Fresh)
                StackSource = Source.Existing();

            if (gameObject.activeInHierarchy == false || instant) {
                transform.localRotation = GetFrontRotation(isFront);
                return;
            }

            if (turnCoroutine != null)
                StopCoroutine(turnCoroutine);

            turnCoroutine = StartCoroutine(DoTurn());
        }

        Quaternion GetFrontRotation(bool isFront) {
            return Quaternion.Euler(0f, isFront ? 0f : 180f, 0f);
        }

        public bool IsFront() {
            return isFront;
        }

        IEnumerator DoTurn() {
            float time = 0f;
            float targetAngle = isFront ? 0f : 180f;
            float currentAngle = transform.localEulerAngles.y;
            float duration = Mathf.Abs(targetAngle - currentAngle) / 900f;

            while (time < duration) {
                time += Time.deltaTime;
                transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(currentAngle, targetAngle, time / duration), 0f);
                yield return null;
            }

            transform.localRotation = Quaternion.Euler(0f, targetAngle, 0f);
            turnCoroutine = null;
        }

 


        public override bool CanAnimate()
        {
            if (gameObject == null)
                return false;

            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated

            if (_element == null)
                return false;

            return frames.Any();
        }

        /// <summary>
        /// Trigger an animation on the card
        /// </summary>
        /// <param name="duration">Determines how long the animation runs. Time is spent equally on all frames</param>
        /// <param name="frameCount">How many frames to show. Default is 1</param>
        /// <param name="frameIndex">At which frame to start. Default is 0</param>
        public override void StartArtAnimation() {
            if (!CanAnimate())
                return;
            _manifestation.BeginArtAnimation();
           
        }

      

    }
}
