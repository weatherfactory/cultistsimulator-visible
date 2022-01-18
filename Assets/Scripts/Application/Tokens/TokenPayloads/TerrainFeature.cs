using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Abstract;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants.Events;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Events;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using Steamworks;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads
{
    [IsEncaustableClass(typeof(PopulateTerrainFeatureCommand))]
    public class TerrainFeature: MonoBehaviour, ITokenPayload,ISphereEventSubscriber
    {
        //A terrain feature is the IHasAspects equivalent of a permanent sphere:
        //the command does not set it up, but rather assumes it exists and populates it.
        //A terrain feature also initialises all its component, editor-built spheres at startup.
        //Does it need a Token? We'll see.
        private readonly HashSet<Sphere> _registeredSpheres = new HashSet<Sphere>();

        public List<Sphere> Spheres => new List<Sphere>(_registeredSpheres);

        void Awake()
        {
            //A terrain feature will set its own identifier at startup. This is true
            //whether or not we've loaded the contents.
            //TODO: make sure all identifiers are unique.
            Id = EditableIdentifier;

            //A terrain feature will likely have permanent sphere children.
            var sphereComponentsInChildren = gameObject.GetComponentsInChildren<Sphere>();
            foreach (var s in sphereComponentsInChildren)
            {
                var spec = s.GetComponent<PermanentSphereSpec>();
                spec.ApplySpecToSphere(s);
                AttachSphere(s);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public string EditableIdentifier;

        [Encaust]
        public string Id { get; protected set; }
        public FucinePath GetAbsolutePath()
        {
            var pathAbove = FucinePath.Root();
            var absolutePath = pathAbove.AppendingToken(this.Id);
            return absolutePath;
        }

        public FucinePath GetWildPath()
        {
            var wildCardPath = FucinePath.Wild();
            return wildCardPath.AppendingToken(this.Id);
        }

        public RectTransform GetRectTransform()
        {
            return gameObject.GetComponent<RectTransform>();

        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, int> Mutations { get; }
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
            throw new System.NotImplementedException();
        }

        public string GetSignature()
        {
            throw new System.NotImplementedException();
        }

        public Sphere GetEnRouteSphere()
        {
            return FucineRoot.Get().GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            return FucineRoot.Get().GetWindowsSphere();
        }

        public List<Sphere> GetSpheres()
        {
            return new List<Sphere>(_registeredSpheres);
        }

        public void AttachSphere(Sphere sphere)
        {
            sphere.Subscribe(this);
     
        }

        public void DetachSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _registeredSpheres.Remove(c);
        }

        public bool IsOpen { get; }
        public void OnSphereChanged(SphereChangedArgs args)
        {
            //
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            //
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
           //
        }


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
            return new List<Sphere>(_registeredSpheres.Where(s => s.SphereCategory == category));
        }

        public Type GetManifestationType(SphereCategory sphereCategory)
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
            return true;
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