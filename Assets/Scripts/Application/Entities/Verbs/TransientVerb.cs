using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.Logic;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Spheres;

namespace SecretHistories.Entities
{
   public class TransientVerb: IVerb
    {
        public TransientVerb()
        {
            Startable = false;
        }

        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;


        public TransientVerb(string id, string label, string description):this()
        {
            Id = id;
            Label = label;
            Description = description;
            Thresholds=new List<SphereSpec>();

        }

        public  bool Transient => true;

        public string Art=>String.Empty;


        public int Quantity => 0;
        public Dictionary<string, int> Mutations { get; }

        public Timeshadow GetTimeshadow()
        {
            //so a Transient Verb timeshadow is not actually Transient. which is confusing. Let's rename Transient Verb
            return Timeshadow.CreateTimelessShadow();
        }



        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
          return  typeof(VerbManifestation);
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


        
        public string Id { get; private set; }

        public void SetId(string id)
        {
            Id = id;
        }

        public string Label { get; set; }

        public string Description { get; set; }

        public List<SphereSpec> Thresholds { get; set; }

        public bool Startable { get; set; }
        public bool ExclusiveOpen => true;

        public string UniquenessGroup => string.Empty;
        public bool Unique => false;

        public IAspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public void ExecuteHeartbeat(float interval)
        {
            throw new NotImplementedException();
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
            return false;
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
        {
            throw new NotImplementedException();
        }

        public string GetSignature()
        {
            return Id;
        }

        public void ExecuteTokenEffectCommand(ITokenEffectCommand command)
        {
          //
        }
    }
}
