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
     
     
        public Sphere GetSphereById(string id);
        public List<Sphere> GetSpheresByCategory(SphereCategory category);

        Type GetManifestationType(SphereCategory sphereCategory);
        void InitialiseManifestation(IManifestation manifestation);
        bool IsValid();
        bool IsValidElementStack();
        void FirstHeartbeat();
        void ExecuteHeartbeat(float seconds, float metaseconds);
        bool CanInteractWith(ITokenPayload incomingTokenPayload);
        bool CanMergeWith(ITokenPayload incomingTokenPayload);
        void InteractWithIncoming(Token incomingToken);
        bool ReceiveNote(INotification notification,Context context);
        void ShowNoMergeMessage(ITokenPayload incomingTokenPayload);
        void SetQuantity(int quantityToLeaveBehind, Context context);
        void ModifyQuantity(int unsatisfiedChange, Context context);

        void ExecuteTokenEffectCommand(IAffectsTokenCommand command);

        void OpenAt(TokenLocation location);
        void Close();
        void Conclude();

        bool ApplyExoticEffect(ExoticEffect exoticEffect);

        void SetToken(Token token);
        void OnTokenMoved(TokenLocation toLocation);
        void StorePopulateDominionCommand(PopulateDominionCommand populateDominionCommand);
    }
    }

