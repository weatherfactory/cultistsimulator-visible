#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Entities.NullEntities;
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



        [Encaust] public DominionEnum Identifier => EditableIdentifier;
        [SerializeField] private DominionEnum EditableIdentifier;

        [Encaust]
        public List<Sphere> Spheres => new List<Sphere>(_spheres);



        private OnSphereAddedEvent _onSphereAdded = new OnSphereAddedEvent();
        private OnSphereRemovedEvent _onSphereRemoved = new OnSphereRemovedEvent();

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

        public List<StateEnum> VisibleForStates;
        public Sphere spherePrefab;

        private Situation _situation;
        private readonly List<Sphere> _spheres=new List<Sphere>();
      [SerializeField] private int MaxSpheresAllowed;


        public void RegisterFor(IManifestable situation)
        {

            _situation = situation as Situation; // this is a bit schizo; we're subscribing to it, but we're also keeping a reference?

            _situation.RegisterDominion(this);

            foreach (var subscriber in gameObject.GetComponentsInChildren<ISituationSubscriber>())
                _situation.AddSubscriber(subscriber);

            foreach (var existingSphere in gameObject.GetComponentsInChildren<Sphere>())
                _situation.AttachSphere(existingSphere);

            OnSphereRemoved.AddListener(sphereArrangement.SphereRemoved);
        }

        public void Evoke()
        {
            canvasGroupFader.Show();
        }

        public void Dismiss()
        {
            canvasGroupFader.Hide();
        }

        public  Sphere TryCreateSphere(SphereSpec spec)
        {
            if (!CanCreateSphere(spec))
                return NullSphere.Create();

            //ensure that the spec will be visible in states for which this dominion is active
            foreach (var activeInState in VisibleForStates)
                spec.MakeActiveInState(activeInState);

            return AddSphere(spec);
        }
public bool CanCreateSphere(SphereSpec spec)
{
    if (MaxSpheresAllowed == 0)
        return true;

    return (_spheres.Count < MaxSpheresAllowed);
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
            return Spheres.SingleOrDefault(s => s.Id == Id && !s.Defunct);
        }

        public bool RemoveSphere(string id)
        {
            var sphereToRemove = GetSphereById(id);
            if (sphereToRemove == null)
                return false;
            RemoveSphere(sphereToRemove);
            return true;
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
                var newSphere=AddSphere(childSlotSpecification);
                newSphere.OwnerSphereIdentifier = sphere.Id;
            }
        }

        private void RemoveChildSpheres(Sphere sphereToOrphan)
        {

            //This assumes all spheres in a given dominion will have unique ids... but, currently, they should!
            var spheresToRemove =
                new List<Sphere>(_spheres.Where(s => s.OwnerSphereIdentifier==sphereToOrphan.Id));
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

            //if a token has been removed: remove any child thresholds
            if (args.TokenRemoved != null)
                RemoveChildSpheres(args.Sphere);

         
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
