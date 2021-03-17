#pragma warning disable 0649

using System;
using System.Collections.Generic;
using SecretHistories.Core;
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
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Elements;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using SecretHistories.Utilities.Exetensions;
using Steamworks;
using UnityEngine.InputSystem;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace SecretHistories.UI {


    [IsEncaustableClass(typeof(ElementStackCreationCommand))]
    public class ElementStack : ITokenPayload,ISphereEventSubscriber
    {
        public event Action<float> OnLifetimeSpent;
        public event Action<TokenPayloadChangedArgs> OnChanged;

        [Encaust] public string Id { get; private set; }
    
        [Encaust] public string EntityId => Element.Id;
        public FucinePath GetAbsolutePath()
        {
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendToken(this.Id);
            return absolutePath;
        }

        [Encaust] public bool Defunct { get; protected set; }

        [Encaust] public float LifetimeRemaining => _timeshadow.LifetimeRemaining;

        [Encaust] public virtual int Quantity => Defunct ? 0 : _quantity;

        [Encaust] public virtual Dictionary<string, int> Mutations => new Dictionary<string, int>(_mutations);

        [Encaust]
        public virtual Dictionary<string,string> Illuminations=>new Dictionary<string, string>(_illuminations);


        [Encaust]
        public List<IDominion> Dominions => new List<IDominion>(_dominions);


        protected Element Element { get; set; }
        [DontEncaust] virtual public string Label => Element.Label;
        [DontEncaust] virtual public string Description => Element.Description;
        [DontEncaust] virtual public string Icon => Element.Icon;
        [DontEncaust] virtual public bool Unique => Element.Unique;
        [DontEncaust] virtual public string UniquenessGroup => Element.UniquenessGroup;
        [DontEncaust] virtual public bool Decays => Element.Decays;
        [DontEncaust] public bool IsOpen => false;
        

        private Timeshadow _timeshadow;
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();


        public string GetIllumination(string key)
        {
            if (_illuminations.ContainsKey(key))
                return _illuminations[key];
            return null;
        }

        public void SetIllumination(string key, string value)
        {
            if(_illuminations.ContainsKey(key))
                _illuminations[key] = value;
            else
                _illuminations.Add(key,value);
        }

        public Timeshadow GetTimeshadow()
        {
            return _timeshadow;
        }

        public Sphere GetEnRouteSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsValid() && !Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsEmpty())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.EnRouteSpherePath);

            return Token.Sphere.GetContainer().GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsValid() && !Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsEmpty())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.WindowsSpherePath);

            return Token.Sphere.GetContainer().GetWindowsSphere();
        }

        public string GetSignature()
        {
            // Generate a distinctive signature for a combination of entity ID and mutations
            // signatures will look something like: entity_id?mutation_1=2&mutation_2=-1
            
            var mutations = Mutations;
                string signature= "elementStack_" + Element.Id + "?" + string.Join(
                    "&",
                    mutations.Keys
                        .Where(m => mutations[m] != 0)
                        .OrderBy(x => x)
                        .Select(m => $"{m}={mutations[m]}"));

                return signature;
        }
        [DontEncaust]
        public Token Token
        {
            get
            {
                {
                    if (_token == null)
                        return NullToken.Create();
                    return _token;
                }
            }
        }

        public void AttachSphere(Sphere sphere)
        {
            sphere.Subscribe(this);
            sphere.SetContainer(this);
            _spheres.Add(sphere);
        }

        public void DetachSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _spheres.Remove(c);
        }

        private int _quantity;

        // Cache aspect lists because they are EXPENSIVE to calculate repeatedly every frame - CP
        private AspectsDictionary
            _aspectsDictionaryInc = new AspectsDictionary(); // For caching aspects including self 

        private AspectsDictionary
            _aspectsDictionaryExc = new AspectsDictionary(); // For caching aspects excluding self

        private bool _aspectsDirtyInc = true;
        private bool _aspectsDirtyExc = true;


        private readonly Dictionary<string, int>
            _mutations =
                new Dictionary<string, int>(); //not strictly an aspects dictionary; it can contain negatives
        
        private readonly Dictionary<string,string> _illuminations=new Dictionary<string, string>();
        private Token _token;
        private List<IDominion> _dominions=new List<IDominion>();

        public bool IsValidElementStack()
        {
            if (Defunct)
                return false;

            return Element.Id != NullElement.NULL_ELEMENT_ID;
        }

        public void FirstHeartbeat()
        {
            ExecuteHeartbeat(0f);
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
                    Retire(RetirementVFX.CardBurn);
            }

            if (quantity > 1 && (Unique || !string.IsNullOrEmpty(UniquenessGroup)))
            {
                _quantity = 1;
            }

            _aspectsDirtyInc = true;
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Update,context));

        }

        public void ModifyQuantity(int change, Context context)
        {
            SetQuantity(_quantity + change, context);
        }


        public ElementStack():this(NullElement.Create().DefaultUniqueTokenId(),NullElement.Create(),1,Timeshadow.CreateTimelessShadow(), Context.Unknown())
        {}


        public ElementStack(string id, Element element, int quantity, Timeshadow timeshadow, Context context)
        {
            Id = id;
            Element = element;
            SetQuantity(quantity, context);

            _aspectsDirtyExc = true;
            _aspectsDirtyInc = true;
            _timeshadow = timeshadow;

        }


        public bool RegisterDominion(IDominion dominionToRegister)
        {
            dominionToRegister.OnSphereAdded.AddListener(AttachSphere);
            dominionToRegister.OnSphereRemoved.AddListener(DetachSphere);

            if (_dominions.Contains(dominionToRegister))
                return false;

            _dominions.Add(dominionToRegister);
            return true;
        }

        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == Id && !s.Defunct);
        }

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            if (forSphereCategory == SphereCategory.SituationStorage)
                return typeof(StoredManifestation);

            if (forSphereCategory == SphereCategory.Dormant)
                return typeof(MinimalManifestation);

            if (forSphereCategory == SphereCategory.Notes)
                return typeof(TextManifestation);

            if (forSphereCategory == SphereCategory.Meta)
                return typeof(NullManifestation);

            var type = Watchman.LocateManifestationType(Element.ManifestationType);
            return type;

            //return typeof(CardManifestation);
        }

        public void InitialiseManifestation(IManifestation _manifestation)
        {
            _manifestation.InitialiseVisuals(this);
            _manifestation.UpdateVisuals(this);
        }

        virtual public void SetMutation(string aspectId, int value, bool additive)
        {
            if (_mutations.ContainsKey(aspectId))
            {
                if (additive)
                    _mutations[aspectId] += value;
                else
                    _mutations[aspectId] = value;

                if (_mutations[aspectId] == 0)
                    _mutations.Remove(aspectId);
            }
            else if (value != 0)
            {
                _mutations.Add(aspectId, value);
            }

            _aspectsDirtyExc = true;
            _aspectsDirtyInc = true;
        }

  


        virtual public Dictionary<string, List<MorphDetails>> GetXTriggers()
        {
            return Element.XTriggers;
        }

        public virtual AspectsDictionary GetAspects(bool includeSelf = true)
        {
            //if we've somehow failed to populate an element, return empty aspects, just to exception-proof ourselves


            var tc = Watchman.Get<HornedAxe>();

            if (Element == null || tc == null)
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
                    if (_aspectsDictionaryInc == null)
                        _aspectsDictionaryInc = new AspectsDictionary();
                    else
                        _aspectsDictionaryInc.Clear(); // constructor is expensive

                    _aspectsDictionaryInc.CombineAspects(Element.AspectsIncludingSelf);
                    _aspectsDictionaryInc[Element.Id] =
                        _aspectsDictionaryInc[Element.Id] *
                        Quantity; //This might be a stack. In this case, we always want to return the multiple of the aspect of the element itself (only).

                    _aspectsDictionaryInc.ApplyMutations(_mutations);

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
                    if (_aspectsDictionaryExc == null)
                        _aspectsDictionaryExc = new AspectsDictionary();
                    else
                        _aspectsDictionaryExc.Clear(); // constructor is expensive

                    _aspectsDictionaryExc.CombineAspects(Element.Aspects);

                    _aspectsDictionaryExc.ApplyMutations(_mutations);

                    if (tc.EnableAspectCaching)
                        _aspectsDirtyExc = false;

                    tc.NotifyAspectsDirty();
                }

                return _aspectsDictionaryExc;
            }
        }





        public void InteractWithIncoming(Token incomingToken)
        {
            SetQuantity(Quantity + incomingToken.Quantity, new Context(Context.ActionSource.Merge));
            incomingToken.Retire(RetirementVFX.None);

            SoundManager.PlaySfx("CardPutOnStack");

        }

        public bool ReceiveNote(string label, string description, Context context)
        {
            SetIllumination(NoonConstants.TLG_NOTES_TITLE_KEY, label);
            SetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY, description);
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update, context));
            
            return true;
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
            TokenPayloadChangedArgs args= new TokenPayloadChangedArgs(this, PayloadChangeType.Retirement);
            args.VFX = RetirementVFX.CardBurn;
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Retirement));

            return true;

        }


        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            return CanMergeWith(incomingTokenPayload);
        }

        public virtual bool CanMergeWith(ITokenPayload otherPayload)
        {
            if (!otherPayload.IsValidElementStack())
                return false;
            if (Decays || Element.Unique)
                return false;

            if (otherPayload.EntityId != this.EntityId)
                return false;
            if (otherPayload == this)
                return false;
            if (!this.AllowsOutgoingMerge())
                return false;
            if (!otherPayload.Mutations.IsEquivalentTo(Mutations))
                return false;

            return true;
        }


        public virtual bool AllowsOutgoingMerge()
        {
            if (Decays || Element.Unique)
                return false;
            else
                return true; // If outgoing, it doesn't matter what its current container is - CP
        }



        public void ShowNoMergeMessage(ITokenPayload stackDroppedOn)
        {
            if (stackDroppedOn.EntityId != EntityId)
                return; // We're dropping on a different element? No message needed.

            if (stackDroppedOn.GetTimeshadow().Transient)
            {
                Watchman.Get<Notifier>().ShowNotificationWindow(Watchman.Get<ILocStringProvider>().Get("UI_CANTMERGE"),
                    Watchman.Get<ILocStringProvider>().Get("UI_DECAYS"), false);
            }
        }

        /// <summary>
        /// passing a negative interval will immediately decay the stack
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public void ExecuteHeartbeat(float interval)
        {
            if (interval <= 0 || !Decays)
                return;

            _timeshadow.SpendTime(interval);
            
            OnLifetimeSpent?.Invoke(LifetimeRemaining); //display decay effects for listeners elsewhere


            if (LifetimeRemaining <= 0 || interval < 0)
            {

                // If we DecayTo, then do that. Otherwise straight up retire the card
                if (string.IsNullOrEmpty(Element.DecayTo))
                {
                    Retire(RetirementVFX.CardBurn);
                }
                else
                    ChangeTo(Element.DecayTo);
            }


        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
            command.ExecuteOn(this);
        }

        public void OpenAt(TokenLocation location)
        {
  
            Watchman.Get<Notifier>().ShowCardElementDetails(Element, this);
        }

        public void Close()
        {
            //
        }

        public void SetToken(Token token)
        {
            _token = token;
        }
        public void OnTokenMoved(TokenLocation toLocation)
        {
            //
        }


        public void ChangeTo(string newElementId)
        {
            var newElement = Watchman.Get<Compendium>().GetEntityById<Element>(newElementId);
            _timeshadow = new Timeshadow(newElement.Lifetime, newElement.Lifetime, newElement.Resaturate);

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Fundamental));
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            //
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
           //
        }
    }
}
