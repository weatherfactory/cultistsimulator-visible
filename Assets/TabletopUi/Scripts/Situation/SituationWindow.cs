#pragma warning disable 0649
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
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Services;

namespace Assets.CS.TabletopUI {
    [RequireComponent(typeof(SituationWindowPositioner))]
    public class SituationWindow : MonoBehaviour, ISituationDetails {

        string buttonDefault;
        string buttonBusy;

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

		[SerializeField] Button startButton;
		[SerializeField] TextMeshProUGUI startButtonText;

		private SituationController situationController;
		private Heart _heart;
		private IVerb Verb;
        private bool windowIsWide = false;

        public bool IsOpen {
            get { return gameObject.activeInHierarchy; }
        }

		public string Title {
			get { return title.text; }
			set { title.text = value; }
        }

		public Vector3 Position
		{
			get { return positioner.GetPosition(); }
			set { positioner.SetPosition( value ); }
		}
        // INIT & LIFECYCLE

        void OnEnable()
		{
			startButton.onClick.AddListener(HandleStartButton);
            dumpResultsButton.onClick.AddListener(DumpAllResultingCardsToDesktop);

			buttonDefault = Registry.Get<ILanguageManager>().Get("VERB_START");
			buttonBusy = Registry.Get<ILanguageManager>().Get("VERB_RUNNING");
        }

		void OnDisable() {
			startButton.onClick.RemoveListener(HandleStartButton);
            dumpResultsButton.onClick.RemoveListener(DumpAllResultingCardsToDesktop);
        }


        public void DisplayIcon(string icon)
        {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(icon);
            artwork.sprite = sprite;
        }

        public void Initialise(IVerb verb, SituationController sc, Heart heart) {
			situationController = sc;
			_heart = heart;
			Verb = verb;
            name = "Window_" + verb.Id;
            DisplayIcon(verb.Id);

            startingSlots.Initialise(sc);
            ongoing.Initialise(sc);
            results.Initialise(sc);
            storage.Initialise();
		}

        public void Retire()
        {
            var startingStacks = new List<IElementStack>(GetStartingStacks());
            foreach (var s in startingStacks)
                s.Retire(CardVFX.None);
            var ongoingStacks=new List<IElementStack>(GetOngoingStacks());
           foreach (var o in ongoingStacks)
               o.Retire(CardVFX.None);


            GetStorageStacksManager().RemoveAllStacks();
            GetResultsStacksManager().RemoveAllStacks();
            Destroy(gameObject);
        }

        // to be accessable from Close Button
        public void Close() {
            situationController.CloseWindow();
        }

        // BASIC DISPLAY

        public void Show( Vector3 targetPosOverride )
		{
			if (!gameObject.activeInHierarchy)
			{
				SoundManager.PlaySfx("SituationWindowShow");
			}

			canvasGroupFader.Show();
            positioner.Show(canvasGroupFader.durationTurnOn, targetPosOverride); // Animates the window (position allows optional change is position)
            results.UpdateDumpButtonText(); // ensures that we've updated the dump button accordingly
            startingSlots.ArrangeSlots(); //won't have been arranged if a card was dumped in while the window was closed
 			PaginatedNotes.Reset();
        }

		public void Hide() {
			if (gameObject.activeInHierarchy)
				SoundManager.PlaySfx("SituationWindowHide");

			canvasGroupFader.Hide();
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
            ongoing.ShowDeckEffects(recipe.DeckEffects);
            ongoing.gameObject.SetActive(true);

            results.gameObject.SetActive(false);

            DisplayButtonState(false, buttonBusy);
            
            SetWindowSize(false); //always collapse the window if we don't need to display multiple slots

			_heart.AdvanceTime( 0.0f );	// Force a refresh of desktop without actually advancing time, so that new timer will appear on verb token - CP
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

            results.UpdateDumpButtonText();
        }

        // SHOW VIZ

        public void DisplayUnstarted() {
            Title = Verb.Label;
            PaginatedNotes.SetText(Verb.Description);
            DisplayButtonState(false);
            SetWindowSize(false);
        }

		public void DisplayNoRecipeFound() {
			Title = Verb.Label;
			PaginatedNotes.SetText(Verb.Description);
            
			DisplayButtonState(false);
        }

        public void DisplayStartingRecipeFound(Recipe r) {


            Title = r.Label;
            //Check for possible text refinements based on the aspects in context
            var aspectsInSituation = GetAspectsFromAllSlottedElements(true);
            TextRefiner tr = new TextRefiner(aspectsInSituation);
            PaginatedNotes.SetText(tr.RefineString(r.StartDescription));

            DisplayTimeRemaining(r.Warmup, r.Warmup, r.SignalEndingFlavour); //Ensures that the time bar is set to 0 to avoid a flicker
			DisplayButtonState(true);

            SoundManager.PlaySfx("SituationAvailable");
        }

