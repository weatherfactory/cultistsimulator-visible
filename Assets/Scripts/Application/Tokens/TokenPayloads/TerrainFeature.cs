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
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using Steamworks;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads
{
    [IsEncaustableClass(typeof(PopulateTerrainFeatureCommand))]
    public class TerrainFeature: ITokenPayload,ISphereEventSubscriber
    {
        //A terrain feature is the IHasAspects equivalent of a permanent sphere spec:
        //the command does not set it up, but rather assumes it exists and populates it.
        //A terrain feature also initialises all its component, editor-built spheres at startup.


        private readonly HashSet<Sphere> _registeredSpheres = new HashSet<Sphere>();
        private readonly HashSet<AbstractDominion> _registeredDominions = new HashSet<AbstractDominion>();
        private readonly Dictionary<string, int> _mutations=new Dictionary<string, int>();

        [DontEncaust]
        public string EntityId { get; }
        [DontEncaust]
        public string Label { get; }
        [DontEncaust]
        public string Description { get;}
        [Encaust]
        public int Quantity { get; set; }
        [DontEncaust]
        public string UniquenessGroup { get; }
        [DontEncaust]
        public bool Unique { get; }
        [DontEncaust]
        public string Icon { get; }

        [Encaust] public bool IsOpen { get; set; }

        [Encaust]
        public Dictionary<string, int> Mutations =>new Dictionary<string, int>(_mutations);
        [Encaust]
        public List<AbstractDominion> Dominions => new List<AbstractDominion>(_registeredDominions);

        private Token _token;
        

        [DontEncaust]
        public Token Token
        {
            get
            {
                {
                    if (_token == null)
                        return NullToken.Create();
                    return _token;
                }
            }
        }
        

 
        [Encaust]
        public string Id { get; protected set; }


        public TerrainFeature()
        {

        }
        public void SetId(string id)
        {
            Id = id;
        }
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
            return _token.TokenRectTransform;

        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            throw new System.NotImplementedException();
        }


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
            sphere.SetContainer(this);
            _registeredSpheres.Add(sphere);
            var args = new TokenPayloadChangedArgs(this, PayloadChangeType.Update);
            OnChanged?.Invoke(args);

        }

        public void DetachSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _registeredSpheres.Remove(c);
        }

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
            dominion.OnSphereAdded.AddListener(AttachSphere);
            dominion.OnSphereRemoved.AddListener(DetachSphere);
            _registeredDominions.Add(dominion);

            return true;
        }


        [DontEncaust]
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
            return typeof(MinimalManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
           //
        }

        public bool IsValid()
        {
            return true;
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsPermanent()
        {
            return true;
        }

        public void FirstHeartbeat()
        {
           //
        }

        public void ExecuteHeartbeat(float seconds, float metaseconds)
        {
            //
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
            _token = token;
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