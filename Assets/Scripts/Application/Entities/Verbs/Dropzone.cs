using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.Logic;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Services;

namespace SecretHistories.Entities.Verbs
{
    public class Dropzone: ITokenPayload
    {
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        public string Id { get; private set; }
        public int Quantity => 1;
        public Dictionary<string, int> Mutations { get; }

        public string Icon => string.Empty;
        public bool IsOpen => false;

        public string GetIllumination(string key)
        {
            return string.Empty;
        }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }

        public void SetId(string id)
        {
            Id = id;
        }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }
         
        [FucineValue]
        public string Art { get; set; }


        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(DropzoneManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsValidVerb()
        {
            return true;
        }

        public List<SphereSpec> Thresholds { get; set; }
    


        public bool Transient => false;
        public bool Startable => false;
        public bool ExclusiveOpen => false;

        public Dropzone()
        {
            Thresholds=new List<SphereSpec>();
        }

        public static Dropzone Create()
        {
            return new Dropzone();

        }

        public string UniquenessGroup => string.Empty;
        public bool Unique => false;
        public bool Decays => false;

        public IAspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public void ExecuteHeartbeat(float interval)
        {
            //
        }

        public ITokenPayload Decay(float interval)
        {
            return this;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            //
        }

        public bool Retire(RetirementVFX vfx)
        {
            return true;
        }

        public void AcceptIncomingPayloadForMerge(ITokenPayload incomingTokenPayload)
        {
            //
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            //
        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            //
        }

        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {//
        }

        public string GetSignature()
        {
            return Id;
        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
            //
        }

        public void OpenAt(TokenLocation location)
        {
            //
        }

        public void Close()
        {
            //
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
           //
        }
    }
}
