#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using SecretHistories.Commands;
using Assets.TabletopUi;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using SecretHistories.Interfaces;
using SecretHistories.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SecretHistories.Services;

using UnityEngine.Events;

namespace SecretHistories.UI {
    public class OngoingDisplay:MonoBehaviour,ISituationSubscriber,ISituationAttachment {

        [SerializeField] Transform slotHolder; 
        [SerializeField] Image countdownBar;
		[SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] LayoutGroup storedCardsLayout;
        public CanvasGroupFader canvasGroupFader;

        [SerializeField] DeckEffectView[] deckEffectViews;
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
        }


        public void SituationStateChanged(Situation situation)
        {
            ShowDeckEffects(situation.CurrentPrimaryRecipe.DeckEffects);
        }

        public void TimerValuesChanged(Situation situation)
        {
            UpdateTimerVisuals(situation.Warmup, situation.TimeRemaining,
                situation.IntervalForLastHeartbeat, false, situation.CurrentPrimaryRecipe.SignalEndingFlavour);
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



        public void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
            if(durationRemaining<=0f)
                canvasGroupFader.Hide();
            else
            {
                canvasGroupFader.Show();

                Color barColor =
                    UIStyle.GetColorForCountdownBar(signalEndingFlavour, durationRemaining);

                countdownBar.color = barColor;
                countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (durationRemaining / originalDuration));
                countdownText.color = barColor;
                countdownText.text =
                    Registry.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(durationRemaining);
                countdownText.richText = true;



            }

        }


        public void ShowStoredAspects(IEnumerable<ElementStack> stacks) {
            int i = 0;

            var aspectFrames = storedCardsLayout.GetComponentsInChildren<ElementFrame>();
            ElementFrame frame;
            Element element;

            foreach (var stack in stacks) {
                element = Registry.Get<Compendium>().GetEntityById<Element>(stack.Element.Id);

                if(!element.IsHidden)
                { 
                    for (int q = 0; q < stack.Quantity; q++) {
                        if (i < aspectFrames.Length)
                            frame = aspectFrames[i];
                        else
                            frame = Registry.Get<PrefabFactory>().CreateLocally<ElementFrame>(storedCardsLayout.transform);

                        frame.PopulateDisplay(element,1);
                        frame.gameObject.SetActive(true);
                        i++;
                    }
                }
            }

            while (i < aspectFrames.Length) {
                aspectFrames[i].gameObject.SetActive(false);

                i++;
            }
        }

        public void ShowDeckEffects(Dictionary<string, int> deckEffects) {
            if(deckEffects.Count>deckEffectViews.Length)
                NoonUtility.LogWarning($"{deckEffects.Count} deck effects to show in OngoingDisplay, but only {deckEffectViews.Length} slots.");

            int i = 0;
            foreach(var dev in deckEffectViews)
                dev.gameObject.SetActive(false);


            // Populate those we need
            foreach (var item in deckEffects) {
                var deckSpec = Registry.Get<Compendium>().GetEntityById<DeckSpec>(item.Key);
                deckEffectViews[i].PopulateDisplay(deckSpec, item.Value);
                deckEffectViews[i].gameObject.SetActive(true);
                i++;
            }


        }

    }
}
