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
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.CS.TabletopUI {
    [RequireComponent(typeof(SituationWindowPositioner))]
    public class SituationWindow : MonoBehaviour,ISituationView,ISituationWindowAsStorage {

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

            buttonDefault = "VERB_START";
			buttonBusy = "VERB_RUNNING";
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

        public void Initialise(IVerb verb, SituationController sc) {
			situationController = sc;
			Verb = verb;
            name = "Window_" + verb.Id;
            DisplayIcon(verb.Id);

            if (Verb.Startable)
            {
                startButton.gameObject.SetActive(true);
            }
            else
            {
                startButton.gameObject.SetActive(false);
            }



            startingSlots.Initialise(Verb, sc);
            ongoing.Initialise(Verb,sc);
            results.Initialise(sc);
            storage.Initialise();
		}

        public void Retire()
        {
            var startingStacks = new List<ElementStackToken>(GetStartingStacks());
            foreach (var s in startingStacks)
                s.Retire(CardVFX.None);
            var ongoingStacks=new List<ElementStackToken>(GetOngoingStacks());
           foreach (var o in ongoingStacks)
               o.Retire(CardVFX.None);


           storage.RemoveAllStacks();
            results.RemoveAllStacks();
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

        }

        // Results State

        public void SetOutput(List<ElementStackToken> stacks) {
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
            Title = Registry.Get<ILocStringProvider>().Get("UI_HINT") + " " + r.Label;
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
            startButtonText.GetComponent<Babelfish>().UpdateLocLabel(string.IsNullOrEmpty(text) ? buttonDefault : text);
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
			else if (situationController.situationAnchor.IsTransient)
				SoundManager.PlaySfx("SituationTokenRetire");
			else 
				SoundManager.PlaySfx("UIButtonClick");
        }

        public void DumpAllStartingCardsToDesktop() {
            if (situationController.Situation.State == SituationState.Unstarted)
                DumpToDesktop(GetStartingStacks(), new Context(Context.ActionSource.PlayerDumpAll));
        }

        void DumpToDesktop(IEnumerable<ElementStackToken> stacks, Context context) {

            foreach (var item in stacks) {
                item.ReturnToTabletop(context);
                
            }
        }

        // ISituationDetails

        public IEnumerable<ElementStackToken> GetStartingStacks() {
            return startingSlots.GetStacksInSlots();
        }

        public IEnumerable<ElementStackToken> GetOngoingStacks() {
            return ongoing.GetStacksInSlots();
        }

        public IEnumerable<ElementStackToken> GetStoredStacks() {
            return storage.GetStacks();
        }

        public IEnumerable<ElementStackToken> GetOutputStacks() {
            return results.GetOutputStacks();
        }


        public ElementStackToken ReprovisionExistingElementStackInStorage(ElementStackSpecification stackSpecification, Source stackSource, string locatorid = null)
        {
            return storage.ReprovisionExistingElementStack(stackSpecification, stackSource, new Context(Context.ActionSource.Loading), locatorid);
        }



        public void StoreStacks(IEnumerable<ElementStackToken> stacksToStore)
        {
            storage.AcceptStacks(stacksToStore, new Context(Context.ActionSource.SituationStoreStacks));
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

        public SituationStorage GetStorageContainer()
        {
            return storage;
        }

        public SituationResults GetResultsContainer()
        {
            return results;
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
            var stacksToDecay = results.GetStacks();
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
            return storage.GetTotalAspects(includeElementAspects);
        }

        public IAspectsDictionary GetAspectsFromOutputElements(bool includeElementAspects) {
            return results.GetTotalAspects(includeElementAspects);
        }



        public IEnumerable<ISituationNote> GetNotes() {
            return PaginatedNotes.GetCurrentTexts();
        }

    }
}
