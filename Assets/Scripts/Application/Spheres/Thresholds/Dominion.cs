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
    public class Dominion:MonoBehaviour,IEncaustable {

        [SerializeField] SpheresWrangler _spheresWrangler;
        [SerializeField] CanvasGroupFader canvasGroupFader;

        [Encaust]
        public List<Sphere> Spheres => _spheresWrangler.GetSpheres();
        
        [DontEncaust]
        public OnSphereAddedEvent OnSphereAdded => _spheresWrangler.OnSphereAdded;
        [DontEncaust]
        public OnSphereRemovedEvent OnSphereRemoved => _spheresWrangler.OnSphereRemoved;

        [Header("Preserve spheres when dismissed?")]
        [Tooltip("Dominions will gracefully retire spheres and flush their contents when dismissed unless this box is ticked. NB that hiding a window doesn't dismiss dominions - dismissal is a situation life cycle thing.")]
        public bool PreserveSpheresWhenDismissed;
        public List<StateEnum> VisibleForStates;
        public Sphere spherePrefab;

        private Situation _situation;

        public void RegisterFor(Situation situation)
        {

            _situation = situation; // this is a bit schizo; we're subscribing to it, but we're also keeping a reference?

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
            if(!PreserveSpheresWhenDismissed)
                RemoveAllSpheres();
        }

        public Sphere CreatePrimarySphere(SphereSpec spec)
        {
            //ensure that the spec will be visible in states for which this dominion is active
            foreach (var activeInState in VisibleForStates)
                spec.MakeActiveInState(activeInState);

            return  _spheresWrangler.BuildPrimarySphere(spec,_situation.Path,_situation.Verb);
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



        public void RemoveAllSpheres()
        {
            _spheresWrangler.RemoveAllSpheres();
        }


    }
}
