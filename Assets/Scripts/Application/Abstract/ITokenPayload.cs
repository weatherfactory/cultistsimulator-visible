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
    public interface ITokenPayload: IEncaustable
    {
        public event Action OnChanged ;
        public string Id { get; }
        public int Quantity { get; }
        Type GetManifestationType(SphereCategory sphereCategory);
        void InitialiseManifestation(IManifestation manifestation);
        bool IsValidElementStack();
        bool IsValidVerb();
        public string Label { get; }
        public string Description { get; }
        string UniquenessGroup { get; }
        bool Unique { get; }
        bool Decays { get; }
        IAspectsDictionary GetAspects(bool includeSelf);
        void Decay(float interval);
        bool CanMergeWith(ITokenPayload incomingTokenPayload);
        void SetQuantity(int quantityToLeaveBehind, Context context);
        bool Retire(RetirementVFX vfx);
        void AcceptIncomingPayloadForMerge(ITokenPayload incomingTokenPayload);
        void ShowNoMergeMessage(ITokenPayload incomingTokenPayload);
        void ModifyQuantity(int unsatisfiedChange, Context context);
        void ExecuteTokenEffectCommand(ITokenEffectCommand command);
    }
    }

