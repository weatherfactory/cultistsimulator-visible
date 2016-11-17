using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
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
        [SerializeField] private ElementStacksGateway allStacksGateway;
        private Verb _verb;
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

        private void SetTimerVisibility(bool b)
        {
            countdownBar.gameObject.SetActive(b);
            countdownText.gameObject.SetActive(b);
        }


        public void ExecuteHeartbeat(float interval)
        {
            if (situation != null)
            {
                SituationState currentState = situation.Continue(interval);
            }
        }

        public void SituationContinues()
        {
            SetTimerVisibility(true);
            DisplayTimeRemaining(situation.Warmup, situation.TimeRemaining);
        }

        public void SituationCompletes(IEffectCommand command)
        {
            //er.... do I definitely want to do this? in the cold light of day this looks like it needs refactoring
            //but this holding the allstacksgateway - the other obvious route? is also smelly
            //is there another way I can get it into the recipeconductor?
            //should the stacksgateways be the things with subscribers?
            _subscribers.ForEach(s => s.TokenEffectCommandSent(this, command));
            RecipeConductor rc = new RecipeConductor(Registry.Compendium, allStacksGateway, Registry.Dice);
            situation.TryFindRecipeToRunAfterCompletion(rc);
        }

        public void SituationExtinct()
        {
            SetTimerVisibility(false);
            situation = null;
            detailsWindow.PopulateAndShow(this);
        }

        public void Initialise(Verb verb, ElementStacksGateway allStacksGateway) {
            _verb = verb;
            name = "Verb_" + VerbId;

            DisplayName(verb);
            DisplayIcon(verb);
            SetSelected(false);
            countdownBar.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);

            this.allStacksGateway = allStacksGateway;
        }
        

        private void DisplayName(Verb v) {
            text.text = v.Label;
        }

        private void DisplayIcon(Verb v) {
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
            IElementStacksWrapper verbBoxWrapper = new TabletopElementStacksWrapper(elementsInSituation.transform);
            ElementStacksGateway verbBoxStacks = new ElementStacksGateway(verbBoxWrapper);
            return verbBoxStacks;
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
