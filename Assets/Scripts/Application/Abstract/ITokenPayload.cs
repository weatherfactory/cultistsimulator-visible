using System;
using System.Collections.Generic;
using Assets.Scripts.Application.Infrastructure.Events;

using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Abstract
    {
    public interface ITokenPayload: IEncaustable,IManifestable
    {
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
     

        
        public List<IDominion> Dominions { get; }
        public Sphere GetSphereById(string id);
        
        Type GetManifestationType(SphereCategory sphereCategory);
        void InitialiseManifestation(IManifestation manifestation);
        bool IsValid();
        bool IsValidElementStack();
        void FirstHeartbeat();
        void ExecuteHeartbeat(float interval);
        bool CanInteractWith(ITokenPayload incomingTokenPayload);
        bool CanMergeWith(ITokenPayload incomingTokenPayload);
        bool Retire(RetirementVFX vfx);
        void InteractWithIncoming(Token incomingToken);
        bool ReceiveNote(string label, string description,Context context);
        void ShowNoMergeMessage(ITokenPayload incomingTokenPayload);
        void SetQuantity(int quantityToLeaveBehind, Context context);
        void ModifyQuantity(int unsatisfiedChange, Context context);

        void ExecuteTokenEffectCommand(IAffectsTokenCommand command);

        void OpenAt(TokenLocation location);
        void Close();

        void SetToken(Token token);
        void OnTokenMoved(TokenLocation toLocation);
        void StorePopulateDominionCommand(PopulateDominionCommand populateDominionCommand);
    }
    }

