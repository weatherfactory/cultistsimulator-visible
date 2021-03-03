#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.UI.Situation;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SecretHistories.Services;
using SecretHistories.Spheres;

namespace SecretHistories.UI {
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public class SituationDominion: MonoBehaviour, IDominion,ISphereEventSubscriber
    {
        [SerializeField] CanvasGroupFader canvasGroupFader;

        [SerializeField] private AbstractSphereArrangement sphereArrangement;

        
        private OnSphereAddedEvent _onSphereAdded = new OnSphereAddedEvent();
        private OnSphereRemovedEvent _onSphereRemoved = new OnSphereRemovedEvent();
        

        [Encaust]
        public List<Sphere> Spheres => new List<Sphere>(_spheres);

        [DontEncaust]
        public OnSphereAddedEvent OnSphereAdded
        {
            get => _onSphereAdded;
            set => _onSphereAdded = value;
        }

        [DontEncaust]
        public OnSphereRemovedEvent OnSphereRemoved
        {
            get => _onSphereRemoved;
            set => _onSphereRemoved = value;
        }

        [Header("Preserve spheres when dismissed?")]
        [Tooltip("Dominions will gracefully retire spheres and flush their contents when dismissed unless this box is ticked. NB that hiding a window doesn't dismiss dominions - dismissal is a situation life cycle thing.")]
        public bool PreserveSpheresWhenDismissed;
        public List<StateEnum> VisibleForStates;
        public Sphere spherePrefab;

        private Situation _situation;
        private readonly List<Sphere> _spheres=new List<Sphere>();
        

        public void RegisterFor(IManifestable situation)
        {

            _situation = situation as Situation; // this is a bit schizo; we're subscribing to it, but we're also keeping a reference?

            _situation.RegisterDominion(this);

            foreach (var subscriber in gameObject.GetComponentsInChildren<ISituationSubscriber>())
                _situation.AddSubscriber(subscriber);

            foreach (var existingSphere in gameObject.GetComponentsInChildren<Sphere>())
                _situation.AttachSphere(existingSphere);
        }

        public void Evoke()
        {
            canvasGroupFader.Show();
        }

        public void Dismiss()
        {
            canvasGroupFader.Hide();
            //if(!PreserveSpheresWhenDismissed)
            //    RemoveAllSpheres();
        }

        public  Sphere CreateSphere(SphereSpec spec)
        {
            //ensure that the spec will be visible in states for which this dominion is active
            foreach (var activeInState in VisibleForStates)
                spec.MakeActiveInState(activeInState);

            return AddSphere(spec);
        }



        public bool VisibleFor(StateEnum state)
        {
            return VisibleForStates.Contains(state);
        }

        public bool RelevantTo(StateEnum state,Type sphereType)
        {
            Type dominionSphereType = spherePrefab.GetType();
            return VisibleForStates.Contains(state) && sphereType == dominionSphereType;
        }

        public Sphere GetSphereById(string Id)
        {
            return Spheres.SingleOrDefault(s => s.Id == Id);
        }

      


        public void RemoveAllSpheres()
        {
            var spheresToRetire = new List<Sphere>(_spheres);

            foreach (var t in spheresToRetire)
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
            var childSlotSpecs = sphere.GetChildSpheresSpecsToAddIfThisTokenAdded(tokenAdded, _situation.VerbId);

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
                new List<Sphere>(_spheres.Where(s => s.OwnerSphereIdentifier==sphereToOrphan.Id));
            foreach (var s in spheresToRemove)
                RemoveSphere(s);
        }

        public void OnTokensChangedForAnySphere(SphereContentsChangedEventArgs args)
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


        public void OnTokenInteractionInAnySphere(TokenInteractionEventArgs args)
        {
            //
        }

        public int GetSpheresCurrentlyWrangledCount()
        {
            return _spheres.Count;
        }
    }
}
