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
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class SituationWindow : MonoBehaviour,ISituationDetails {

        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Transform cardHolder;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] StartingSlotsContainer startingSlotsContainer;
        [SerializeField] SituationOutputContainer outputContainer;
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;
        [SerializeField] private TextMeshProUGUI NextRecipe;
        [SerializeField] private TabletopContainer tabletopContainer;
        public IList<INotification> queuedNotifications = new List<INotification>();
        private SituationController situationController;

        void OnEnable()
        {
            button.onClick.AddListener(HandleOnButtonClicked);
        }
        void OnDisable()
        {
            button.onClick.RemoveListener(HandleOnButtonClicked);
        }


        public void Initialise(SituationController sc)
        {

            situationController = sc;
            startingSlotsContainer.Initialise(situationController);
            DisplayRecipe(null);
        }

        public void Show(bool situationOngoing)
        {
            canvasGroupFader.SetAlpha(0f);
            canvasGroupFader.Show();
            

            if(situationOngoing || outputContainer.GetCurrentOutputs().Any())
                DisplayOngoing();
            else
              DisplayStarting();

        }

        public void Hide()
        {
            canvasGroupFader.Hide();
        }


        public void DisplayOngoing()
        {
            button.gameObject.SetActive(false);
            NextRecipe.gameObject.SetActive(true);
            startingSlotsContainer.gameObject.SetActive(false);
        }

        public void DisplayStarting()
        {
            startingSlotsContainer.gameObject.SetActive(true);
            button.gameObject.SetActive(true);
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
            }
            else
            {
                title.text = "";
                description.text = "";
            }
        }

        void HandleOnButtonClicked()
        {

            situationController.AttemptActivateRecipe();
 
        }

        public IRecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo)
        {
            return
                startingSlotsContainer.GetAllSlots().SingleOrDefault(s => s.SaveLocationInfoPath==locationInfo);
        }

        public IEnumerable<IElementStack> GetStacksInStartingSlots()
        {
            return startingSlotsContainer.GetStacksInSlots();
        }


        public AspectsDictionary GetAspectsFromSlottedElements()
        {
            return startingSlotsContainer.GetAspectsFromSlottedCards();
        }

        public void AddOutput(IEnumerable<IElementStack> stacks,INotification notification)
        {
            outputContainer.AddOutput(stacks,notification);
        }


        public void DisplaySituation(string stitle, string sdescription, string nextRecipeDescription)
        {
            title.text = stitle;
            description.text = sdescription;
            NextRecipe.text = nextRecipeDescription;
        }

        public void AllOutputsGone()
        {
            title.text = "";
            description.text = "";
            NextRecipe.text = "";
            situationController.AllOutputsGone();
        }

        public void Retire()
        {
            Destroy(gameObject);
        }
    }
}
