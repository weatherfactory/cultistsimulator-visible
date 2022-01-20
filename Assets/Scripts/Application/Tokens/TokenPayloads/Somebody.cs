using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads
{
    [IsEncaustableClass(typeof(SomebodyCreationCommand))]
    public class Somebody: ITokenPayload
    {

        protected Element Element { get; set; }


        private readonly Dictionary<string, string> _illuminations = new Dictionary<string, string>();
        private Token _token;
        private List<AbstractDominion> _dominions = new List<AbstractDominion>();

        [Encaust]
        public string Id { get; protected set; }

        [Encaust] public string EntityId => Element.Id;
        [Encaust]
        public int Quantity { get; set; }

        [Encaust] public bool Defunct { get; protected set; }
        


        [Encaust] public virtual Dictionary<string, int> Mutations => new Dictionary<string, int>();

        [Encaust] public List<AbstractDominion> Dominions { get; }
        [Encaust]
        public virtual Dictionary<string, string> Illuminations => new Dictionary<string, string>(_illuminations);
        [DontEncaust]  public string Label => Element.Label;
        [DontEncaust]  public string Description => Element.Description;
        [DontEncaust]  public string Icon => Element.Icon;
        [DontEncaust]  public bool Unique => Element.Unique;
        [DontEncaust]  public string UniquenessGroup => Element.UniquenessGroup;
        [DontEncaust]  public bool Decays => Element.Decays;
        [DontEncaust] public bool IsOpen => false;

        
        [DontEncaust]
        public bool Metafictional { get; }
        public Somebody(string id, Element element)
        {
            Id = id;
            Element=element;
        }


        public FucinePath GetAbsolutePath()
        {
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendingToken(this.Id);
            return absolutePath;
        }

        public FucinePath GetWildPath()
        {
            var wildPath = FucinePath.Wild();

            return wildPath.AppendingToken(this.Id);
        }

        public RectTransform GetRectTransform()
        {
            return _token.TokenRectTransform;

        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            throw new NotImplementedException();
        }

   

        public string GetSignature()
        {
            return $"_somebody_{EntityId}";
        }

        public Sphere GetEnRouteSphere()
        {
            throw new NotImplementedException();
        }

        public Sphere GetWindowsSphere()
        {
            throw new NotImplementedException();
        }

        public void AttachSphere(Sphere sphere)
        {
            throw new NotImplementedException();
        }

        public void DetachSphere(Sphere sphere)
        {
            throw new NotImplementedException();
        }
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
            throw new NotImplementedException(); //we probably want to keep the mutations in internal element state
        }

        public string GetIllumination(string key)
        {
            if (_illuminations.ContainsKey(key))
                return _illuminations[key];
            return null;
        }

        public void SetIllumination(string key, string value)
        {
            if (_illuminations.ContainsKey(key))
                _illuminations[key] = value;
            else
                _illuminations.Add(key, value);
        }

        public Timeshadow GetTimeshadow()
        {
            return null;
        }

        public bool RegisterDominion(AbstractDominion dominion)
        {
            throw new NotImplementedException();
        }


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
            throw new NotImplementedException();
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
            return true;
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsPermanent()
        {
            return false;

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
