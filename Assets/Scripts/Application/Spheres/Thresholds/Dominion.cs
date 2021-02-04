#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.UI.Situation;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SecretHistories.Services;
using SecretHistories.Spheres;

namespace SecretHistories.UI {
    public class Dominion:MonoBehaviour {

        [SerializeField] ThresholdsWrangler thresholdsWrangler;
        [SerializeField] CanvasGroupFader canvasGroupFader;
        public List<StateEnum> VisibleForStates;
        public List<CommandCategory> RespondToCommandCategories;

        public OnSphereAddedEvent OnSphereAdded => thresholdsWrangler.OnSphereAdded;

        public OnSphereRemovedEvent OnSphereRemoved => thresholdsWrangler.OnSphereRemoved;
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

        public void Show()
        {
            canvasGroupFader.Show();

        }

        public void Hide()
        {
            canvasGroupFader.Hide();
        }

        public void CreateThreshold(SphereSpec spec)
        {
            foreach (var activeInState in VisibleForStates)
                spec.MakeActiveInState(activeInState);

            thresholdsWrangler.BuildPrimaryThreshold(spec,_situation.Path,_situation.Verb);
        }

        public bool VisibleFor(StateEnum state)
        {
            return VisibleForStates.Contains(state);
        }

        public bool MatchesCommandCategory(CommandCategory category)
        {
            return RespondToCommandCategories.Contains(category);
        }

        public void ClearThresholds()
        {
            thresholdsWrangler.RemoveAllThresholds();
        }


    }
}
