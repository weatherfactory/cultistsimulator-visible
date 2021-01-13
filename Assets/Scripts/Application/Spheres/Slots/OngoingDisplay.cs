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
    public class OngoingDisplay:MonoBehaviour,ISituationSubscriber,ISituationAttachment {

        [SerializeField] Transform slotHolder; 

        [SerializeField] LayoutGroup storedCardsLayout;
        public CanvasGroupFader canvasGroupFader;
        [SerializeField] SituationCountdownDisplay countdownDisplay;
        [SerializeField] private SituationDeckEffectsView deckEffectsView;

        readonly HashSet<Threshold> recipeSlots=new HashSet<Threshold>();

        private readonly OnContainerAddedEvent _onSlotAdded=new OnContainerAddedEvent();
        private readonly OnContainerRemovedEvent _onSlotRemoved=new OnContainerRemovedEvent();
        private SituationPath _situationPath;

        public void Initialise(Situation situation)
        {
            situation.AddSubscriber(this);
            situation.RegisterAttachment(this);
            _onSlotAdded.AddListener(situation.AttachSphere);
            _onSlotRemoved.AddListener(situation.RemoveContainer);
            _situationPath = situation.Path;

            situation.AddSubscriber(countdownDisplay);
            situation.AddSubscriber(deckEffectsView);

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
            var newSlot = Registry.Get<PrefabFactory>().CreateLocally<Threshold>(slotHolder);
            newSlot.Initialise(spec, _situationPath);


            spec.MakeActiveInState(StateEnum.Ongoing);


            this.recipeSlots.Add(newSlot);
            _onSlotAdded.Invoke(newSlot);
        }

        public bool MatchesCommandCategory(CommandCategory category)
        {

            return category == CommandCategory.RecipeSlots;
        }

        public void ClearThresholds()
        {

            foreach (var os in this.recipeSlots)
            {
                _onSlotRemoved.Invoke(os);
                os.Retire();
            }

            this.recipeSlots.Clear();
        }


    }
}
