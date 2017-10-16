using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.SlotsContainers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI {
	public class SituationWindowNew : MonoBehaviour, ISituationDetails {

		//TEMP TO TEST
		public RecipeSlot slotPrefab;


		[Header("Visuals")]
		[SerializeField] CanvasGroupFader canvasGroupFader;

		[Space]
		[SerializeField] TextMeshProUGUI title;
		public PaginatedText notes;

		[Space]
		public SituationSlotManager slotManager;
		[SerializeField] AspectsDisplay aspectsDisplay;

		[Space]
		[SerializeField] TextMeshProUGUI hintText;
		[SerializeField] GameObject hintTimer;
		[SerializeField] RectTransform hintTimerBar;
		[SerializeField] TextMeshProUGUI hintTimerText;

		[SerializeField] Button startButton;
		[SerializeField] TextMeshProUGUI startButtonText;

		private SituationController situationController;
		private IVerb Verb;

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

        // INIT

		void OnEnable() {
			startButton.onClick.AddListener(StartRecipe);
		}

		void OnDisable() {
			startButton.onClick.RemoveListener(StartRecipe);
		}


		public void Initialise(IVerb verb, SituationController sc) {
			situationController = sc;
			Verb = verb;
		}

		// BASIC STATUS

		public void Show() {
			canvasGroupFader.Show();
		}

		public void Hide() {
			canvasGroupFader.Hide();
		}

		public void SetUnstarted() {
			/*
			startingSlotsContainer.Reset();

			startingSlotsContainer.gameObject.SetActive(true);
			ongoingSlotsContainer.gameObject.SetActive(false);

			_situationResults.Reset();
			_situationResults.gameObject.SetActive(false);
			*/
			Title = Verb.Label;
			notes.SetText(Verb.Description);
			ShowRecipeHint(null);
			DisplayButtonState(false);
		}

		public void SetOngoing(Recipe forRecipe) {
			/*
			startingSlotsContainer.gameObject.SetActive(false);
			ongoingSlotsContainer.gameObject.SetActive(true);
			_situationResults.gameObject.SetActive(false);

			ongoingSlotsContainer.SetUpSlots(forRecipe.SlotSpecifications);
			*/
			ShowRecipeHint(null); // TODO: Start showing timer instead
			DisplayButtonState(false);
			ConsumeMarkedElements();
		}

		public void SetComplete() {
			/*
			startingSlotsContainer.gameObject.SetActive(false);
			ongoingSlotsContainer.gameObject.SetActive(false);
			_situationResults.gameObject.SetActive(true);
			*/

			aspectsDisplay.ClearAspects();
		}

        // SHOW VIZ

		public void ShowRecipeHint(string hint) {
			bool isActive = !string.IsNullOrEmpty(hint);
			hintText.gameObject.SetActive(isActive);

			if (!isActive)
				return;
			
			hintText.text = hint;
			hintTimer.gameObject.SetActive(false);
		}

		public void DisplayTimeRemaining(float duration, float timeRemaining) {
			hintText.gameObject.SetActive(false);
			hintTimer.gameObject.SetActive(true);

			hintTimerBar.anchorMax = new Vector2(timeRemaining / duration, hintTimerBar.anchorMax.y);
			hintTimerText.text = timeRemaining.ToString("0.0") + "s";
		}

		public void DisplayNoRecipeFound() {
			Title = Verb.Label;
			notes.SetText(Verb.Description);
			ShowRecipeHint("This does not work. If I experiment further, I may find another combination.");
			DisplayButtonState(false);
		}

		public void DisplayStartingRecipeFound(Recipe r) {
			Title = r.Label;
			notes.SetText(r.StartDescription);
			ShowRecipeHint(null);
			DisplayButtonState(true);
		}

		public void UpdateTextForPrediction(RecipePrediction recipePrediction) {
			Title = recipePrediction.Title;
			notes.SetText(recipePrediction.DescriptiveText);
			ShowRecipeHint(recipePrediction.Commentary);
			DisplayButtonState(false);
		}

		public void DisplayAspects(IAspectsDictionary forAspects) {
			aspectsDisplay.DisplayAspects(forAspects);
		}

		void DisplayButtonState(bool interactable, string text = null) {
			startButton.interactable = interactable;
			startButtonText.text = string.IsNullOrEmpty(text) ? "Start" : text;
		}

		// ACTIONS

		void StartRecipe() {

		}



        // ISituationDetails

        public void ConsumeMarkedElements() {
            throw new NotImplementedException();
        }

        public AspectsDictionary GetAspectsFromAllSlottedElements() {
            throw new NotImplementedException();
        }

        public IAspectsDictionary GetAspectsFromStoredElements() {
            throw new NotImplementedException();
        }

        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo) {
            throw new NotImplementedException();
        }

        public IEnumerable<ISituationOutputNote> GetOutputNotes() {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetOutputStacks() {
            throw new NotImplementedException();
        }

        public ElementStacksManager GetSituationStorageStacksManager() {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetStacksInOngoingSlots() {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetStacksInStartingSlots() {
            throw new NotImplementedException();
        }

        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo) {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetStoredStacks() {
            throw new NotImplementedException();
        }

        public IRecipeSlot GetUnfilledGreedySlot() {
            throw new NotImplementedException();
        }

        public void Retire() {
            throw new NotImplementedException();
        }

        public void SetOutput(List<IElementStack> stacks, INotification notification) {
            throw new NotImplementedException();
        }

        public void SetSlotConsumptions() {
            throw new NotImplementedException();
        }

        public void ShowDestinationsForStack(IElementStack stack) {
            throw new NotImplementedException();
        }

        public void StoreStacks(IEnumerable<IElementStack> stacksToStore) {
            throw new NotImplementedException();
        }

    }
}
