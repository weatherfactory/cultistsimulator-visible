#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.TabletopUi;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine.Events;

namespace Assets.CS.TabletopUI {
    public class OngoingDisplay:MonoBehaviour {

        [SerializeField] Transform slotHolder; 
        [SerializeField] Image countdownBar;
		[SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] LayoutGroup storedCardsLayout;
        public CanvasGroupFader canvasGroupFader;

        [SerializeField] DeckEffectView[] deckEffectViews; 
        HashSet<RecipeSlot> ongoingSlots=new HashSet<RecipeSlot>();

        private OnContainerAddedEvent _onSlotAdded;
        private OnContainerRemovedEvent _onSlotRemoved;
        private SituationPath _situationPath;

        public void Initialise(OnContainerAddedEvent onContainerAdded, OnContainerRemovedEvent onContainerRemoved,SituationCreationCommand situationCreationCommand)
        {
            _onSlotAdded = onContainerAdded;
            _onSlotRemoved = onContainerRemoved;
            _situationPath = situationCreationCommand.SituationPath;
            populateOngoingSlots((situationCreationCommand.OngoingSlots));
        }


        private void populateOngoingSlots(List<SlotSpecification> ongoingSlots)
        {

            foreach (var os in this.ongoingSlots)
            {
                _onSlotRemoved.Invoke(os);
                os.Retire();
            }

            this.ongoingSlots.Clear();


            foreach (var spec in ongoingSlots)
            {
                var newSlot = Registry.Get<PrefabFactory>().CreateLocally<RecipeSlot>(slotHolder);
                newSlot.name = spec.UniqueId;

                newSlot.Initialise(spec, _situationPath);

                this.ongoingSlots.Add(newSlot);
                _onSlotAdded.Invoke(newSlot);

            }
        }

        public void UpdateDisplay(SituationEventData e)
		{
            switch(e.SituationState)
            {
                case SituationState.Unstarted:

                    canvasGroupFader.Hide();
                    break;

                case SituationState.Ongoing:
                    canvasGroupFader.Show();
                    ShowDeckEffects(e.CurrentRecipe.DeckEffects);

                    Color barColor = UIStyle.GetColorForCountdownBar(e.CurrentRecipe.SignalEndingFlavour, e.TimeRemaining);

                    countdownBar.color = barColor;
                    countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (e.TimeRemaining / e.Warmup));
                    countdownText.color = barColor;
			        countdownText.text = Registry.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage( e.TimeRemaining );
                    countdownText.richText = true;

                    if (e.BeginningEffectCommand != null)
                    {
                        populateOngoingSlots(e.BeginningEffectCommand.OngoingSlots);
                    }


                    break;

                
                default:
                    canvasGroupFader.Hide();
                    break;
            }
    }

        public void ShowStoredAspects(IEnumerable<ElementStackToken> stacks) {
            int i = 0;

            var aspectFrames = storedCardsLayout.GetComponentsInChildren<ElementFrame>();
            ElementFrame frame;
            Element element;

            foreach (var stack in stacks) {
                element = Registry.Get<ICompendium>().GetEntityById<Element>(stack.EntityId);

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
                var deckSpec = Registry.Get<ICompendium>().GetEntityById<DeckSpec>(item.Key);
                deckEffectViews[i].PopulateDisplay(deckSpec, item.Value);
                deckEffectViews[i].gameObject.SetActive(true);
                i++;
            }


        }

    }
}
