using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Tokens.TokenPayloads
{
    [IsEncaustableClass(typeof(IngressCreationCommand))]
    public  class Ingress: ITokenPayload, ISphereEventSubscriber
    {
        
        private Token _token;
        private Portal _portal { get; set; }


        [Encaust]
        public string Id { get; private set; }
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
        [DontEncaust]
        public bool IsOpen { get; protected set; }
        [Encaust]
        public string EntityId => _portal.Id;
        [Encaust]
        public string Label { get; set; }
        [Encaust]
        public string Description { get; set; }
        [Encaust]
        public int Quantity { get; }
        [DontEncaust]
        public string UniquenessGroup { get; }
        [DontEncaust]
        public bool Unique { get; }
        [DontEncaust]
        public string Icon => _portal.Icon;
        [DontEncaust]
        public bool Defunct
        { get; set; }
        [Encaust]
        public List<AbstractDominion> Dominions => new List<AbstractDominion>(_registeredDominions);

        [Encaust]
        public Dictionary<string, int> Mutations => _mutations;

        public void SetMutation(string aspectId, int value, bool additive)
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

        }


        private List<AbstractDominion> _registeredDominions=new List<AbstractDominion>();
        private List<Sphere> _spheres=new List<Sphere>();
        private readonly Dictionary<string, int> _mutations=new Dictionary<string, int>();

        public Ingress(Portal portal)
        {
            _portal = portal;
            int identity = FucineRoot.Get().IncrementedIdentity();
            Id = $"!{portal.Id}_{identity}";
            Label = _portal.Label;
            Description = _portal.Description;
        }


        public string GetOtherworldId()
        {
            return _portal.OtherworldId;
        }

        public string GetEgressId()
        {
            return _portal.EgressId;
        }

        public List<LinkedRecipeDetails> GetConsequences()
        {
            return new List<LinkedRecipeDetails>(_portal.Consequences);
        }

        public FucinePath GetAbsolutePath()
        {
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendToken(this.Id);
            return absolutePath;
        }

        public RectTransform GetRectTransform()
        {
            return Token.TokenRectTransform;
        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            throw new NotImplementedException();
        }

        

        public string GetSignature()
        {
            throw new NotImplementedException();
        }

        public Sphere GetEnRouteSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsValid())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.EnRouteSpherePath);

            return Token.Sphere.GetContainer().GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsValid())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.WindowsSpherePath);

            return Token.Sphere.GetContainer().GetWindowsSphere();
        }

        public void AttachSphere(Sphere sphere)
        {
            sphere.SetContainer(this);
            sphere.Subscribe(this);
            _spheres.Add(sphere);
            
        }

        public void DetachSphere(Sphere sphere)
        {
            _spheres.Remove(sphere);

        }


        public string GetIllumination(string key)
        {
            throw new NotImplementedException();
        }

        public void SetIllumination(string key, string value)
        {
            throw new NotImplementedException();
        }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }

        public bool RegisterDominion(AbstractDominion dominionToRegister)
        {
            dominionToRegister.OnSphereAdded.AddListener(AttachSphere);
            dominionToRegister.OnSphereRemoved.AddListener(DetachSphere);

            if (_registeredDominions.Contains(dominionToRegister))
                return false;

            _registeredDominions.Add(dominionToRegister);
               return true;
        }

        public Sphere GetEgressOutputSphere()
        {
            return _spheres.Single();
        }

        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        
        public Sphere GetSphereById(string id)
        {
            throw new NotImplementedException();
        }

        public Type GetManifestationType(SphereCategory sphereCategory)
        {
            return typeof(PortalManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.Initialise(this);

        }

        public bool IsValid()
        {
            return true;
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public void FirstHeartbeat()
        {
            OpenAt(Token.Location);
        }

        public void ExecuteHeartbeat(float interval)
        {
            //
        }


        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public bool Retire(RetirementVFX vfx)
        {
            if (Defunct)
                return false;

            Defunct = true; //don't want an infinite loop if there are any tokens in the output sphere

            foreach (var s in _spheres)
                s.Retire(SphereRetirementType.Graceful);


            TokenPayloadChangedArgs args = new TokenPayloadChangedArgs(this, PayloadChangeType.Retirement);
            args.VFX = RetirementVFX.None;
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Retirement));

            return true;
        }


        public void InteractWithIncoming(Token incomingToken)
        {
            throw new NotImplementedException();
        }

        public bool ReceiveNote(string label, string description, Context context)
        {
            throw new NotImplementedException();
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            throw new NotImplementedException();
        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            throw new NotImplementedException();
        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            throw new NotImplementedException();
        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
            throw new NotImplementedException();
        }

        public void OpenAt(TokenLocation location)
        {
            //note: we don't actually use the passed location. We always assume, inside the window, that we just centre on the portal.
            this.IsOpen = true;
            Watchman.Get<Numa>().Open(this.GetRectTransform(),this);
     
            
        
        }

        public void Close()
        {
            this.Retire(RetirementVFX.None);
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
            throw new NotImplementedException();
        }

        private void TryDisplayDrawMessage(SphereContentsChangedEventArgs args)
        {
            var potentialMessage =
                args.TokenAdded.Payload.GetIllumination(NoonConstants.MESSAGE_ILLUMINATION_KEY);
            if (!string.IsNullOrEmpty(potentialMessage))
            {
               this.Description= potentialMessage;
                args.TokenAdded.Payload.SetIllumination(NoonConstants.MESSAGE_ILLUMINATION_KEY, string.Empty);
            }
            OnChanged.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Update));
        }

        /// <summary>
        /// NB: this will fire for *any* sphere that's been attached, because we subscribe to all spheres which are attached.
        /// Of course at the moment there's only one sphere - the egressoutput - but bear it in mind pls
        /// </summary>
        /// <param name="args"></param>
        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            if (args.TokenAdded != null)
                TryDisplayDrawMessage(args);
            if (args.TokenRemoved != null)
            {
                if (args.Sphere.Tokens.Count == 0)
                    Retire(RetirementVFX.None);
            }

        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }
    }
}
