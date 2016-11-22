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
        [SerializeField] AbstractSlotsContainer slotsHolder;
        [SerializeField] SituationOutputContainer outputHolder;
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;
        [SerializeField] private TextMeshProUGUI NextRecipe;
        private SituationController situationController;

        void OnEnable () {
            button.onClick.AddListener(HandleOnButtonClicked);
        }
        void OnDisable() {
            button.onClick.RemoveListener(HandleOnButtonClicked);
        }


        private void DisplayBusy()
        {
            button.gameObject.SetActive(false);
            NextRecipe.gameObject.SetActive(true);
            slotsHolder.gameObject.SetActive(false);
            NextRecipe.text =  situationController.GetNextRecipeDescription();
        }

        public void DisplayReady()
        {
            button.gameObject.SetActive(true);
            NextRecipe.gameObject.SetActive(false);
            slotsHolder.Initialise(situationController);
            DisplayRecipe(null);
        }

        public void PopulateAndShow(SituationController sc)
        {

           situationController = sc;

            canvasGroupFader.SetAlpha(0f);
            canvasGroupFader.Show();

            if (situationController.situationToken.IsBusy)
            
                DisplayBusy();
            else
                DisplayReady();

        }


        public void Hide()
        {
            canvasGroupFader.Hide();
        }


        public ElementStacksGateway GetStacksGatewayForOutput()
        {
            return outputHolder.GetStacksGateway();
        }

        private void DisplayRecipe(Recipe r)
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
            var aspects = GetAspectsFromSlottedCards();
            var recipe = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationController.situationToken.VerbId);
            if(recipe!=null)
            {

                situationController.situationToken.StoreElementStacksInSituation(slotsHolder.GetStacksGateway().GetStacks());
                situationController.BeginSituation(recipe);
            DisplayBusy();
            }
        }

        public AspectsDictionary GetAspectsFromSlottedCards()
        {
            return slotsHolder.GetAspectsFromSlottedCards();
        }

        public void DisplayRecipeForVerbAndAspects(AspectsDictionary aspects,string verbId)
        {
            Recipe r = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, verbId);
            DisplayRecipe(r);
        }


        public void DisplaySituation(string stitle, string sdescription, string nextRecipeDescription)
        {
            title.text = stitle;
            description.text = sdescription;
            NextRecipe.text = nextRecipeDescription;
        }

    }
}
