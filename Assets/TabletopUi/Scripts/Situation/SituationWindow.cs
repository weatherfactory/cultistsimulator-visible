﻿#pragma warning disable 0649
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
        [SerializeField] StartingSlotsManager startingSlots;

        [Space]
        [SerializeField] OngoingSlotManager ongoing;

        [Space]
        [SerializeField] SituationResults results;
		[SerializeField] Button dumpResultsButton;
        [SerializeField] TextMeshProUGUI dumpResultsButtonText;

        [Space]
        [SerializeField] SituationStorage storage;

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

        // INIT & LIFECYCLE

        void OnEnable() {
			startButton.onClick.AddListener(HandleStartButton);
            dumpResultsButton.onClick.AddListener(DumpAllResultingCardsToDesktop);
        }

		void OnDisable() {
			startButton.onClick.RemoveListener(HandleStartButton);
            dumpResultsButton.onClick.RemoveListener(DumpAllResultingCardsToDesktop);
        }

		public void Initialise(IVerb verb, SituationController sc) {
			situationController = sc;
			Verb = verb;
            name = "Window_" + verb.Id;
            artwork.sprite = ResourcesManager.GetSpriteForVerbLarge(Verb.Id);

            startingSlots.Initialise(sc);
            ongoing.Initialise(sc);
            results.Initialise(sc);
            storage.Initialise();
		}

        public void Retire() {
            Destroy(gameObject);
        }

        // to be accessable from Close Button
        public void Close() {
            situationController.CloseWindow();
        }

        // BASIC DISPLAY

        public void Show() {
			canvasGroupFader.Show();
            positioner.Show(canvasGroupFader.durationTurnOn); // Animates the window
            SoundManager.PlaySfx("SituationWindowShow");
            results.UpdateDumpButtonText(); // ensures that we've updated the dump button accordingly
        }

		public void Hide() {
			canvasGroupFader.Hide();
            SoundManager.PlaySfx("SituationWindowHide");
        }

        // Start State

		public void SetUnstarted() {
            startingSlots.DoReset();
            startingSlots.gameObject.SetActive(true);

            ongoing.DoReset();
            ongoing.gameObject.SetActive(false);

            results.DoReset();
            results.gameObject.SetActive(false);

            DisplayUnstarted();
        }

        // Ongoing State

		public void SetOngoing(Recipe recipe) {
            startingSlots.gameObject.SetActive(false);

            // If our recipe has at least one slot specified, we use that - only one slot supported for now (and maybe forever)
            ongoing.SetupSlot(recipe);
            ongoing.gameObject.SetActive(true);

            results.gameObject.SetActive(false);

            DisplayRecipeMetaComment(null); // TODO: Start showing timer instead
            DisplayButtonState(false, buttonBusy);
        }

        // Results State

        public void SetOutput(List<IElementStack> stacks) {
            results.SetOutput(stacks);
 
        }

        public void SetComplete() {
            startingSlots.gameObject.SetActive(false);
            ongoing.gameObject.SetActive(false);
            results.gameObject.SetActive(true);
            aspectsDisplay.ClearCurrentlyDisplayedAspects();

            hintText.text = "";
            results.UpdateDumpButtonText();
        }

        // SHOW VIZ

        public void DisplayUnstarted() {
            Title = Verb.Label;
            PaginatedNotes.SetText(Verb.Description);
            DisplayRecipeMetaComment(null);
            DisplayButtonState(false);
        }

		public void DisplayNoRecipeFound() {
			Title = Verb.Label;
			PaginatedNotes.SetText(Verb.Description);
			DisplayRecipeMetaComment("This does nothing. If I experiment further, I may find another combination.");
			DisplayButtonState(false);
		}

        public void DisplayStartingRecipeFound(Recipe r) {
			Title = r.Label;
			PaginatedNotes.SetText(r.StartDescription);
            DisplayTimeRemaining(r.Warmup, r.Warmup, r); //Ensures that the time bar is set to 0 to avoid a flicker
			DisplayRecipeMetaComment(null);
			DisplayButtonState(true);

            SoundManager.PlaySfx("SituationAvailable");
        }

        public void DisplayHintRecipeFound(Recipe r) {
            Title = r.Label;
            PaginatedNotes.SetText("<i>" + r.StartDescription + "</i>");
            DisplayRecipeMetaComment(null);
            DisplayButtonState(false);

            SoundManager.PlaySfx("SituationAvailable");
        }

        public void ReceiveTextNote(INotification notification) {
            PaginatedNotes.AddText(notification.Description);
        }

        public void UpdateTextForPrediction(RecipePrediction recipePrediction) {
			Title = recipePrediction.Title;
			PaginatedNotes.AddText(recipePrediction.DescriptiveText);
			DisplayRecipeMetaComment(recipePrediction.Commentary);
			//DisplayButtonState(false);
        }

        public void DisplayRecipeMetaComment(string hint) {
            bool isActive = !string.IsNullOrEmpty(hint);
            hintText.gameObject.SetActive(isActive);

            if (!isActive)
                return;

            hintText.text = hint;
        }

        public void DisplayAspects(IAspectsDictionary forAspects) {
			aspectsDisplay.DisplayAspects(forAspects);
		}

        public void DisplayStoredElements() {
            ongoing.ShowStoredAspects(GetStoredStacks());
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, Recipe recipe) {
            ongoing.UpdateTime(duration, timeRemaining,recipe);
        }

        void DisplayButtonState(bool interactable, string text = null) {
			startButton.interactable = interactable;
			startButtonText.text = string.IsNullOrEmpty(text) ? buttonDefault : text;
        }

        public void ShowDestinationsForStack(IElementStack stack, bool show) {
            IList<RecipeSlot> slots;

            slots = startingSlots.gameObject.activeInHierarchy ? startingSlots.GetAllSlots() : null;
            HighlightSlots(slots, show ? stack : null);

            slots = ongoing.gameObject.activeInHierarchy ? ongoing.GetAllSlots() : null;
            HighlightSlots(slots, show ? stack : null);
        }

        void HighlightSlots(IList<RecipeSlot> slots, IElementStack stack) {
            if (slots == null)
                return;

            foreach (var s in slots) {
                if (CanHighlightSlot(s, stack))
                    s.ShowGlow(true, false);
                else
                    s.ShowGlow(false, false);
            }
        }

        bool CanHighlightSlot(RecipeSlot slot, IElementStack stack) {
            if (stack == null || slot == null)
                return false; 
            if (slot.GetElementStackInSlot() != null)
                return false; // Slot is filled? Don't highlight it as interactive
            if (slot.IsBeingAnimated)
                return false; // Slot is being animated? Don't hihglight

            return slot.GetSlotMatchForStack(stack).MatchType == SlotMatchForAspectsType.Okay;
        }

        // ACTIONS

        void HandleStartButton() {
            situationController.AttemptActivateRecipe();
        }

        // so the token-dump button can trigger this
        public void DumpAllResultingCardsToDesktop() {
            DumpToDesktop(GetOutputStacks(), new Context(Context.ActionSource.PlayerClick));
            situationController.ResetSituation();
        }

        public void DumpAllStartingCardsToDesktop() {
            if (situationController.SituationClock.State == SituationState.Unstarted)
                DumpToDesktop(GetStartingStacks(), new Context(Context.ActionSource.PlayerClick));
        }

        void DumpToDesktop(IEnumerable<IElementStack> stacks, Context context) {
            DraggableToken token;

            foreach (var item in stacks) {
                token = item as DraggableToken;

                if (token != null)
                    token.ReturnToTabletop(context);
            }
        }

        // ISituationDetails

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

        public IElementStacksManager GetOutputStacksManager() {
            return results.GetElementStacksManager();
        }

        public void StoreStacks(IEnumerable<IElementStack> stacksToStore) {
            GetStorageStacksManager().AcceptStacks(stacksToStore, new Context(Context.ActionSource.SituationStoreStacks));
            // Now that we've stored stacks, make sure we update the starting slots
            startingSlots.RemoveAnyChildSlotsWithEmptyParent(new Context(Context.ActionSource.SituationStoreStacks)); 
        }



        public void SetSlotConsumptions() {
            foreach (var s in startingSlots.GetAllSlots())
                s.SetConsumption();

            foreach (var o in ongoing.GetAllSlots())
                o.SetConsumption();
        }


        public IList<RecipeSlot> GetStartingSlots() {
            return startingSlots.GetAllSlots();
        }

        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo) {
            return startingSlots.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo) {
            return ongoing.GetSlotBySaveLocationInfoPath(locationInfo);
        }


        public IList<RecipeSlot> GetOngoingSlots() {
            return ongoing.GetAllSlots();
        }

        public IRecipeSlot GetUnfilledGreedySlot() {
            return ongoing.GetUnfilledGreedySlot();
        }



        public IAspectsDictionary GetAspectsFromAllSlottedAndStoredElements(bool includeElementAspects)
        {
            var aspects = GetAspectsFromAllSlottedElements(includeElementAspects);
            var storedAspects = GetAspectsFromStoredElements(includeElementAspects);
            aspects.CombineAspects(storedAspects);
            return aspects;
        }

        public IAspectsDictionary GetAspectsFromStartingElements(bool includeElementAspects) {
            return startingSlots.GetAspectsFromSlottedCards(includeElementAspects);
        }

        public IAspectsDictionary GetAspectsFromAllSlottedElements(bool includeElementAspects) {
            var slottedAspects = new AspectsDictionary();
            slottedAspects.CombineAspects(startingSlots.GetAspectsFromSlottedCards(includeElementAspects));
            slottedAspects.CombineAspects(ongoing.GetAspectsFromSlottedCards(includeElementAspects));
            return slottedAspects;
        }

        public IAspectsDictionary GetAspectsFromStoredElements(bool includeElementAspects) {
            return GetStorageStacksManager().GetTotalAspects(includeElementAspects);
        }

        public IAspectsDictionary GetAspectsFromOutputElements(bool includeElementAspects) {
            return GetOutputStacksManager().GetTotalAspects(includeElementAspects);
        }



        public IEnumerable<ISituationNote> GetNotes() {
            return PaginatedNotes.GetCurrentTexts();
        }

    }
}
