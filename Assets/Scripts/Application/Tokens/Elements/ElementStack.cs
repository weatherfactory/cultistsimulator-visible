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
using SecretHistories.Abstract;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using SecretHistories.UI;
using SecretHistories.Utilities.Exetensions;
using UnityEngine.InputSystem;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace SecretHistories.UI {


    [IsEncaustableClass(typeof(ElementStackCreationCommand))]
    public class ElementStack: ITokenPayload
    {
        public event Action<float> onDecay;
        public event Action OnChanged;

        [Encaust]
        public string Id => Element.Id;

        [Encaust]
        public bool Defunct { get; protected set; }

        [Encaust]
        public float LifetimeRemaining { get; set; }

        [Encaust]
        public virtual int Quantity => Defunct ? 0 : _quantity;

        [Encaust]
        public virtual bool MarkedForConsumption { get; set; }

        [Encaust]
        public virtual Dictionary<string, int> Mutations=>new Dictionary<string, int>(_currentMutations);

        [Encaust]
        public IlluminateLibrarian IlluminateLibrarian
        {
            get { return _illuminateLibrarian; }
            set { _illuminateLibrarian = value; }
        }

        protected  Element Element { get; set; }
        [DontEncaust]
        virtual public string Label => Element.Label;
        [DontEncaust]
        virtual public string Description => Element.Description;
        [DontEncaust]
        virtual public string Icon => Element.Icon;
        [DontEncaust] 
        virtual public bool Unique => Element.Unique;
        [DontEncaust]
        virtual public string UniquenessGroup =>Element.UniquenessGroup;
        [DontEncaust]
        virtual public bool Decays => Element.Decays;

        [DontEncaust] public virtual float Lifetime => Element.Lifetime;
        [DontEncaust] public virtual bool Resaturate => Element.Resaturate;


        [DontEncaust]
        public float IntervalForLastHeartbeat { get; set; }
        [DontEncaust]
        public string EntityWithMutationsId
        {
            // Generate a unique ID for a combination of entity ID and mutations
            // IDs will look something like: entity_id?mutation_1=2&mutation_2=-1
            get
            {
                var mutations = Mutations;
                return Element.Id + "?" + string.Join(
                    "&",
                    mutations.Keys
                        .Where(m => mutations[m] != 0)
                        .OrderBy(x => x)
                        .Select(m => $"{m}={mutations[m]}"));
            }
        }


        private IElementStackHost _attachedToken;
        private int _quantity;
        // Cache aspect lists because they are EXPENSIVE to calculate repeatedly every frame - CP
        private IAspectsDictionary
            _aspectsDictionaryInc = new AspectsDictionary(); // For caching aspects including self 
        private IAspectsDictionary
            _aspectsDictionaryExc = new AspectsDictionary(); // For caching aspects excluding self
        private bool _aspectsDirtyInc = true;
        private bool _aspectsDirtyExc = true;
        private Dictionary<string, int>
            _currentMutations =
                new Dictionary<string, int>(); //not strictly an aspects dictionary; it can contain negatives
        private IlluminateLibrarian _illuminateLibrarian;
        
        public bool IsValidElementStack()
        {
            return Element.Id != NullElement.NULL_ELEMENT_ID;
        }

        public bool IsValidVerb()
        {
            return false;
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

            _attachedToken.onElementStackQuantityChanged(this,context);
            
        }

        public void ModifyQuantity(int change,Context context) {
            SetQuantity(_quantity + change, context);
        }


        public ElementStack()
        {
            Element=new NullElement();
            _attachedToken = new NullToken();
            SetQuantity(1,new Context(Context.ActionSource.Unknown));
        }

        public ElementStack(Element element,int quantity,float lifetimeRemaining, Context context)
        {
            Element = element;
            LifetimeRemaining = lifetimeRemaining;
            _attachedToken = new NullToken();
            SetQuantity(quantity,context);

            _aspectsDirtyExc = true;
            _aspectsDirtyInc = true;
        }



        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            if (forSphereCategory == SphereCategory.SituationStorage)
                return typeof(StoredManifestation);

            if (forSphereCategory == SphereCategory.Dormant)
                return typeof(MinimalManifestation);

            return typeof(CardManifestation);
        }

        public void InitialiseManifestation(IManifestation _manifestation)
        {
            _manifestation.InitialiseVisuals(Element);
            _manifestation.UpdateVisuals(this);
            _manifestation.UpdateTimerVisuals(Element.Lifetime, LifetimeRemaining, 0f, Element.Resaturate,
                EndingFlavour.None);
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

        public virtual IAspectsDictionary GetAspects(bool includeSelf = true)
        {
            //if we've somehow failed to populate an element, return empty aspects, just to exception-proof ourselves
    
            
            var tc = Watchman.Get<SphereCatalogue>();

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

        virtual public List<SphereSpec> GetChildSlotSpecificationsForVerb(string forVerb) {
            return Element.Slots.Where(cs=>cs.ActionId==forVerb || cs.ActionId==string.Empty).ToList();
        }

        virtual public bool HasChildSlotsForVerb(string verb) {
            return Element.HasChildSlotsForVerb(verb);
        }

        public void AttachToken(Token token)
        {
            if (_attachedToken != null)
                _attachedToken.Retire(RetirementVFX.None);
            _attachedToken = token;
            _attachedToken.Populate(this);
        }



        public void AcceptIncomingPayloadForMerge(ITokenPayload stackMergedIntoThisOne) {
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

    
        public virtual bool CanMergeWith(ITokenPayload intoStack)
		{
            if(intoStack.Id != this.Id)
                return false;
            if (intoStack == this)
                return false;
            if (!intoStack.AllowsIncomingMerge())
                return false;
            if (!this.AllowsOutgoingMerge())
                return false;
            if(!intoStack.Mutations.IsEquivalentTo(Mutations))
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



        public void ShowNoMergeMessage(ITokenPayload stackDroppedOn) {
            if (stackDroppedOn.Id != this.Element.Id)
                return; // We're dropping on a different element? No message needed.

            if (stackDroppedOn.Decays)
			{
                Watchman.Get<Notifier>().ShowNotificationWindow(Watchman.Get<ILocStringProvider>().Get("UI_CANTMERGE"), Watchman.Get<ILocStringProvider>().Get("UI_DECAYS"), false);
            }
        }

        /// <summary>
        /// passing a negative interval will immediately decay the stack
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public void Decay(float interval) {
            
            if (!Decays && interval>=0)
			    return;

            onDecay?.Invoke(LifetimeRemaining); //display decay effects for listeners elsewhere

            LifetimeRemaining = LifetimeRemaining - interval;

            if (LifetimeRemaining <= 0 || interval<0) {

                // If we DecayTo, then do that. Otherwise straight up retire the card
                if (string.IsNullOrEmpty(Element.DecayTo))
                {
                    Retire(RetirementVFX.CardBurn);
                }
                else
                    ChangeTo(Element.DecayTo);
            }


        }

        public void ExecuteTokenEffectCommand(ITokenEffectCommand command)
        {
            command.ExecuteOn(this);
        }
        


        public void ChangeTo(string newElementId)
        {
            var newElement = Watchman.Get<Compendium>().GetEntityById<Element>(newElementId);
            LifetimeRemaining = newElement.Lifetime;
            MarkedForConsumption = false; //If a stack has just been transformed into another element, all sins are forgiven. It won't be consumed.

            OnChanged?.Invoke();
        }







    }
}
