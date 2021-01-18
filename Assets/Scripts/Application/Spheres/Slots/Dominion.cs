#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.UI.Situation;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SecretHistories.Services;

namespace SecretHistories.UI {
    public class Dominion:MonoBehaviour,ISituationSubscriber {

        [SerializeField] ThresholdsWrangler thresholdsWrangler;
        [SerializeField] CanvasGroupFader canvasGroupFader;
        public List<StateEnum> VisibleForStates;
        public List<CommandCategory> RespondToCommandCategories;

        public OnSphereAddedEvent OnSphereAdded => thresholdsWrangler.OnSphereAdded;

        public OnSphereRemovedEvent OnSphereRemoved => thresholdsWrangler.OnSphereRemoved;
        private SituationPath _situationPath;
        private IVerb situationVerb;

        public void AttachTo(Situation situation)
        {

            situation.RegisterDominion(this);
            _situationPath = situation.Path;

     
            foreach (var c in gameObject.GetComponentsInChildren<ISituationSubscriber>())
                situation.AddSubscriber(c);

        }


        public void SituationStateChanged(Situation situation)
        {
            _situationPath = situation.Path;
            situationVerb = situation.Verb;

            if(situation.CurrentState.IsVisibleInThisState(this))
                canvasGroupFader.Show();

            if (!situation.CurrentState.IsVisibleInThisState(this))
                canvasGroupFader.Hide();
        }

        public void TimerValuesChanged(Situation situation)
        {
 //
        }

        public void SituationSphereContentsUpdated(Situation s)
        {
         
        }

        public void ReceiveNotification(INotification n)
        {
         //
        }

        public void CreateThreshold(SphereSpec spec)
        {
            foreach (var activeInState in VisibleForStates)
                spec.MakeActiveInState(activeInState);

            thresholdsWrangler.BuildPrimaryThreshold(spec,_situationPath,situationVerb);
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
