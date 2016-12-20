using System.Collections.Generic;
using System.Linq;
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

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class SituationWindow : MonoBehaviour,ISituationDetails, IDropHandler
    {

        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;

        [SerializeField] StartingSlotsContainer startingSlotsContainer;
        [SerializeField] OngoingSlotsContainer ongoingSlotsContainer;
        [SerializeField] SituationStorage situationStorage;
        [SerializeField] SituationOutputContainer outputContainer;

        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;
        [SerializeField] private TextMeshProUGUI NextRecipe;
        [SerializeField] private TabletopContainer tabletopContainer;
        public IList<INotification> queuedNotifications = new List<INotification>();
        private SituationController situationController;
        private IVerb Verb;

        void OnEnable()
        {
            button.onClick.AddListener(HandleOnButtonClicked);
        }
        void OnDisable()
        {
            button.onClick.RemoveListener(HandleOnButtonClicked);
        }


        public void Initialise(IVerb verb, SituationController sc)
        {

            situationController = sc;
            Verb = verb;
            startingSlotsContainer.Initialise(situationController);
            ongoingSlotsContainer.Initialise(situationController);
            DisplayStarting();
        }

        public void Show()
        {
            canvasGroupFader.Show();

        }

        public void Hide()
        {
            canvasGroupFader.Hide();
        }

        public void DisplayStarting()
        {

            startingSlotsContainer.gameObject.SetActive(true);
            ongoingSlotsContainer.gameObject.SetActive(false);
            outputContainer.gameObject.SetActive(false);
            
            title.text = Verb.Label;
            description.text = Verb.Description;
            NextRecipe.text = "";

            NextRecipe.gameObject.SetActive(false);
        }

        public void DisplayOngoing(Recipe forRecipe) {

            startingSlotsContainer.gameObject.SetActive(false);
            ongoingSlotsContainer.gameObject.SetActive(true);
            outputContainer.gameObject.SetActive(false);

            ongoingSlotsContainer.SetUpSlots(forRecipe.SlotSpecifications);
           

            button.gameObject.SetActive(false);
            NextRecipe.gameObject.SetActive(true);
        }

        public void DisplayComplete()
        {
            startingSlotsContainer.gameObject.SetActive(false);
            ongoingSlotsContainer.gameObject.SetActive(false);
            outputContainer.gameObject.SetActive(true);

            aspectsDisplay.ClearAspects();
            
        }

        public void DisplayAspects(IAspectsDictionary forAspects)
        {
            aspectsDisplay.DisplayAspects(forAspects);
        }

        public void DisplayRecipe(Recipe r)
        {
            if (r != null)
            {
                title.text = r.Label;
                description.text = r.StartDescription;
                button.gameObject.SetActive(true);
            }
            else
            {
                title.text = "";
                description.text = "";
                button.gameObject.SetActive(false);
            }
        }

        void HandleOnButtonClicked()
        {

            situationController.AttemptActivateRecipe();
 
        }

        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo)
        {
            return
                startingSlotsContainer.GetSlotBySaveLocationInfoPath(locationInfo);

        }

        public IEnumerable<IElementStack> GetStacksInStartingSlots()
        {
            return startingSlotsContainer.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetStacksInOngoingSlots()
        {
            return ongoingSlotsContainer.GetStacksInSlots();
        }

        public AspectsDictionary GetAspectsFromAllSlottedElements()
        {
            var slottedAspects=new AspectsDictionary();
            slottedAspects.CombineAspects(startingSlotsContainer.GetAspectsFromSlottedCards());
            slottedAspects.CombineAspects(ongoingSlotsContainer.GetAspectsFromSlottedCards());

            return slottedAspects;
        }

        public IEnumerable<ISituationOutput> GetCurrentOutputs()
        {
            return outputContainer.GetCurrentOutputs();
        }

        public IList<IRecipeSlot> GetUnfilledGreedySlots()
        {

            return ongoingSlotsContainer.GetUnfilledGreedySlots();
        }


        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo)
        {
            return ongoingSlotsContainer.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public void RunSlotConsumptions()
        {
            foreach (var s in startingSlotsContainer.GetAllSlots())
                s.RunConsumption();

        }

        public void AddOutput(IEnumerable<IElementStack> stacks,INotification notification) {
            outputContainer.AddOutput(stacks,notification);
        }


        public void UpdateSituationDisplay(string stitle, string sdescription, string nextRecipeDescription)
        {
            title.text = stitle;
            description.text = sdescription;
            NextRecipe.text = nextRecipeDescription;
        }

        public void ModifyStoredElementStack(string elementId, int quantity)
        {
            GetSituationStorageStacksManager().ModifyElementQuantity(elementId, quantity);
        }

        public IEnumerable<IElementStack> GetStoredStacks()
        {
            return GetSituationStorageStacksManager().GetStacks();
        }



        public void StoreStacks(IEnumerable<IElementStack> stacksToStore)
        {
            GetSituationStorageStacksManager().AcceptStacks(stacksToStore);
        }

        public IAspectsDictionary GetAspectsFromStoredElements()
        {
            return GetSituationStorageStacksManager().GetTotalAspects();
        }

        public ElementStacksManager GetSituationStorageStacksManager()
        {
            return situationStorage.GetElementStacksManager();
        }

        public void AllOutputsGone() {
            outputContainer.gameObject.SetActive(false);
            situationController.AllOutputsGone();
        }

        public void Retire()
        {
            Destroy(gameObject);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("on window");
        }
    }
}
