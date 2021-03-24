using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Tokens.TokenPayloads
{
    [IsEncaustableClass(typeof(DropzoneCreationCommand))]
    public  class Portal: ITokenPayload
    {
        
        private Token _token;

        [DontEncaust]
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

        public bool IsOpen { get; protected set; }
        public string EntityId { get; }
        public string Label { get; }
        public string Description { get; }
        public int Quantity { get; }
        public string UniquenessGroup { get; }
        public bool Unique { get; }
        public string Icon { get; }

        private readonly string _otherworldId;

        public Portal(string portalId, string otherworldId)
        {
            _otherworldId = otherworldId;
            int identity = FucineRoot.Get().IncrementedIdentity();
            Id = $"!{portalId}_{otherworldId}_{identity}";
            EntityId = portalId;
            Icon = portalId;
        }

    public FucinePath GetAbsolutePath()
        {
            throw new NotImplementedException();
        }

        public RectTransform GetRectTransform()
        {
            return Token.TokenRectTransform;
        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, int> Mutations { get; }
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
            throw new NotImplementedException();
        }

        public string GetSignature()
        {
            throw new NotImplementedException();
        }

        public Sphere GetEnRouteSphere()
        {
            throw new NotImplementedException();
        }

        public Sphere GetWindowsSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsValid())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.WindowsSpherePath);

            return Token.Sphere.GetContainer().GetWindowsSphere();
        }

        public void AttachSphere(Sphere sphere)
        {
            throw new NotImplementedException();
        }

        public void DetachSphere(Sphere sphere)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool RegisterDominion(IDominion dominion)
        {
            throw new NotImplementedException();
        }

        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        public List<IDominion> Dominions { get; }
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
            manifestation.InitialiseVisuals(this);

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
            throw new NotImplementedException();
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            throw new NotImplementedException();
        }

        public bool Retire(RetirementVFX vfx)
        {
            throw new NotImplementedException();
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
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Fundamental, Context.Unknown()));
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void SetToken(Token token)
        {
            _token = token;
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
            throw new NotImplementedException();
        }

        public void StorePopulateDominionCommand(PopulateDominionCommand populateDominionCommand)
        {
            throw new NotImplementedException();
        }
    }
}
