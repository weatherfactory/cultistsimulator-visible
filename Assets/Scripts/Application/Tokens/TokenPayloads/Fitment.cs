using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Entities
{
    [IsEncaustableClass(typeof(FitmentCreationCommand))]
    public class Fitment : ITokenPayload
    {

        private Token _token;

        public string Id { get; }
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
            //this is more straightforward and less safe than similar implementations in Situation and ElementStack
            return _token.TokenRectTransform;
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
            throw new NotImplementedException();
        }

        public void AttachSphere(Sphere sphere)
        {
            throw new NotImplementedException();
        }

        public void DetachSphere(Sphere sphere)
        {
            throw new NotImplementedException();
        }

        public bool IsOpen { get; }
        public string EntityId { get; }
        public string Label { get; }
        public string Description { get; }
        public int Quantity { get; }
        public string UniquenessGroup { get; }
        public bool Unique { get; }
        public string Icon { get; }
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

        public bool RegisterDominion(AbstractDominion dominion)
        {
            throw new NotImplementedException();
        }

        public List<AbstractDominion> Dominions { get; }
        public bool Metafictional { get; }
        public bool Retire(RetirementVFX VFX)
        {
            throw new NotImplementedException();
        }

        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        public Sphere GetSphereById(string id)
        {
            throw new NotImplementedException();
        }

        public List<Sphere> GetSpheresByCategory(SphereCategory category)
        {
            throw new NotImplementedException();
        }

        public Type GetManifestationType(Sphere sphere)
        {
            throw new NotImplementedException();
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public bool IsValidElementStack()
        {
            throw new NotImplementedException();
        }

        public bool IsPermanent()
        {
            return false;
        }

        public void FirstHeartbeat()
        {
            throw new NotImplementedException();
        }

        public void ExecuteHeartbeat(float seconds, float metaseconds)
        {
            throw new NotImplementedException();
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            throw new NotImplementedException();
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            throw new NotImplementedException();
        }

        public void InteractWithIncoming(Token incomingToken)
        {
            throw new NotImplementedException();
        }

        public bool ReceiveNote(INotification notification, Context context)
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
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Conclude()
        {
            throw new NotImplementedException();
        }

        public bool ApplyExoticEffect(ExoticEffect exoticEffect)
        {
            throw new NotImplementedException();
        }

        public void SetToken(Token token)
        {
            throw new NotImplementedException();
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