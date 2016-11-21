using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Logic;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class SituationToken : DraggableToken,ISituationSubscriber{

        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text; // Currently can be above boxes. Ideally should always be behind boxes - see shadow for solution?
        [SerializeField] Image countdownBar;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] GameObject selectedMarker;
        [SerializeField] private GameObject elementsInSituation;
        private IVerb _verb;
        private Situation situation;
        public SituationState SituationState { get { return situation == null ? SituationState.Extinct : situation.State; } }
        public bool IsOpen = false;

        [HideInInspector] public SituationWindow detailsWindow;
        public string VerbId { get {
            return _verb == null ? null : _verb.Id;
        } }


        public bool isBusy { get { return situation != null; } }
        private float timeRemaining = 0f;
        private int numCompletions = 0; // Stands for the amount of completed cycles.

        public void BeginSituation(Recipe r)
        {
            situation=new Situation(r);
            situation.Subscribe(this);
            SetTimerVisibility(true);
        }

        public string GetTitle()
        {
            return situation == null ? _verb.Label :
        situation.GetTitle();
        }

        public string GetDescription()
        {
            return situation == null ? _verb.Description :
          situation.GetDescription();
        }


        public string GetNextRecipeDescription()
        {
            RecipeConductor rc = new RecipeConductor(Registry.Compendium, GetSituationStacksGateway().GetTotalAspects(), new Dice());
            return situation.GetPrediction(rc);
        }

        private void SetTimerVisibility(bool b)
        {
            countdownBar.gameObject.SetActive(b);
            countdownText.gameObject.SetActive(b);
        }


        public void ExecuteHeartbeat(float interval)
        {
            if (situation != null)
            {
                RecipeConductor rc=new RecipeConductor(Registry.Compendium,GetSituationStacksGateway().GetTotalAspects(),new Dice());
               situation.Continue(rc, interval);
            }
        }

        public void SituationContinues()
        {
            SetTimerVisibility(true);
            DisplayTimeRemaining(situation.Warmup, situation.TimeRemaining);
        }

        public void SituationExecutingRecipe(IEffectCommand command)
        {
            //apply changes *internally*
            foreach (var kvp in command.GetElementChanges())
            {
                GetSituationStacksGateway().ModifyElementQuantity(kvp.Key,kvp.Value);
            }
          //  _subscribers.ForEach(s => s.TokenEffectCommandSent(this, command));

        }



        public void SituationExtinct()
        {
            IElementStacksGateway storedStacksGateway = GetSituationStacksGateway();

            //currently just retrieving everything
            var stacksToRetrieve = storedStacksGateway.GetStacks();

            GetWindowStacksGateway().AcceptStacks(stacksToRetrieve);

            //retrieve everything with slots
            //GetWindowStacksGateway().AcceptStacks(storedStacksGateway.GetStacks().Where(stack=>stack.HasChildSlots()));
            //find what other stacks we need to retrieve
            //AspectMatchFilter filter = situation.GetRetrievalFilter();
            //retrieve them
            // IEnumerable<IElementStack> stacksToRetrieve = filter.FilterElementStacks(storedStacksGateway.GetStacks());
            //    GetWindowStacksGateway().AcceptStacks(stacksToRetrieve);

            //everything else is consumed
            //  storedStacksGateway.ConsumeAllStacks();

            SetTimerVisibility(false);
            situation = null;

            detailsWindow.PopulateAndShow(this);
        }

        public void Initialise(IVerb verb, ElementStacksGateway allStacksGateway) {
            _verb = verb;
            name = "Verb_" + VerbId;

            DisplayName(verb);
            DisplayIcon(verb);
            SetSelected(false);
            countdownBar.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);

        }
        

        private void DisplayName(IVerb v) {
            text.text = v.Label;
        }

        private void DisplayIcon(IVerb v) {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(v.Id);
            artwork.sprite = sprite;
        }

        public void SetSelected(bool isSelected) {
            selectedMarker.gameObject.SetActive(isSelected);
        }

        public Sprite GetSprite() {
            return artwork.sprite;
        }



       public void DisplayTimeRemaining(float duration, float timeRemaining)
        {
            countdownBar.fillAmount = 1f - (timeRemaining / duration);
            countdownText.text = timeRemaining.ToString("0.0") + "s";
        }

        public ElementStacksGateway GetSituationStacksGateway()
        {
            IElementStacksWrapper w = new TabletopElementStacksWrapper(elementsInSituation.transform);
            ElementStacksGateway g = new ElementStacksGateway(w);
            return g;
        }
        public ElementStacksGateway GetWindowStacksGateway()
        {
            IElementStacksWrapper w = new TabletopElementStacksWrapper(detailsWindow.GetSlotsHolder());
            ElementStacksGateway g = new ElementStacksGateway(w);
            return g;
        }

        public void Open()
        {
            detailsWindow.transform.position = transform.position;
            detailsWindow.PopulateAndShow(this);
            elementsInSituation.SetActive(true);
            IsOpen = true;
        }


        public void Close()
        {
            elementsInSituation.SetActive(false);
            detailsWindow.Hide();
            IsOpen = false;
        }

        public void StoreElementStacks(IEnumerable<IElementStack> stacks)
        {
            var containerGateway = GetSituationStacksGateway();
            containerGateway.AcceptStacks(stacks);
        }
    }
}
