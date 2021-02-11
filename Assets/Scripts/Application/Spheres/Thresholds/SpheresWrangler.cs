#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI {
    public class SpheresWrangler : MonoBehaviour,ISphereEventSubscriber {

        public Sphere SpherePrefab;
        [SerializeField] private AbstractSphereArrangement sphereArrangement;

        public readonly OnSphereAddedEvent OnSphereAdded = new OnSphereAddedEvent();
        public readonly OnSphereRemovedEvent OnSphereRemoved = new OnSphereRemovedEvent();

        private Dictionary<Sphere, FucinePath> _spheres =
          new Dictionary<Sphere, FucinePath>();

        private Verb _verb;



        /// <summary>
        /// Removes any existing thresholds, so we only ever have one primary
        /// </summary>
        /// <param name="sphereSpec"></param>
        /// <param name="situationPath"></param>
        /// <param name="verb"></param>
        /// <returns></returns>
        public virtual Sphere BuildPrimarySphere(SphereSpec sphereSpec,SituationPath situationPath, Verb verb)
        {
            RemoveAllSpheres();

            _verb = verb;

            return AddSphere(sphereSpec,situationPath);

        }

        
        public void RemoveAllSpheres()
        {
            var thresholdsToRetire = new List<Sphere>(_spheres.Keys);
            
            foreach(var t in thresholdsToRetire)
                RemoveSphere(t);
        }

        public void RemoveSphere(Sphere sphereToRemove) {

            OnSphereRemoved.Invoke(sphereToRemove);
            _spheres.Remove(sphereToRemove);

            sphereToRemove.Retire(SphereRetirementType.Graceful);
        }

       

        protected Sphere AddSphere(SphereSpec sphereSpec,FucinePath parentPath)
        {
            var newSphere = GameObject.Instantiate(SpherePrefab);
            _spheres.Add(newSphere, parentPath);
            SpherePath newThresholdPath = new SpherePath(parentPath, sphereSpec.Id);
            newSphere.Initialise(sphereSpec, newThresholdPath);

            OnSphereAdded.Invoke(newSphere);
            newSphere.Subscribe(this);

            sphereArrangement.ArrangeSphere(newSphere, _spheres.Keys.Count);
            
            return newSphere;
        }

        protected void AddChildSpheresForToken(Token token, FucinePath parentPath)
        {
            var elementInToken = Watchman.Get<Compendium>().GetEntityById<Element>(token.Payload.Id);

            var childSlotSpecs= elementInToken.Slots.Where(cs => cs.ActionId == _verb.Id || cs.ActionId == string.Empty).ToList();
            

            foreach (var childSlotSpecification in childSlotSpecs)
            {
                AddSphere(childSlotSpecification, parentPath);
            }
        }
        
        private void RemoveChildSpheres(Sphere sphereToOrphan)
        {
            if(sphereToOrphan.GetElementStacks().Any())
                NoonUtility.LogWarning($"This code currently assumes thresholds can only contain one stack token. One ({sphereToOrphan.GetElementStacks().First().Id}) has been removed from {sphereToOrphan.GetPath()}, but at least one remains - you may see unexpected results.");

            var spheresToRemove =
                new List<Sphere>(_spheres.Where(kvp=>kvp.Value.Equals(sphereToOrphan.GetPath())).Select(kvp=>kvp.Key));
            foreach(var s in spheresToRemove)
                RemoveSphere(s);
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            //if a token has been added: add any necessary child thresholds
            if(args.TokenAdded!=null && args.TokenRemoved != null)
                NoonUtility.LogWarning($"Tokens with valid element stacks seem to have been added ({args.TokenAdded.name}) and removed ({args.TokenRemoved.name}) in a single event. This will likely cause issues, but we'll go ahead with both.");

            if(args.TokenAdded!=null)
                AddChildSpheresForToken(args.TokenAdded,args.Sphere.GetPath());

            if (args.TokenRemoved!=null)
                RemoveChildSpheres(args.Sphere);

            //if a token has been removed: remove any child thresholds
        }

   
        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }
    }
}