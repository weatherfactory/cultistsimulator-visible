using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Abstract
    {
    public interface ITokenPayload: IEncaustable,IDrivesManifestation
    {
        public event Action OnChanged ;
        public string Id { get; }
        public int Quantity { get; }
        Dictionary<string, int> Mutations { get; }
        Type GetManifestationType(SphereCategory sphereCategory);
        void InitialiseManifestation(IManifestation manifestation);
        bool IsValidElementStack();
        bool IsValidVerb();
        public string Label { get; }
        public string Description { get; }
        string UniquenessGroup { get; }
        bool Unique { get; }
        IAspectsDictionary GetAspects(bool includeSelf);
        void ExecuteHeartbeat(float interval);
        bool CanMergeWith(ITokenPayload incomingTokenPayload);
        bool Retire(RetirementVFX vfx);
        void AcceptIncomingPayloadForMerge(ITokenPayload incomingTokenPayload);
        void ShowNoMergeMessage(ITokenPayload incomingTokenPayload);
        void SetQuantity(int quantityToLeaveBehind, Context context);
        void ModifyQuantity(int unsatisfiedChange, Context context);
        void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive);
        void ExecuteTokenEffectCommand(ITokenEffectCommand command);

        
    }
    }

