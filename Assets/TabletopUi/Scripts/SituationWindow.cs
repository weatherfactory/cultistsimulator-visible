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
    public class SituationWindow : MonoBehaviour, ISituationDetails {

        private enum State { Starting, TransitionToOngoing, Ongoing, TransitionToResults, Results, TransitionToStarting }
        private State currentState = State.Starting;

        const string buttonDefault = "Start";
        const string buttonBusy = "Waiting...";

		//TEMP TO TEST
		public RecipeSlot slotPrefab;

		[Header("Visuals")]
		[SerializeField] CanvasGroupFader canvasGroupFader;

		[Space]
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;
		public PaginatedText notes;

		[Space]
        // Public to test
		public SituationSlotManager slotManager;
        [SerializeField] StartingSlotsContainer startingSlots;

        [Space]
        [SerializeField] OngoingSlotManager ongoing;

        [Space]
        [SerializeField] SituationResults results;

        [Space]
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] TextMeshProUGUI hintText;

		[SerializeField] Button startButton;
		[SerializeField] TextMeshProUGUI startButtonText;

		private SituationController situationController;
		private IVerb Verb;

        public bool IsOpen {
            get { return gameObject.activeInHierarchy; }
        }

		public string Title {
			get { return title.text; }
			set { title.text = value; }
        }

        public string Description {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
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

            startingSlots.Initialise(sc);
            ongoing.Initialise(sc);
            results.Initialise(this, sc);
		}

        public void Retire() {
            Destroy(gameObject);
        }

        // BASIC DISPLAY

        public void Show() {
			canvasGroupFader.Show();
		}

		public void Hide() {
			canvasGroupFader.Hide();
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
            currentState = State.Starting;
        }

        // Ongoing State

		public void SetOngoing(Recipe recipe) {
            if (IsOpen)
                StartCoroutine(TransitionToOngoingState(recipe));
            else
                DisplayOngoingState(recipe);
        }

        IEnumerator TransitionToOngoingState(Recipe recipe) {
            currentState = State.TransitionToOngoing;
            bool hasConsumed = ConsumeMarkedElements(true);
            ongoing.SetupSlot(recipe);

            if (hasConsumed)
                yield return new WaitForSeconds(1f);

            startingSlots.canvasGroupFader.Hide();

            yield return new WaitForSeconds(startingSlots.canvasGroupFader.durationTurnOff);

            ongoing.canvasGroupFader.Show();
            DisplayRecipeHint(null); // TODO: Start showing timer instead
            DisplayButtonState(false);
            currentState = State.Ongoing;

            // TODO: WARNING IF THIS IS INTERRUPTED THE WINDOW IS FUCKED
        }

        void DisplayOngoingState(Recipe recipe) {
            currentState = State.Ongoing;
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

        public void SetOutput(List<IElementStack> stacks, INotification notification) {
                results.SetOutput(stacks);
        }

        public void SetComplete() {
            if (IsOpen)
                StartCoroutine(TransitionToResultsState());
            else
                DisplayResults();
        }

        IEnumerator TransitionToResultsState() {
            currentState = State.TransitionToResults;
            ongoing.canvasGroupFader.Hide();

            yield return new WaitForSeconds(ongoing.canvasGroupFader.durationTurnOff);

            results.canvasGroupFader.Show();
            currentState = State.Results;

            // TODO: WARNING IF THIS IS INTERRUPTED THE WINDOW IS FUCKED
        }

        public void DisplayResults() {
            currentState = State.Results;
            startingSlots.gameObject.SetActive(false);
            ongoing.gameObject.SetActive(false);
            results.gameObject.SetActive(true);
            aspectsDisplay.ClearAspects();
        }

        // SHOW VIZ

        // SHOW VIZ

        public void DisplayUnstarted() {
            artwork.sprite = ResourcesManager.GetSpriteForVerbLarge(Verb.Id);
            Title = Verb.Label;
            notes.SetText(Verb.Description);
            DisplayRecipeHint(null);
            DisplayButtonState(false);
        }

		public void DisplayNoRecipeFound() {
			Title = Verb.Label;
			notes.SetText(Verb.Description);
			DisplayRecipeHint("This does not work. If I experiment further, I may find another combination.");
			DisplayButtonState(false);
		}

		public void DisplayStartingRecipeFound(Recipe r) {
			Title = r.Label;
			notes.SetText(r.StartDescription);
			DisplayRecipeHint(null);
			DisplayButtonState(true);
		}

		public void UpdateTextForPrediction(RecipePrediction recipePrediction) {
			Title = recipePrediction.Title;
			notes.SetText(recipePrediction.DescriptiveText);
			DisplayRecipeHint(recipePrediction.Commentary);
			DisplayButtonState(false);
		}

		public void DisplayAspects(IAspectsDictionary forAspects) {
			aspectsDisplay.DisplayAspects(forAspects);
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
                if (stack == null || s.GetSlotMatchForStack(stack).MatchType != SlotMatchForAspectsType.Okay)
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
        
        // ISituationDetails

        public bool ConsumeMarkedElements(bool withAnim) {
            bool hasConsumed = false;

            var stacks = GetStacksInStartingSlots();

            for (int i = 0; i < stacks.Count(); i++) {
                if (stacks.ElementAt(i) != null && stacks.ElementAt(i).MarkedForConsumption) {
                    stacks.ElementAt(i).Retire(withAnim);
                    hasConsumed = true;
                }
            }

            return hasConsumed;
        }

        public IEnumerable<IElementStack> GetStacksInStartingSlots() {
            return startingSlots.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetStacksInOngoingSlots() {
            return ongoing.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetOutputStacks() {
            return results.GetOutputStacks();
        }


        public IElementStacksManager GetStartingSlotStacksManager() {
            return startingSlots.GetElementStacksManager();
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


        public IAspectsDictionary GetAspectsFromStartingElements() {
            return startingSlots.GetAspectsFromSlottedCards();
        }

        public IAspectsDictionary GetAspectsFromAllSlottedElements() {
            var slottedAspects = new AspectsDictionary();
            slottedAspects.CombineAspects(startingSlots.GetAspectsFromSlottedCards());
            slottedAspects.CombineAspects(ongoing.GetAspectsFromSlottedCards());
            return slottedAspects;
        }



        public IEnumerable<ISituationOutputNote> GetOutputNotes() {
            throw new NotImplementedException();
        }

    }
}
