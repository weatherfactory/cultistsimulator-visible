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
    public class SituationWindow : MonoBehaviour {

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

        private void Notify(INotification notification)
        {
            Debug.Log(notification.Title + " - " + notification.Description);
        }

        public void AddNotification(INotification notification)
        {
            if(gameObject.activeSelf)
                Notify(notification);
            else
                queuedNotifications.Add(notification);
        }

        public IList<INotification> FlushNotifications()
        {
            List<INotification> flushed = new List<INotification>();
            flushed.AddRange(queuedNotifications);
            queuedNotifications.Clear();

            foreach (var n in flushed)
            {
                Notify(n);
            }

            return flushed;
        }

        public void Initialise(SituationController sc)
        {

            situationController = sc;
        }

        public void Show()
        {
            canvasGroupFader.SetAlpha(0f);
            canvasGroupFader.Show();


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
            button.gameObject.SetActive(true);
            NextRecipe.gameObject.SetActive(false);
            startingSlotsContainer.Initialise(situationController);
            DisplayRecipe(null);
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

        public ElementStacksManager GetStacksGatewayForOutput()
        {
            return outputContainer.GetElementStacksGateway();
        }

        public IEnumerable<IElementStack> GetStacksInStartingSlots()
        {
            return startingSlotsContainer.GetStacksInSlots();
        }


        public AspectsDictionary GetAspectsFromSlottedElements()
        {
            return startingSlotsContainer.GetAspectsFromSlottedCards();
        }



        public void DisplaySituation(string stitle, string sdescription, string nextRecipeDescription)
        {
            title.text = stitle;
            description.text = sdescription;
            NextRecipe.text = nextRecipeDescription;
        }

    }
}
