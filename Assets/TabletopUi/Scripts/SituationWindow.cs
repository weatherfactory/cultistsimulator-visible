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


        public void DisplayOngoing(Recipe forRecipe) {

            Debug.Log("Display ongoing  " + Verb.Id);


            startingSlotsContainer.gameObject.SetActive(false);
            ongoingSlotsContainer.SetUpSlots(forRecipe.SlotSpecifications);
            outputContainer.gameObject.SetActive(true);

            button.gameObject.SetActive(false);
            NextRecipe.gameObject.SetActive(true);
        }

        public void DisplayStarting()
        {

            startingSlotsContainer.gameObject.SetActive(true);
            outputContainer.gameObject.SetActive(false);

            title.text = Verb.Label;
            description.text = Verb.Description;
            NextRecipe.text = "";

            NextRecipe.gameObject.SetActive(false);
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


        public AspectsDictionary GetAspectsFromSlottedElements()
        {
            return startingSlotsContainer.GetAspectsFromSlottedCards();
        }

        public IEnumerable<ISituationOutput> GetCurrentOutputs()
        {
            return outputContainer.GetCurrentOutputs();
        }

        public void RunSlotConsumptions()
        {
            foreach (var s in startingSlotsContainer.GetAllSlots())
                s.RunConsumption();

        }

        public void AddOutput(IEnumerable<IElementStack> stacks,INotification notification) {
            startingSlotsContainer.gameObject.SetActive(false);
            outputContainer.AddOutput(stacks,notification);
        }


        public void UpdateSituationDisplay(string stitle, string sdescription, string nextRecipeDescription)
        {
            title.text = stitle;
            description.text = sdescription;
            NextRecipe.text = nextRecipeDescription;
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
