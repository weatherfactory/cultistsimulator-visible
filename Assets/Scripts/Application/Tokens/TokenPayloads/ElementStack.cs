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
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Elements;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Events;
using SecretHistories.Logic;
using SecretHistories;
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
            var absolutePath = pathAbove.AppendingToken(this.Id);
            return absolutePath;
        }

        public FucinePath GetWildPath()
        {
            var wildPath = FucinePath.Wild();
            
           return wildPath.AppendingToken(this.Id);
        }

        public RectTransform GetRectTransform()
        {
            return Token.TokenRectTransform;

        }

        [Encaust] public bool Defunct { get; protected set; }

        [Encaust] public float LifetimeRemaining => _timeshadow.LifetimeRemaining;

        [Encaust] public virtual int Quantity => Defunct ? 0 : _quantity;

        [Encaust] public virtual Dictionary<string, int> Mutations => new Dictionary<string, int>(_mutations);

        [Encaust]
        public virtual Dictionary<string,string> Illuminations=>new Dictionary<string, string>(_illuminations);


        [Encaust]
        public List<AbstractDominion> Dominions => new List<AbstractDominion>(_dominions);

        


        protected Element Element { get; set; }
        [DontEncaust] virtual public string Label => Element.Label;
        [DontEncaust] virtual public string Description => Element.Description;
        [DontEncaust] virtual public string Icon => Element.Icon;
        [DontEncaust] virtual public bool Unique => Element.Unique;
        [DontEncaust] virtual public string UniquenessGroup => Element.UniquenessGroup;
        [DontEncaust] virtual public bool Decays => Element.Decays;
        [DontEncaust] public bool IsOpen => false;

        [DontEncaust] public bool Metafictional => Element.Metafictional;

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
        private List<AbstractDominion> _dominions=new List<AbstractDominion>();

        public virtual bool IsValid()
        {
            return true;
        }

        public virtual bool IsValidElementStack()
        {
            if (Defunct)
                return false;

            return Element.Id != NullElement.NULL_ELEMENT_ID;
        }

        public bool IsPermanent()
        {
            return false;
        }

        public void FirstHeartbeat()
        {
            ExecuteHeartbeat(0f, 0f);
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


        public bool RegisterDominion(AbstractDominion dominionToRegister)
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

        public List<Sphere> GetSpheresByCategory(SphereCategory category)
        {
            return new List<Sphere>(_spheres.Where(c => c.SphereCategory == category && !c.Defunct));
        }

        public Type GetManifestationType(Sphere sphere)
        {
            if (sphere.SphereCategory == SphereCategory.SituationStorage)
                return typeof(StoredManifestation);

            if (sphere.SphereCategory == SphereCategory.Dormant)
                return typeof(MinimalManifestation);

            if (sphere.SphereCategory == SphereCategory.Notes)
                return typeof(TextManifestation);

            if (sphere.SphereCategory == SphereCategory.Meta)
                return typeof(NullManifestation);

            var type = Watchman.LocateManifestationType(Element.ManifestationType);
            return type;

            //return typeof(CardManifestation);
        }

        public void InitialiseManifestation(IManifestation _manifestation)
        {
            _manifestation.Initialise(this);
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
                  
                    _aspectsDictionaryInc.MultiplyByQuantity(Quantity);
                    //_aspectsDictionaryInc[Element.Id] =
                      //  _aspectsDictionaryInc[Element.Id] *
                       // Quantity; //This might be a stack. In this case, we always want to return the multiple of the aspect of the element itself (only).

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
                    _aspectsDictionaryExc.MultiplyByQuantity(Quantity);


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

  

        public bool ReceiveNote(INotification notification, Context context)
        {
            SetIllumination(NoonConstants.TLG_NOTES_TITLE_KEY, notification.Title);
            SetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY, notification.Description);
            SetIllumination(NoonConstants.TLG_NOTES_EMPHASISLEVEL_KEY, notification.EmphasisLevel.ToString());
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update, context));
            
            return true;
        }


        public bool Retire()
        {
            return Retire(RetirementVFX.CardBurn);
        }


        public bool Retire(RetirementVFX VFX)
        {

            if (Defunct)
                return false;
            Defunct = true;
            TokenPayloadChangedArgs args= new TokenPayloadChangedArgs(this, PayloadChangeType.Retirement);
            args.VFX = VFX;
            OnChanged?.Invoke(args);

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
            if (Element.Unique)
                return false;

            if (Decays && otherPayload.GetTimeshadow().Transient)
                if(Math.Abs(otherPayload.GetTimeshadow().LifetimeRemaining - GetTimeshadow().LifetimeRemaining) > 0.1f)
                    return false;
            if (!Token.Sphere.AllowStackMerge)
                return false;

            if (otherPayload.EntityId != this.EntityId)
                return false;
            if (otherPayload == this)
                return false;

            if (!otherPayload.Mutations.IsEquivalentTo(Mutations))
                return false;

            return true;
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
        /// <param name="seconds"></param>
        /// <param name="metaseconds"></param>
        /// <returns></returns>
        public void ExecuteHeartbeat(float seconds, float metaseconds)
        {
            if (seconds <= 0 || !Decays)
                return;

            _timeshadow.SpendTime(seconds);
            
            OnLifetimeSpent?.Invoke(LifetimeRemaining); //display decay effects for listeners elsewhere

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update));

            if (LifetimeRemaining <= 0 || seconds < 0)
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
        if(Watchman.Exists<Notifier>())
                Watchman.Get<Notifier>().ShowCardElementDetails(Element, this);
        }

        public void Close()
        {
            //
        }

        public void Conclude()
        {
            Close();
        }

        public bool ApplyExoticEffect(ExoticEffect exoticEffect)
        {
            if (exoticEffect is ExoticEffect.Purge)
            {
                if (string.IsNullOrEmpty(Element.DecayTo))
                {
                     Retire(RetirementVFX.CardLight);
                }
                else
                    ChangeTo(Element.DecayTo);

                return true;
            }


            else  if(exoticEffect is ExoticEffect.BurnPurge)
            {
                if(!string.IsNullOrEmpty(Element.BurnTo))
                    ChangeTo(Element.BurnTo);
                else if (!string.IsNullOrEmpty(Element.DecayTo))
                    ChangeTo(Element.DecayTo);
                else
                    Retire(RetirementVFX.CardBurn);

                return true;

            }
            //As a placeholder and token of intent. Relevant for BoH obviously, and in another life I'd have added it
            //as a Rite outcome.
            else if(exoticEffect is ExoticEffect.DrownPurge)
            {
                if (!string.IsNullOrEmpty(Element.DrownTo))
                    ChangeTo(Element.BurnTo);
                else if (!string.IsNullOrEmpty(Element.DecayTo))
                    ChangeTo(Element.DecayTo);
                else
                    Retire(RetirementVFX.CardDrown);

                return true;

            }

            else
            {
                
            }
            return false;

        }

        public void SetToken(Token token)
        {
            _token = token;
        }
        public void OnTokenMoved(TokenLocation toLocation)
        {
            //
        }

        public void StorePopulateDominionCommand(PopulateDominionCommand populateDominionCommand)
        {
            throw new ApplicationException($"No provision for storing a populate dominion command on an elementstack, but we can't find dominion with identifier {populateDominionCommand.Identifier} on elementstack {Id}");
        }


        public void ChangeTo(string newElementId)
        {
            var newElement = Watchman.Get<Compendium>().GetEntityById<Element>(newElementId);
            _timeshadow = new Timeshadow(newElement.Lifetime, newElement.Lifetime, newElement.Resaturate);
            Element = newElement;
            _aspectsDirtyExc = true;
            _aspectsDirtyInc = true;
            _token.gameObject.name = newElement.Id + "_token";

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Fundamental));
        }

        public void OnSphereChanged(SphereChangedArgs args)
        {
            //
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
