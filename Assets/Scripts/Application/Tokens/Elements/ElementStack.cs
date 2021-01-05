#pragma warning disable 0649

using System;
using System.Collections.Generic;
using SecretHistories.Core;
using SecretHistories.Interfaces;
using SecretHistories.UI.Scripts;
using SecretHistories.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using Assets.Logic;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Events;
using SecretHistories.TokenContainers;
using SecretHistories.UI;
using SecretHistories.Utilities.Exetensions;
using UnityEngine.InputSystem;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace SecretHistories.UI {



    public class ElementStack: MonoBehaviour
    {

        public event System.Action<float> onDecay;
        public bool Defunct { get; protected set; }


        public virtual Element Element { get; set; }

        private Token _attachedToken;
        private int _quantity;

		// Cache aspect lists because they are EXPENSIVE to calculate repeatedly every frame - CP
		private IAspectsDictionary _aspectsDictionaryInc=new AspectsDictionary();		// For caching aspects including self 
		private IAspectsDictionary _aspectsDictionaryExc = new AspectsDictionary();		// For caching aspects excluding self
		private bool _aspectsDirtyInc = true;
		private bool _aspectsDirtyExc = true;

        public float LifetimeRemaining { get; set; }

        public Source StackSource { get; set; }

        
        private Dictionary<string,int> _currentMutations=new Dictionary<string, int>(); //not strictly an aspects dictionary; it can contain negatives
        private IlluminateLibrarian _illuminateLibrarian;



        public void Awake()
        {
            var nullToken = new GameObject().AddComponent<NullToken>();
            AttachToken(nullToken);
        }


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

        public Type GetDefaultManifestationType()
        {
            return typeof(CardManifestation);
        }

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            if (forSphereCategory == SphereCategory.SituationStorage)
                return typeof(StoredManifestation);

            if (forSphereCategory == SphereCategory.Dormant)
                return typeof(MinimalManifestation);

            return typeof(CardManifestation);
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

            _attachedToken.OnElementStackStateChanged(this);
            
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
            if (_attachedToken != null)
                _attachedToken.Retire(RetirementVFX.None);
            gameObject.transform.SetParent(token.transform);
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
            
            Element = Registry.Get<Compendium>().GetEntityById<Element>(elementId);
            if (Element==null)
			{
                Element=new NullElement();
				NoonUtility.Log("Trying to create nonexistent element! - '" + elementId + "'");
            }

            gameObject.name = Element.Id + "_stack";

            if (_currentMutations == null)
                _currentMutations = new Dictionary<string, int>();
            if (_illuminateLibrarian == null)
                _illuminateLibrarian = new IlluminateLibrarian();


            try
            {
                SetQuantity(quantity, new Context(Context.ActionSource.Unknown));


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




		



        public void AcceptIncomingStackForMerge(ElementStack stackMergedIntoThisOne) {
            SetQuantity(Quantity + stackMergedIntoThisOne.Quantity,new Context(Context.ActionSource.Merge));
            stackMergedIntoThisOne.Retire(RetirementVFX.None);

            SoundManager.PlaySfx("CardPutOnStack");

        }

        
        public bool Retire()
        {
            return Retire(RetirementVFX.CardBurn);
        }

       
        public bool Retire(RetirementVFX vfxName)
        {

            if (Defunct)
                return false;
            Defunct = true;

            if(!_attachedToken.Equals(null) && !_attachedToken.Defunct)
                _attachedToken.Retire(vfxName); 

            return true;

        }


        
        
        public virtual bool CanMergeWith(ElementStack intoStack)
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




        virtual public bool AllowsIncomingMerge()
        {
            if (Decays || Element.Unique)
                return false;
            else
                return _attachedToken.Sphere.AllowStackMerge;
        }

        virtual public bool AllowsOutgoingMerge()
        {
            if (Decays || Element.Unique)
                return false;
            else
                return true;	// If outgoing, it doesn't matter what its current container is - CP
        }



        public void ShowNoMergeMessage(ElementStack stackDroppedOn) {
            if (stackDroppedOn.Element.Id != this.Element.Id)
                return; // We're dropping on a different element? No message needed.

            if (stackDroppedOn.Decays)
			{
                Registry.Get<Notifier>().ShowNotificationWindow(Registry.Get<ILocStringProvider>().Get("UI_CANTMERGE"), Registry.Get<ILocStringProvider>().Get("UI_DECAYS"), false);
            }
        }

        
        public void Decay(float interval) {
            //passing a negative interval overrides and ensures it'll always decay
            if (!Decays && interval>=0)
			    return;
            

			var stackAnim = this.gameObject.GetComponent<TokenTravellingToSlot>();
			if (stackAnim)
			{
				return;	// Do not decay while being dragged into greedy slot (#1335) - CP
			}


            LifetimeRemaining = LifetimeRemaining - interval;

            if (LifetimeRemaining <= 0 || interval<0) {

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

            Populate(elementId,quantity,Source.Existing());

            _attachedToken.Remanifest(RetirementVFX.CardTransformWhite);

            return true;
        }







    }
}
