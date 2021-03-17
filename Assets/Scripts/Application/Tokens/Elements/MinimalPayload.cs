using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Entities
{
    
    public class MinimalPayload : ITokenPayload
    {
#pragma warning disable 67
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
#pragma warning restore 67

        private Token _token;
        private List<IDominion> _dominions= new List<IDominion>();
        public string Id { get; protected set; }

        private HashSet<Sphere> _spheres=new HashSet<Sphere>();
        
        public FucinePath GetAbsolutePath()
        {
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendToken(this.Id);
            return absolutePath;
        }

                public void SetToken(Token token)
        {
            _token = token;
        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public Dictionary<string, int> Mutations { get; }
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
      //
        }

        public string GetSignature()
        {
            return Id;
        }

        public Sphere GetEnRouteSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsValid() && !Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsEmpty())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.EnRouteSpherePath);

            return Token.Sphere.GetContainer().GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsValid() && !Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsEmpty())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.WindowsSpherePath);

            return Token.Sphere.GetContainer().GetWindowsSphere();
        }

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

        public void AttachSphere(Sphere sphere)
        {
          sphere.SetContainer(this);
          _spheres.Add(sphere);
        }

        public void DetachSphere(Sphere sphere)
        {
            _spheres.Remove(sphere);
        }

        public string Label { get; }
        public string Description { get; }
        public int Quantity { get; }
        public string UniquenessGroup { get; }
        public bool Unique { get; }
        public string Icon { get; }
        public string GetIllumination(string key)
        {
            return String.Empty;
        }

        public void SetIllumination(string key, string value)
        {
            //
        }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }




        public MinimalPayload(string id)
        {
            Id = id;
        }
        
        
        public bool IsOpen { get; }
        public string EntityId => "minimal";
        public FucinePath AbsolutePath { get; }
        public List<IDominion> Dominions=>new List<IDominion>(_dominions);
        public bool RegisterDominion(IDominion dominion)
        {
            if (_dominions.Contains(dominion))
                return false;
            _dominions.Add(dominion);
            return true;
        }

        public Sphere GetSphereById(string sphereId)
        {
            return _spheres.SingleOrDefault(s => s.Id == sphereId && !s.Defunct);
        }

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

        public void FirstHeartbeat()
        {
            ExecuteHeartbeat(0f);
        }

        public void ExecuteHeartbeat(float interval)
        {
        //
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;

        }

        public bool Retire(RetirementVFX vfx)
        {
            throw new NotImplementedException("Retiring a minimal payload. should this kill the token?");
        }

        public void InteractWithIncoming(Token incomingToken)
        {
            //
        }

        public bool ReceiveNote(string label, string description, Context context)
        {
            return false;
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            //

        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            //

        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            //

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



        public void OnTokenMoved(TokenLocation toLocation)
        {
            //
        }
    }
}
