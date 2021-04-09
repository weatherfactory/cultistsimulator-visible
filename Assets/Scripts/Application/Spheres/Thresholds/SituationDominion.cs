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
using SecretHistories.Events;
using SecretHistories.Fucine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEditorInternal;

namespace SecretHistories.UI {

    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class SituationDominion: AbstractDominion,ISphereEventSubscriber
    {
        [SerializeField] CanvasGroupFader canvasGroupFader;

        [SerializeField] private AbstractSphereArrangement sphereArrangement;


        [SerializeField] private string EditableIdentifier;

        public List<StateEnum> VisibleForStates;
        public Sphere spherePrefab;

       

        [SerializeField] private int MaxSpheresAllowed;


      public void Awake()
      {
          Identifier = EditableIdentifier;
      }

        public override void RegisterFor(IManifestable manifestable)
        {

            base.RegisterFor(manifestable);

          var situation = manifestable as Situation;
            
            foreach (var subscriber in gameObject.GetComponentsInChildren<ISituationSubscriber>())
                situation.AddSubscriber(subscriber);

            foreach (var existingSphere in gameObject.GetComponentsInChildren<Sphere>())
                situation.AttachSphere(existingSphere);

            OnSphereRemoved.AddListener(sphereArrangement.SphereRemoved);
        }

        public override void Evoke()
        {
            canvasGroupFader.Show();
        }

        public override void Dismiss()
        {
            canvasGroupFader.Hide();
        }

        public override Sphere TryCreateSphere(SphereSpec spec)
        {
            if (!CanCreateSphere(spec))
                return NullSphere.Create();

            if(VisibleForStates==null)
                VisibleForStates=new List<StateEnum>(); //in case it hasn't been initialised - eg in testing

            //ensure that the spec will be visible in states for which this dominion is active
            foreach (var activeInState in VisibleForStates)
                spec.MakeActiveInState(activeInState);

            return AddSphere(spec);
        }
public override bool CanCreateSphere(SphereSpec spec)
{
    if (GetSphereById(spec.Id) != null)
        return false; //no spheres with duplicate id

            if (MaxSpheresAllowed == 0)
                return true;
            else
                return (_spheres.Count(s => !s.Defunct) < MaxSpheresAllowed);
}




public override bool VisibleFor(string state)
{
    var canParseState = Enum.TryParse(state, false, out StateEnum stateEnum);
    if (!canParseState)
    {
        NoonUtility.LogWarning($"Can't parse {state} as a StateEnum; assuming it's a match for dominion {Identifier}");
        return true;

    }
    stateEnum = (StateEnum)Enum.Parse(typeof(StateEnum), state);
    return VisibleForStates.Contains(stateEnum);
        }

        public override bool RelevantTo(string state,Type sphereType)
        {
         var canParseState=Enum.TryParse(state, false, out StateEnum stateEnum);
         if (!canParseState)
         {
             NoonUtility.LogWarning($"Can't parse {state} as a StateEnum; assuming it's a match for dominion {Identifier}");
             return true;
         }
         stateEnum = (StateEnum)Enum.Parse(typeof(StateEnum), state);

         Type dominionSphereType = spherePrefab.GetType();
         return VisibleForStates.Contains(stateEnum) && sphereType == dominionSphereType;
        }


        public override bool RemoveSphere(string id,SphereRetirementType retirementType)
        {
            var sphereToRemove = GetSphereById(id);
            if (sphereToRemove == null)
                return false;
            RemoveSphere(sphereToRemove, retirementType);
            return true;
        }


        public void RemoveSphere(Sphere sphereToRemove,SphereRetirementType retirementType)
        {
            OnSphereRemoved.Invoke(sphereToRemove);
            _spheres.Remove(sphereToRemove);

            sphereToRemove.Retire(retirementType);
        }


        public Sphere AddSphere(SphereSpec sphereSpec)
        {
            var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(sphereSpec);
            _spheres.Add(newSphere);

            OnSphereAdded.Invoke(newSphere);
            newSphere.Subscribe(this);

            if(sphereArrangement!=null) //for testing, but may be useful later also
                sphereArrangement.AddNewSphereToArrangement(newSphere, _spheres.Count - 1);

            return newSphere;
        }

        protected void AddDependentSpheresForToken(Sphere sphere, Token tokenAdded)
        {
            var situation = _manifestable as Situation;

            if (situation==null)
                throw new NotImplementedException("UGH PICK AN INTERFACE AK");

            var childSlotSpecs = sphere.GetChildSpheresSpecsToAddIfThisTokenAdded(tokenAdded, situation.VerbId);

            foreach (var childSlotSpecification in childSlotSpecs)
            {
                var newSphere=AddSphere(childSlotSpecification);
                newSphere.OwnerSphereIdentifier = sphere.Id;
            }
        }

        private void removeDependentSpheres(Sphere sphereToOrphan)
        {

            //This assumes all spheres in a given dominion will have unique ids... but, currently, they should!
            var spheresToRemove =
                new List<Sphere>(_spheres.Where(s => s.OwnerSphereIdentifier==sphereToOrphan.Id));
            foreach (var s in spheresToRemove)
                RemoveSphere(s,SphereRetirementType.Graceful);
        }

        public void OnSphereChanged(SphereChangedArgs args)
        {
            //
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            //if a token has been added: add any necessary child thresholds
            if (args.TokenAdded != null && args.TokenRemoved != null)
                NoonUtility.LogWarning(
                    $"Tokens with valid element stacks seem to have been added ({args.TokenAdded.name}) and removed ({args.TokenRemoved.name}) in a single event. This will likely cause issues, but we'll go ahead with both.");

            if (args.TokenAdded != null)
                AddDependentSpheresForToken(args.Sphere, args.TokenAdded);

            if (args.TokenRemoved != null)
            {
                if(args.Context.actionSource!=Context.ActionSource.FlushingTokens) //This is another strong argument for moving ActionSource to class rather than enum.
                //The reason it's here is because when we're flushing tokens between categories during situation activation, the first token will leave the slot,
                //and all the other tokens will be evicted from the disappearing dependent-slots before they can be flushed.
                    removeDependentSpheres(args.Sphere);
            }

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
