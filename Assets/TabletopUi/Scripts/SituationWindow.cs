using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class SituationWindow : MonoBehaviour,ITokenSubscriber {

        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Transform cardHolder;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] SlotsContainer slotsHolder;
        [SerializeField]Transform outputHolder;
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

        private void DisplayReady()
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
            return new ElementStacksGateway(new TabletopElementStacksWrapper(outputHolder,this));
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

        public void TokenEffectCommandSent(DraggableToken draggableToken, IEffectCommand effectCommand)
        {
            //nothing yet: this may be redundant
        }

        public void TokenPickedUp(DraggableToken draggableToken)
        {
            
            var stacks = GetStacksGatewayForOutput().GetStacks();
            //picking up a token from a completed window; some left
            if (stacks.Any())
                return;
            //if picking up a token from a completed window; none left
            else if (!stacks.Any() & slotsHolder.primarySlot==null)
                DisplayReady();
            else
            { 
            //if picking up a token from an open window
            DisplayRecipeForAspects(slotsHolder.GetAspectsFromSlottedCards());
            draggableToken.SetContainer(null);
                slotsHolder.TokenRemovedFromSlot();

            }

        }

        
        public void DisplayRecipeForAspects(AspectsDictionary aspects)
        {
            Recipe r = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, linkedToken.VerbId);
            DisplayRecipe(r);
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
//currently nothing 
        }

        public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
        {
           //currently nothing; tokens are automatically returned home
        }
    }
}