        public void DisplayHintRecipeFound(Recipe r)
		{
            Title = Registry.Get<ILanguageManager>().Get("UI_HINT") + " " + r.Label;
            //Check for possible text refinements based on the aspects in context
            var aspectsInSituation = GetAspectsFromAllSlottedElements(true);
            TextRefiner tr = new TextRefiner(aspectsInSituation);

            PaginatedNotes.SetText("<i>" + tr.RefineString(r.StartDescription) + "</i>");
            DisplayButtonState(false);
            

            SoundManager.PlaySfx("SituationAvailable");
        }

       public void SetWindowSize(bool wide) {
            RectTransform rectTrans = transform as RectTransform;

            if (wide)
                rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 900f);
            else
                rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 700f);

            if (wide != windowIsWide) {
                if (wide)
                    rectTrans.anchoredPosition = rectTrans.anchoredPosition + new Vector2(100f, 0f);
                else
                    rectTrans.anchoredPosition = rectTrans.anchoredPosition - new Vector2(100f, 0f);

                startingSlots.SetGridNumPerRow(); // Updates the grid row numbers
                ongoing.SetSlotToPos(); // Updates the ongoing slot position
            }

            windowIsWide = wide;

           startingSlots.ArrangeSlots();
        }

        public void ReceiveTextNote(INotification notification) {
            PaginatedNotes.AddText(notification.Description);
        }

        public void UpdateTextForPrediction(RecipePrediction recipePrediction) {
			Title = recipePrediction.Title;
			PaginatedNotes.AddText(recipePrediction.DescriptiveText);

        }


        public void DisplayAspects(IAspectsDictionary forAspects) {
			aspectsDisplay.DisplayAspects(forAspects);
		}

        public void DisplayStoredElements() {
            ongoing.ShowStoredAspects(GetStoredStacks());
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour forEndingFlavour) {
            ongoing.UpdateTime(duration, timeRemaining, forEndingFlavour);
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
                return false; // Slot is being animated? Don't highlight
			if (slot.IsGreedy)
				return false; // Slot is greedy? It can never take anything.
            if (stack.EntityId == "dropzone")
                return false; // Dropzone can never be put in a slot

            return slot.GetSlotMatchForStack(stack).MatchType == SlotMatchForAspectsType.Okay;
        }

        // ACTIONS

        void HandleStartButton() {
            situationController.AttemptActivateRecipe();
        }

        // so the token-dump button can trigger this
        public void DumpAllResultingCardsToDesktop() {
			var results = GetOutputStacks();

			DumpToDesktop(results, new Context(Context.ActionSource.PlayerDumpAll));
			situationController.ResetSituation();

			// Only play collect all if there's actually something to collect 
			// Only play collect all if it's not transient - cause that will retire it and play the retire sound
			// Note: If we collect all from the window we also get the default button sound in any case.
			if (results.Count() > 0)
				SoundManager.PlaySfx("SituationCollectAll");
			else if (situationController.situationToken.IsTransient)
				SoundManager.PlaySfx("SituationTokenRetire");
			else 
				SoundManager.PlaySfx("UIButtonClick");
        }

        public void DumpAllStartingCardsToDesktop() {
            if (situationController.SituationClock.State == SituationState.Unstarted)
                DumpToDesktop(GetStartingStacks(), new Context(Context.ActionSource.PlayerDumpAll));
        }

        void DumpToDesktop(IEnumerable<IElementStack> stacks, Context context) {
            DraggableToken token;

            foreach (var item in stacks) {
                token = item as DraggableToken;

                if (token != null)
                {       
                    if(token == DraggableToken.itemBeingDragged)
                    {
                        DraggableToken.CancelDrag();
                    }

                    token.ReturnToTabletop(context);
                }
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
        public IElementStack ReprovisionExistingElementStackInStorage(ElementStackSpecification stackSpecification, Source stackSource, string locatorid = null)
        {
            return storage.ReprovisionExistingElementStack(stackSpecification, stackSource, locatorid);
        }

        public IElementStacksManager GetResultsStacksManager() {
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


        public RecipeSlot GetPrimarySlot() {
            return startingSlots.GetAllSlots().FirstOrDefault();
        }

        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo) {
            return startingSlots.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo) {
            return ongoing.GetSlotBySaveLocationInfoPath(locationInfo);
        }


        public IList<RecipeSlot> GetStartingSlots() {
            return startingSlots.GetAllSlots();
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

        public void TryDecayResults(float interval)
        {
            var stacksToDecay = GetResultsStacksManager().GetStacks();
            foreach(var s in stacksToDecay)
                s.Decay(interval);
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
            return GetResultsStacksManager().GetTotalAspects(includeElementAspects);
        }



        public IEnumerable<ISituationNote> GetNotes() {
            return PaginatedNotes.GetCurrentTexts();
        }

    }
}
