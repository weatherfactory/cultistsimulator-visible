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

using UnityEngine.Events;

namespace SecretHistories.UI {
    public class RecipeSlotsDominion:MonoBehaviour,ISituationSubscriber,IDominion {

        [SerializeField] Transform thresholdsTransform;
        [SerializeField] CanvasGroupFader canvasGroupFader;
        
        readonly HashSet<Threshold> recipeSlots=new HashSet<Threshold>();

        private readonly OnSphereAddedEvent onSphereAdded=new OnSphereAddedEvent();
        private readonly OnSphereRemovedEvent onSphereRemoved=new OnSphereRemovedEvent();
        private SituationPath _situationPath;

        public void Initialise(Situation situation)
        {
            situation.AddSubscriber(this);
            situation.RegisterAttachment(this);
            onSphereAdded.AddListener(situation.AttachSphere);
            onSphereRemoved.AddListener(situation.RemoveSphere);
            _situationPath = situation.Path;

     
            foreach (var c in gameObject.GetComponentsInChildren<ISituationSubscriber>())
                situation.AddSubscriber(c);

        }


        public void SituationStateChanged(Situation situation)
        {
            //
        }

        public void TimerValuesChanged(Situation situation)
        {
            if (situation.TimeRemaining <= 0f)
                canvasGroupFader.Hide();
            else
            {
                canvasGroupFader.Show();
            }
        }

        public void SituationSphereContentsUpdated(Situation s)
        {
         //
        }

        public void ReceiveNotification(INotification n)
        {
         //
        }

        public void CreateThreshold(SlotSpecification spec)
        {
            var newSlot = Registry.Get<PrefabFactory>().CreateLocally<Threshold>(thresholdsTransform);
            newSlot.Initialise(spec, _situationPath);


            spec.MakeActiveInState(StateEnum.Ongoing);


            this.recipeSlots.Add(newSlot);
            onSphereAdded.Invoke(newSlot);
        }

        public bool MatchesCommandCategory(CommandCategory category)
        {

            return category == CommandCategory.RecipeThresholds;
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
