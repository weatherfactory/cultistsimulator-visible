using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Fucine;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Logic;
using SecretHistories.Services;
using SecretHistories.Spheres;

namespace SecretHistories.Entities.Verbs
{
    [IsEncaustableClass(typeof(DropzoneCreationCommand))]
    public class Dropzone: ITokenPayload
    {
        private Token _token;
        private List<IDominion> _dominions=new List<IDominion>();
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        [DontEncaust]
        public string Id { get; private set; }

        public FucinePath GetAbsolutePath()
        {
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendToken(this.Id);
            return absolutePath;
        }

        [Encaust]
        public int Quantity => 1;
        [DontEncaust]
        public Dictionary<string, int> Mutations { get; }
        [DontEncaust]
        public string Icon => string.Empty;
        [DontEncaust]
        public bool IsOpen => false;

        [DontEncaust] public FucinePath AbsolutePath => new NullFucinePath();

        [Encaust]
        public List<IDominion> Dominions
        {
            get { return new List<IDominion>(_dominions); }
        }
        [DontEncaust] public string Label => "Dropzone";
        [DontEncaust] public string Description => "Description";
        [DontEncaust]
        public List<SphereSpec> Thresholds { get; set; }
        [DontEncaust]
        public string UniquenessGroup => string.Empty;
        [DontEncaust]
        public bool Unique => false;

        public string GetIllumination(string key)
        {
            return string.Empty;
        }

        public void SetIllumination(string key, string value)
        {
            //
        }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }


        public bool RegisterDominion(IDominion dominion)
        {
            if (_dominions.Contains(dominion))
                return false;
            _dominions.Add(dominion);
            return true;
        }

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(DropzoneManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public void FirstHeartbeat()
        {
            ExecuteHeartbeat(0f);
        }


        public Dropzone()
        {
            Thresholds=new List<SphereSpec>();
        }

        public static Dropzone Create()
        {
            return new Dropzone();

        }


        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public void ExecuteHeartbeat(float interval)
        {
            //
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public ITokenPayload Decay(float interval)
        {
            return this;
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
            return true;
        }

        public void InteractWithIncoming(Token incomingToken)
        {
            //
        }

        public bool ReceiveNote(string label, string description,Context context)
        {
            return false;
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
        {//
        }

        public string GetSignature()
        {
            return Id;
        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
            //
        }

        public void OpenAt(TokenLocation location)
        {
            //
        }

        public void Close()
        {
            //
        }

        public void SetToken(Token token)
        {
            _token = token;
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
           //
        }
    }
}
