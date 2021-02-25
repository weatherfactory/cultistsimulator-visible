using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories.UI;

namespace SecretHistories.Entities
{
    
    public class MinimalPayload : ITokenPayload
    {
        public string Id { get; protected set; }
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

        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;


        public MinimalPayload(string id)
        {
            Id = id;
        }
        
        
        public bool IsOpen { get; }
        public FucinePath AbsolutePath { get; }
        public List<Dominion> Dominions { get; }
        public Type GetManifestationType(SphereCategory sphereCategory)
        {
            return typeof(MinimalManifestation);
        }


        public void InitialiseManifestation(IManifestation manifestation)
        {
          //
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public void ExecuteHeartbeat(float interval)
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
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
            throw new NotImplementedException();
        }
    }
}
