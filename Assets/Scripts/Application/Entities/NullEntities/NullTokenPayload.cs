using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Logic;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;
using SecretHistories.Interfaces;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullTokenPayload: ITokenPayload
    {
        public event Action OnChanged;
        public string Id => string.Empty;
        public int Quantity => 0;
        public Dictionary<string, int> Mutations { get; }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }

        public string Label => "";
        public string Description => "";


        public Type GetManifestationType(SphereCategory sphereCategory)
        {
            return typeof(NullManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            //
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsValidVerb()
        {
            return false;
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
            throw new NotImplementedException();
        }


        public void Decay(float interval)
        {
            return;
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

        public void ExecuteTokenEffectCommand(ITokenEffectCommand command)
        {
          //
        }
    }
}
