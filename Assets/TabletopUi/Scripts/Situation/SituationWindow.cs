using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.SlotsContainers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI {
    [RequireComponent(typeof(SituationWindowPositioner))]
    public class SituationWindow : MonoBehaviour, ISituationDetails {

        const string buttonDefault = "Start";
        const string buttonBusy = "Waiting...";

		[Header("Visuals")]
		[SerializeField] CanvasGroupFader canvasGroupFader;
        public SituationWindowPositioner positioner;

        [Space]
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;
		public PaginatedText PaginatedNotes;

		[Space]
        // Public to test
		public SituationSlotManager slotManager;
        [SerializeField] StartingSlotsContainer startingSlots;

        [Space]
        [SerializeField] OngoingSlotManager ongoing;

        [Space]
        [SerializeField] SituationResults results;

        [Space]
        [SerializeField] SituationStorage storage;

        [Space]
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] TextMeshProUGUI hintText;

		[SerializeField] Button startButton;
		[SerializeField] TextMeshProUGUI startButtonText;

		private SituationController situationController;
		private IVerb Verb;
        private ISituationDetails _situationDetailsImplementation;

        public bool IsOpen {
            get { return gameObject.activeInHierarchy; }
        }

		public string Title {
			get { return title.text; }
			set { title.text = value; }
        }


        // INIT & LIFECYCLE

        void OnEnable() {
			startButton.onClick.AddListener(HandleButtonAction);
		}

		void OnDisable() {
			startButton.onClick.RemoveListener(HandleButtonAction);
		}

		public void Initialise(IVerb verb, SituationController sc) {
			situationController = sc;
			Verb = verb;
            name = "Window_" + verb.Id;
            artwork.sprite = ResourcesManager.GetSpriteForVerbLarge(Verb.Id);


            startingSlots.Initialise(sc);
            ongoing.Initialise(sc);
            results.Initialise(sc);
		}

        public void Retire() {
            Destroy(gameObject);
        }

        // to be accessable from Close Button
        public void Close() {
            situationController.CloseSituation();
        }

        // BASIC DISPLAY

        public void Show() {
			canvasGroupFader.Show();
            positioner.Show(canvasGroupFader.durationTurnOn); // Animates the window
            SoundManager.PlaySfx("SituationWindowShow");
		}

		public void Hide() {
			canvasGroupFader.Hide();
            SoundManager.PlaySfx("SituationWindowHide");
        }

        // Start State

		public void SetUnstarted() {
            startingSlots.Reset();
            startingSlots.gameObject.SetActive(true);

            ongoing.Reset();
            ongoing.gameObject.SetActive(false);

            results.Reset();
            results.gameObject.SetActive(false);

            DisplayUnstarted();
        }

        // Ongoing State

		public void SetOngoing(Recipe recipe) {
            startingSlots.gameObject.SetActive(false);
            ConsumeMarkedElements(false);

            // If our recipe has at least one slot specified, we use that - we agreed on ONE ongoing slot only
            ongoing.SetupSlot(recipe);
            ongoing.gameObject.SetActive(true);

            results.gameObject.SetActive(false);

            DisplayRecipeHint(null); // TODO: Start showing timer instead
            DisplayButtonState(false);
        }

        // Results State

        public void SetOutput(List<IElementStack> stacks) {
            results.SetOutput(stacks);
 
        }

        public void SetComplete() {
            startingSlots.gameObject.SetActive(false);
            ongoing.gameObject.SetActive(false);
            results.gameObject.SetActive(true);
            aspectsDisplay.ClearAspects();
        }

        // SHOW VIZ

        public void DisplayUnstarted() {
            Title = Verb.Label;
            PaginatedNotes.SetText(Verb.Description);
            DisplayRecipeHint(null);
            DisplayButtonState(false);
        }

		public void DisplayNoRecipeFound() {
			Title = Verb.Label;
			PaginatedNotes.SetText(Verb.Description);
			DisplayRecipeHint("This does nothing. If I experiment further, I may find another combination.");
			DisplayButtonState(false);
		}

        public void AddNote(INotification notification)
        {
            PaginatedNotes.AddText(notification.Description);
        }

        public void DisplayStartingRecipeFound(Recipe r) {
			Title = r.Label;
			PaginatedNotes.SetText(r.StartDescription);
            DisplayTimeRemaining(r.Warmup, r.Warmup); //Ensures that the time bar is set to 0 to avoid a flicker
			DisplayRecipeHint(null);
			DisplayButtonState(true);

            SoundManager.PlaySfx("SituationAvailable");
        }

		public void UpdateTextForPrediction(RecipePrediction recipePrediction) {
			Title = recipePrediction.Title;
			PaginatedNotes.AddText(recipePrediction.DescriptiveText);
			DisplayRecipeHint(recipePrediction.Commentary);
			DisplayButtonState(false);
		}

		public void DisplayAspects(IAspectsDictionary forAspects) {
			aspectsDisplay.DisplayAspects(forAspects);
		}

        public void DisplayStoredElements() {
            ongoing.ShowStoredAspects(GetStoredStacks());
        }

        public void DisplayRecipeHint(string hint) {
            bool isActive = !string.IsNullOrEmpty(hint);
            hintText.gameObject.SetActive(isActive);

            if (!isActive)
                return;

            hintText.text = hint;
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining) {
            ongoing.UpdateTime(duration, timeRemaining);
        }

        void DisplayButtonState(bool interactable, string text = null) {
			startButton.interactable = interactable;
			startButtonText.text = string.IsNullOrEmpty(text) ? buttonDefault : text;
        }

        public void ShowDestinationsForStack(IElementStack stack) {
            IList<RecipeSlot> slots;

            slots = startingSlots.gameObject.activeInHierarchy ? startingSlots.GetAllSlots() : null;
            HighlightSlots(slots, stack);

            slots = ongoing.gameObject.activeInHierarchy ? ongoing.GetAllSlots() : null;
            HighlightSlots(slots, stack);
        }

        void HighlightSlots(IList<RecipeSlot> slots, IElementStack stack) {
            if (slots == null)
                return;

            foreach (var s in slots) {
                if (stack == null || s.GetSlotMatchForStack(stack).MatchType != SlotMatchForAspectsType.Okay || s.GetElementStackInSlot() != null)
                    s.ShowGlow(false, false);
                else
                    s.ShowGlow(true, false);
            }
        }

        // ACTIONS

        void HandleButtonAction() {
            // TODO: Could turn into clear all button on Completion?
            // TODO: Alternative one-click confirm on empty post-results window to reset?
            situationController.AttemptActivateRecipe();
        }
        
        /*
        // currently not in use
        public void DumpAllCardsToDesktop() {
            DumpToDesktop(GetStartingStacks());
            DumpToDesktop(GetOngoingStacks());
            // Don't dump stored stacks - they're supposed to be inaccessible
            DumpToDesktop(GetOutputStacks());
        }
        */

        public void DumpAllStartingCardsToDesktop() {
            if (situationController.Situation.State == SituationState.Unstarted)
                DumpToDesktop(GetStartingStacks());
        }

        void DumpToDesktop(IEnumerable<IElementStack> stacks) {
            DraggableToken token;

            foreach (var item in stacks) {
                token = item as DraggableToken;

                if (token != null)
                    token.ReturnToTabletop(null);
            }
        }

        // ISituationDetails

        public bool ConsumeMarkedElements(bool withAnim) {
            bool hasConsumed = false;
            var stacks = GetStoredStacks();

            for (int i = 0; i < stacks.Count(); i++) {
                if (stacks.ElementAt(i) != null && stacks.ElementAt(i).MarkedForConsumption) {
                    stacks.ElementAt(i).Retire(withAnim);
                    hasConsumed = true;
                }
            }

            return hasConsumed;
        }

        public IEnumerable<IElementStack> GetStartingStacks() {
            return startingSlots.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetOngoingStacks() {
            return ongoing.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetStoredStacks() {
            return GetStorageStacksManager().GetStacks();
        }

        public IEnumerable<IElementStack> GetOutputStacks() {
            return results.GetOutputStacks();
        }


        public IElementStacksManager GetStorageStacksManager() {
            return storage.GetElementStacksManager();
        }

        public void StoreStacks(IEnumerable<IElementStack> stacksToStore) {
            GetStorageStacksManager().AcceptStacks(stacksToStore);
        }



        public void SetSlotConsumptions() {
            foreach (var s in startingSlots.GetAllSlots())
                s.SetConsumption();
        }


        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo) {
            return startingSlots.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo) {
            return ongoing.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public IRecipeSlot GetUnfilledGreedySlot() {
            return ongoing.GetUnfilledGreedySlot();
        }


        public IAspectsDictionary GetAspectsFromStartingElements(bool showElementAspects) {
            return startingSlots.GetAspectsFromSlottedCards(showElementAspects);
        }

        public IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects) {
            var slottedAspects = new AspectsDictionary();
            slottedAspects.CombineAspects(startingSlots.GetAspectsFromSlottedCards(showElementAspects));
            slottedAspects.CombineAspects(ongoing.GetAspectsFromSlottedCards(showElementAspects));
            return slottedAspects;
        }

        public IAspectsDictionary GetAspectsFromStoredElements(bool showElementAspects) {
            return GetStorageStacksManager().GetTotalAspects(showElementAspects);
        }




        public IEnumerable<ISituationNote> GetNotes()
        {
            return PaginatedNotes.GetCurrentTexts();
        }

    }
}
