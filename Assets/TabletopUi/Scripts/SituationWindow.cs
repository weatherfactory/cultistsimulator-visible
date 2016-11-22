using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class SituationWindow : MonoBehaviour,ISituationSubscriber {

        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Transform cardHolder;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] SlotsContainer slotsHolder;
        [SerializeField] SituationOutputContainer outputHolder;
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;
        [SerializeField] private TextMeshProUGUI NextRecipe;



       public SituationToken linkedToken;

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
            NextRecipe.text = linkedToken.GetNextRecipeDescription();
        }

        public void DisplayReady()
        {
            button.gameObject.SetActive(true);
            NextRecipe.gameObject.SetActive(false);
            slotsHolder.InitialiseSlotsForEmptySituation();
            DisplayRecipe(null);
        }

        public void PopulateAndShow(SituationToken situationToken) {

            canvasGroupFader.SetAlpha(0f);
            canvasGroupFader.Show();

            title.text = situationToken.GetTitle();
            description.text = situationToken.GetDescription();

            if (situationToken.IsBusy)
            
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
            var aspects = slotsHolder.GetAspectsFromSlottedCards();
            var recipe = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, linkedToken.VerbId);
            if(recipe!=null)
            {

            linkedToken.StoreElementStacks(slotsHolder.GetStacksGateway().GetStacks());
            linkedToken.BeginSituation(recipe);
            DisplayBusy();
            }

        }
        public void DisplayRecipeForAspects(AspectsDictionary aspects)
        {
            Recipe r = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, linkedToken.VerbId);
            DisplayRecipe(r);
        }


        public void SituationBeginning(Situation s)
        {
            title.text = s.GetTitle();
            description.text = s.GetDescription();
            NextRecipe.text=linkedToken.GetNextRecipeDescription();
        }

        public void SituationContinues(Situation s)
        {
     
        }

        public void SituationExecutingRecipe(IEffectCommand effectCommand)
        {
        
        }

        public void SituationExtinct()
        {
      
        }
    }
}
