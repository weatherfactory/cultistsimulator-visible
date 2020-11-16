#pragma warning disable 0649

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
using System.ComponentModel;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Services;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.TokenContainers;
using Assets.TabletopUi.Scripts.UI;
using Noon;
using UnityEngine.InputSystem;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI {



    public class ElementStack: MonoBehaviour
    {
        public const float SEND_STACK_TO_SLOT_DURATION = 0.2f;

        public event System.Action<float> onDecay;
        public bool Defunct { get; protected set; }


        public Element Element;
        private Token _attachedToken;
        private int _quantity;

		// Cache aspect lists because they are EXPENSIVE to calculate repeatedly every frame - CP
		private IAspectsDictionary _aspectsDictionaryInc;		// For caching aspects including self 
		private IAspectsDictionary _aspectsDictionaryExc;		// For caching aspects excluding self
		private bool _aspectsDirtyInc = true;
		private bool _aspectsDirtyExc = true;

        public float LifetimeRemaining { get; set; }

        public Source StackSource { get; set; }

        
        private Dictionary<string,int> _currentMutations; //not strictly an aspects dictionary; it can contain negatives
        private IlluminateLibrarian _illuminateLibrarian;
    


        virtual public string Label
        {
            get { return Element == null ? null : Element.Label; }
        }

        virtual public string Icon
        {
            get { return Element == null ? null : Element.Icon; }
        }

        public string EntityWithMutationsId 
        {
	        // Generate a unique ID for a combination of entity ID and mutations
	        // IDs will look something like: entity_id?mutation_1=2&mutation_2=-1
	        get
	        {
		        var mutations = GetCurrentMutations();
		        return Element.Id + "?" + string.Join(
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
                if (Element == null)
                    return false;
                return Element.Unique;
            }
        }

        virtual public string UniquenessGroup
        {
            get { return Element == null ? null : Element.UniquenessGroup; }
        }

        virtual public bool Decays
		{
            get { return Element.Decays; }
        }

        virtual public int Quantity {
            get { return Defunct ? 0 : _quantity; }
        }


        virtual public bool MarkedForConsumption { get; set; }

        virtual public IlluminateLibrarian IlluminateLibrarian
        {
            get { return _illuminateLibrarian; }
            set { _illuminateLibrarian = value; }
        }

        public float IntervalForLastHeartbeat { get; set; }


        public bool IsValidElementStack()
        {
            return Element.Id != NullElement.NULL_ELEMENT_ID;
        }
        
        public void SetQuantity(int quantity, Context context)
		{
			_quantity = quantity;
			if (quantity <= 0)
			{
			    if (context.actionSource == Context.ActionSource.Purge)
			        Retire(RetirementVFX.CardLight);
                else
				    Retire(RetirementVFX.CardBurn);
                return;
			}

			if (quantity > 1 && (Unique || !string.IsNullOrEmpty(UniquenessGroup)))
			{
				_quantity = 1;
			}
			_aspectsDirtyInc = true;

            _attachedToken.ElementStackStateUpdated(this);
            
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
            return Element.XTriggers;
        }

        virtual public IAspectsDictionary GetAspects(bool includeSelf = true)
        {
            //if we've somehow failed to populate an element, return empty aspects, just to exception-proof ourselves
    
            
            var tc = Registry.Get<SphereCatalogue>();

            if (Element == null || tc==null)
                return new AspectsDictionary();
            
			if (!tc.EnableAspectCaching)
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

					_aspectsDictionaryInc.CombineAspects(Element.AspectsIncludingSelf);
					_aspectsDictionaryInc[Element.Id] = _aspectsDictionaryInc[Element.Id] * Quantity; //This might be a stack. In this case, we always want to return the multiple of the aspect of the element itself (only).

					_aspectsDictionaryInc.ApplyMutations(_currentMutations);

					if (tc.EnableAspectCaching)
						_aspectsDirtyInc = false;

					tc.NotifyAspectsDirty();
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

					_aspectsDictionaryExc.CombineAspects(Element.Aspects);

					_aspectsDictionaryExc.ApplyMutations(_currentMutations);

					if (tc.EnableAspectCaching)
						_aspectsDirtyExc = false;

					tc.NotifyAspectsDirty();
				}
				return _aspectsDictionaryExc;
			}
        }

        virtual public List<SlotSpecification> GetChildSlotSpecificationsForVerb(string forVerb) {
            return Element.Slots.Where(cs=>cs.ActionId==forVerb || cs.ActionId==string.Empty).ToList();
        }

        virtual public bool HasChildSlotsForVerb(string verb) {
            return Element.HasChildSlotsForVerb(verb);
        }

        public void AttachToken(Token token)
        {
            _attachedToken = token;
            _attachedToken.Populate(this);
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
            
            Element = Registry.Get<ICompendium>().GetEntityById<Element>(elementId);
            if (Element==null)
			{
				NoonUtility.Log("Trying to create nonexistent element! - '" + elementId + "'");
                this.Retire();
                return;
            }

            gameObject.name = Element.Id + "_stack";

            if (_currentMutations == null)
                _currentMutations = new Dictionary<string, int>();
            if (_illuminateLibrarian == null)
                _illuminateLibrarian = new IlluminateLibrarian();


            try
            {
                SetQuantity(quantity, new Context(Context.ActionSource.Unknown)); // this also toggles badge visibility through second call


                LifetimeRemaining = Element.Lifetime;

                MarkedForConsumption = false; //If a stack has just been transformed into another element, all sins are forgiven. It won't be consumed.
			
				_aspectsDirtyExc = true;
				_aspectsDirtyInc = true;

                StackSource = source;

            }
            catch (Exception e)
            {
                NoonUtility.Log("Couldn't create element with ID " + elementId + " - " + e.Message + "(This might be an element that no longer exists being referenced in a save file?)");
                Retire(RetirementVFX.None);
            }
        }




		public Vector2 GetDropZoneSpawnPos()
		{
			var tabletop = Registry.Get<TabletopManager>()._tabletop;
			var existingStacks = tabletop.GetElementTokens();

			Vector2 spawnPos = Vector2.zero;
			Token	dropZoneObject = null;
			Vector3			dropZoneOffset = new Vector3(0f,0f,0f);

			foreach (var stack in existingStacks)
			{
				Token tok = stack as Token;
				if (tok!=null && tok.Element == "dropzone")
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

		private Token CreateDropZone()
		{
			var tabletop = Registry.Get<TabletopManager>() as TabletopManager;

            var dropZone = tabletop._tabletop.ProvisionElementStackToken("dropzone", 1, Source.Fresh(),
                new Context(Context.ActionSource.Loading));

            dropZone.Populate("dropzone", 1, Source.Fresh());

            // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
            dropZone.transform.position = Vector3.zero;
            
            dropZone.transform.localScale = Vector3.one;
            return dropZone as Token;
		}



        public void AcceptIncomingStackForMerge(ElementStack stackMergedIntoThisOne) {
            SetQuantity(Quantity + stackMergedIntoThisOne.Quantity,new Context(Context.ActionSource.Merge));
            stackMergedIntoThisOne.Retire(RetirementVFX.None);

            SetXNess(TokenXNess.MergedIntoStack);
            SoundManager.PlaySfx("CardPutOnStack");

            _manifestation.Highlight(HighlightType.AttentionPls);
        }




        public bool Retire()
        {
            return Retire(RetirementVFX.CardBurn);
        }



        
        public bool Retire(RetirementVFX vfxName)
        {

            if (Defunct)
                return false;

            var hlc = Registry.Get<HighlightLocationsController>();
            if (hlc != null)
                hlc.DeactivateMatchingHighlightLocation(Element?.Id);

            Sphere.NotifyTokensChangedForContainer(new TokenEventArgs{Element = Element,Token = this,Container = Sphere});  // Notify tabletop that aspects will need recompiling
            SetSphere(Registry.Get<NullContainer>(), new Context(Context.ActionSource.Retire));


            Defunct = true;
            FinishDrag(); // Make sure we have the drag aborted in case we're retiring mid-drag (merging stack frex)

            
            _manifestation.Retire(vfxName,OnManifestationRetired);

            return true;

        }

        private void OnManifestationRetired()
        {

            Destroy(this.gameObject);
        }





        virtual public bool AllowsIncomingMerge() {
            if (Decays || Element.Unique || IsBeingAnimated)
                return false;
            else
                return Sphere.AllowStackMerge;
        }

        virtual public bool AllowsOutgoingMerge() {
            if (Decays || Element.Unique || IsBeingAnimated)
                return false;
            else
                return true;	// If outgoing, it doesn't matter what its current container is - CP
        }
        

		private void SendStackToNearestValidSlot()
		{
			if (TabletopManager.IsInMansus())	// Prevent SendTo while in Mansus
				return;
            var tabletopTokenContainer = Sphere as TabletopSphere;

            if(tabletopTokenContainer==null)
				return;


            Dictionary<Sphere, Situation> candidateThresholds = new Dictionary<Sphere, Situation>();
			var registeredSituations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
			foreach(Situation situation in registeredSituations)
            {
                try
                {

                    var candidateThreshold=situation.GetFirstAvailableThresholdForStackPush(this);
                    if(candidateThreshold!=null)
                        candidateThresholds.Add(candidateThreshold,situation);
                }
                catch (Exception e)
                {
                    NoonUtility.LogWarning("Problem adding a candidate threshold to list of valid thresholds - does a valid threshold belong to more than one situation? - "  + e.Message);
                }
            }

            if (candidateThresholds.Any())
            {
                Sphere selectedCandidate=null;
                float selectedSlotDist = float.MaxValue;

                foreach (Sphere candidate in candidateThresholds.Keys)
                {
                    Vector3 distance = candidateThresholds[candidate].GetAnchorLocation().Position - transform.position;
                    //Debug.Log("Dist to " + tokenpair.Token.EntityId + " = " + dist.magnitude );
                    if (!candidate.CurrentlyBlockedFor(BlockDirection.Inward))
                        distance = Vector3.zero;    // Prioritise open windows above all else
                    if (distance.sqrMagnitude < selectedSlotDist)
                    {
                        selectedSlotDist = distance.sqrMagnitude;
                        selectedCandidate = candidate;
                    }
                }

                if (selectedCandidate != null)
                {

                    var candidateAnchorLocation = candidateThresholds[selectedCandidate].GetAnchorLocation();
                    TryCreateNewStackWithSplitCardsToLeaveBehind(1, new Context(Context.ActionSource.DoubleClickSend));
                    tabletopTokenContainer.SendViaContainer.PrepareElementForSendAnim(this, candidateAnchorLocation); // this reparents the card so it can animate properly
                    tabletopTokenContainer.SendViaContainer.MoveElementToSituationSlot(this,candidateAnchorLocation, selectedCandidate, SEND_STACK_TO_SLOT_DURATION);

                }
            }

			// Now find the best target from that list
		

			
		}

        


        public void bool CanMergeWith(ElementStack intoStack)
		{
            if(intoStack.Element != this.Element)
                return false;
            if (intoStack == this)
                return false;
            if (!intoStack.AllowsIncomingMerge())
                return false;
            if (!this.AllowsOutgoingMerge())
                return false;
            if(!intoStack.GetCurrentMutations().IsEquivalentTo(GetCurrentMutations()))
                return false;

            return true;
        }



   

        public void ShowNoMergeMessage(ElementStack stackDroppedOn) {
            if (stackDroppedOn.Element.Id != this.Element.Id)
                return; // We're dropping on a different element? No message needed.

            if (stackDroppedOn.Decays)
			{
                Registry.Get<Notifier>().ShowNotificationWindow(Registry.Get<ILocStringProvider>().Get("UI_CANTMERGE"), Registry.Get<ILocStringProvider>().Get("UI_DECAYS"), false);
            }
        }

        public ElementStack TryCreateNewStackWithSplitCardsToLeaveBehind(int n, Context context) {
            if (Quantity > n)
            {
                var cardLeftBehind =
                    Sphere.ProvisionElementStackToken(Element.Id, Quantity - n, Source.Existing(), context) as ElementStack;
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
                if (string.IsNullOrEmpty(Element.DecayTo))
                    Retire(RetirementVFX.CardBurn);
                else
                    DecayTo(Element.DecayTo);
            }

            
          if (onDecay != null)
                onDecay(LifetimeRemaining);
        }

        
        


        public bool DecayTo(string elementId)
		{
            // Save this, since we're retiring and that sets quantity to 0
            int quantity = Quantity;

            try
            {

                var cardLeftBehind= Sphere.ProvisionElementStackToken(elementId, quantity, Source.Existing(),
                    new Context(Context.ActionSource.ChangeTo)) as ElementStack;

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

                Retire(RetirementVFX.CardTransformWhite);
            }
            catch (Exception e)
            {
                NoonUtility.Log($"Something bad happened when trying to turn the {EntityId} card on the desktop into a {elementId} card: " + e.Message,1,VerbosityLevel.Essential);
                return false;
            }

            return true;
        }







    }
}
