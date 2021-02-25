#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Entities.NullEntities;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI
{
    public class SpheresWrangler : MonoBehaviour, ISphereEventSubscriber
    {
        [SerializeField] private AbstractSphereArrangement sphereArrangement;

        public readonly OnSphereAddedEvent OnSphereAdded = new OnSphereAddedEvent();
        public readonly OnSphereRemovedEvent OnSphereRemoved = new OnSphereRemovedEvent();

        private HashSet<Sphere> _spheres = new HashSet<Sphere>();


        private Verb _verb;
        public Verb Verb => _verb;


        /// <summary>
        /// Removes any existing thresholds, so we only ever have one primary
        /// </summary>
        /// <param name="sphereSpec"></param>
        /// <param name="parentPath"></param>
        /// <param name="verb"></param>
        /// <returns></returns>
        public virtual Sphere BuildPrimarySphere(SphereSpec sphereSpec, Verb verb)
        {
            _verb = verb;

            return AddSphere(sphereSpec);
        }

        public List<Sphere> GetSpheres()
        {
            return new List<Sphere>(_spheres);
        }


        public void RemoveAllSpheres()
        {
            var thresholdsToRetire = new List<Sphere>(_spheres);

            foreach (var t in thresholdsToRetire)
                RemoveSphere(t);
        }

        public void RemoveSphere(Sphere sphereToRemove)
        {
            OnSphereRemoved.Invoke(sphereToRemove);
            _spheres.Remove(sphereToRemove);

            sphereToRemove.Retire(SphereRetirementType.Graceful);
        }


        public Sphere AddSphere(SphereSpec sphereSpec)
        {
            var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(sphereSpec);
            _spheres.Add(newSphere);

            OnSphereAdded.Invoke(newSphere);
            newSphere.Subscribe(this);

            sphereArrangement.AddNewSphereToArrangement(newSphere, _spheres.Count - 1);

            return newSphere;
        }

        protected void AddChildSpheresForToken(Sphere sphere, Token tokenAdded)
        {
            var childSlotSpecs = sphere.GetChildSpheresSpecsToAddIfThisTokenAdded(tokenAdded, this);

            foreach (var childSlotSpecification in childSlotSpecs)
            {
                AddSphere(childSlotSpecification);
            }
        }

        private void RemoveChildSpheres(Sphere sphereToOrphan)
        {
            if (sphereToOrphan.GetElementStacks().Any())
                NoonUtility.LogWarning(
                    $"This code currently assumes thresholds can only contain one stack token. One ({sphereToOrphan.GetElementStacks().First().Id}) has been removed from {sphereToOrphan.Id}, but at least one remains - you may see unexpected results.");

            //THIS WILL EXPLODE. We need to coalese Path and SphereIdentifier (and OwnerSphereIdentifier)
            var spheresToRemove =
                new List<Sphere>(_spheres.Where(s => s.OwnerSphereIdentifier.Equals(sphereToOrphan.Id)));
            foreach (var s in spheresToRemove)
                RemoveSphere(s);
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            //if a token has been added: add any necessary child thresholds
            if (args.TokenAdded != null && args.TokenRemoved != null)
                NoonUtility.LogWarning(
                    $"Tokens with valid element stacks seem to have been added ({args.TokenAdded.name}) and removed ({args.TokenRemoved.name}) in a single event. This will likely cause issues, but we'll go ahead with both.");

            if (args.TokenAdded != null)
                AddChildSpheresForToken(args.Sphere, args.TokenAdded);

            if (args.TokenRemoved != null)
                RemoveChildSpheres(args.Sphere);

            //if a token has been removed: remove any child thresholds
        }


        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }

        public int GetSpheresCurrentlyWrangledCount()
        {
            return _spheres.Count;
        }
    }
}