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
    public class ThresholdsWrangler : MonoBehaviour,ISphereEventSubscriber {

        public ThresholdSphere thresholdSpherePrefab;
        [SerializeField] private AbstractThresholdArrangement thresholdArrangement;

        public readonly OnSphereAddedEvent OnSphereAdded = new OnSphereAddedEvent();
        public readonly OnSphereRemovedEvent OnSphereRemoved = new OnSphereRemovedEvent();

        private Dictionary<ThresholdSphere, FucinePath> _thresholds =
          new Dictionary<ThresholdSphere, FucinePath>();

        private IVerb _verb;



        /// <summary>
        /// Removes any existing thresholds, so we only ever have one primary
        /// </summary>
        /// <param name="sphereSpec"></param>
        /// <param name="situationPath"></param>
        /// <param name="verb"></param>
        /// <returns></returns>
        public virtual ThresholdSphere BuildPrimaryThreshold(SphereSpec sphereSpec,SituationPath situationPath, IVerb verb)
        {
            RemoveAllThresholds();

            _verb = verb;

            return AddThreshold(sphereSpec,situationPath);

        }

        
        public void RemoveAllThresholds()
        {
            var thresholdsToRetire = new List<ThresholdSphere>(_thresholds.Keys);
            
            foreach(var t in thresholdsToRetire)
                RemoveThreshold(t);
        }

        public void RemoveThreshold(ThresholdSphere thresholdToRemove) {

            OnSphereRemoved.Invoke(thresholdToRemove);
            _thresholds.Remove(thresholdToRemove);

            if (gameObject.activeInHierarchy)
                thresholdToRemove.viz.TriggerHideAnim();
            else
                thresholdToRemove.Retire(SphereRetirementType.Graceful);
        }

       

        protected ThresholdSphere AddThreshold(SphereSpec sphereSpec,FucinePath parentPath)
        {
            var newThreshold = GameObject.Instantiate(thresholdSpherePrefab);
            _thresholds.Add(newThreshold, parentPath);
            SpherePath newThresholdPath = new SpherePath(parentPath, sphereSpec.Id);
            newThreshold.Initialise(sphereSpec, newThresholdPath);

            

            OnSphereAdded.Invoke(newThreshold);
            newThreshold.Subscribe(this);

            thresholdArrangement.ArrangeThreshold(newThreshold, _thresholds.Keys.Count);



            return newThreshold;
        }

        protected void AddThresholdChildrenForToken(Token token, FucinePath parentPath)
        {
            var elementInToken = Watchman.Get<Compendium>().GetEntityById<Element>(token.Payload.Id);

            var childSlotSpecs= elementInToken.Slots.Where(cs => cs.ActionId == _verb.Id || cs.ActionId == string.Empty).ToList();
            

            foreach (var childSlotSpecification in childSlotSpecs)
            {
                AddThreshold(childSlotSpecification, parentPath);
            }
        }
        
        private void RemoveThresholdChildren(Sphere thresholdToOrphan)
        {
            if(thresholdToOrphan.GetElementStacks().Any())
                NoonUtility.LogWarning($"This code currently assumes thresholds can only contain one stack token. One ({thresholdToOrphan.GetElementStacks().First().Id}) has been removed from {thresholdToOrphan.GetPath()}, but at least one remains - you may see unexpected results.");

            var thresholdstoRemove=
                new List<ThresholdSphere>(_thresholds.Where(kvp=>kvp.Value.Equals(thresholdToOrphan.GetPath())).Select(kvp=>kvp.Key));
            foreach(var t in thresholdstoRemove)
                RemoveThreshold(t);
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            //if a token has been added: add any necessary child thresholds
            if(args.TokenAdded!=null && args.TokenRemoved != null)
                NoonUtility.LogWarning($"Tokens with valid element stacks seem to have been added ({args.TokenAdded.name}) and removed ({args.TokenRemoved.name}) in a single event. This will likely cause issues, but we'll go ahead with both.");

            if(args.TokenAdded!=null)
                AddThresholdChildrenForToken(args.TokenAdded,args.Sphere.GetPath());

            if (args.TokenRemoved!=null)
                RemoveThresholdChildren(args.Sphere);

            //if a token has been removed: remove any child thresholds
        }

   
        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }
    }
}