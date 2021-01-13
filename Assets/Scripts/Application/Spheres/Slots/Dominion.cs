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

        [SerializeField] Transform thresholdsTransform;
        [SerializeField] CanvasGroupFader canvasGroupFader;
        public List<StateEnum> VisibleForStates;
        public List<CommandCategory> RespondToCommandCategories;
        
        readonly HashSet<Threshold> recipeSlots=new HashSet<Threshold>();

        private readonly OnSphereAddedEvent onSphereAdded=new OnSphereAddedEvent();
        private readonly OnSphereRemovedEvent onSphereRemoved=new OnSphereRemovedEvent();
        private SituationPath _situationPath;

        public void AttachTo(Situation situation)
        {

            situation.RegisterDominion(this);
            onSphereAdded.AddListener(situation.AttachSphere);
            onSphereRemoved.AddListener(situation.RemoveSphere);
            _situationPath = situation.Path;

     
            foreach (var c in gameObject.GetComponentsInChildren<ISituationSubscriber>())
                situation.AddSubscriber(c);

        }


        public void SituationStateChanged(Situation situation)
        {
            if(!canvasGroupFader.IsVisible() && situation.CurrentState.IsVisibleInThisState(this))
                canvasGroupFader.Show();

            if (canvasGroupFader.IsVisible() && !situation.CurrentState.IsVisibleInThisState(this))
                canvasGroupFader.Hide();
        }

        public void TimerValuesChanged(Situation situation)
        {
 //
        }

        public void SituationSphereContentsUpdated(Situation s)
        {
         //
        }

        public void ReceiveNotification(INotification n)
        {
         //
        }

        public void CreateThreshold(SphereSpec spec)
        {
            var newSlot = Registry.Get<PrefabFactory>().CreateLocally<Threshold>(thresholdsTransform);
            newSlot.Initialise(spec, _situationPath);

            foreach(var activeInState in VisibleForStates)
               spec.MakeActiveInState(activeInState);


            this.recipeSlots.Add(newSlot);
            onSphereAdded.Invoke(newSlot);
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

            foreach (var os in this.recipeSlots)
            {
                onSphereRemoved.Invoke(os);
                os.Retire();
            }

            this.recipeSlots.Clear();
        }


    }
}
