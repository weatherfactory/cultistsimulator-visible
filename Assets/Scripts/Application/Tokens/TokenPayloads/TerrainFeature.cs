using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Constants.Events;
using SecretHistories.Core;
using SecretHistories.Events;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads
{
    public class TerrainFeature: MonoBehaviour, IHasAspects,ISphereEventSubscriber
    {
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public string Id { get; }
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
            return new List<Sphere>(_spheres);
        }

        public void AttachSphere(Sphere sphere)
        {
            sphere.Subscribe(this);
            sphere.SetContainer(this);
            _spheres.Add(sphere);
        }

        public void DetachSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _spheres.Remove(c);
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
    }
}