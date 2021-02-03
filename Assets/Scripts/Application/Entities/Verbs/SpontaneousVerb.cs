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
   public class SpontaneousVerb: IVerb
    {
        

        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;


        public SpontaneousVerb(string id, string label, string description)
        {
            Id = id;
            Label = label;
            Description = description;
            Thresholds=new List<SphereSpec>();

        }

        public  bool Spontaneous => true;

        public string Icon=>String.Empty;


        public Dictionary<string, int> Mutations { get; }

        
        public string Id { get; private set; }

        public void SetId(string id)
        {
            Id = id;
        }

        public string Label { get; set; }

        public string Description { get; set; }

        public List<SphereSpec> Thresholds { get; set; }

        public bool ExclusiveOpen => true;


        public AspectsDictionary GetAspects(bool includeSelf)
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

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
          //
        }
    }
}
