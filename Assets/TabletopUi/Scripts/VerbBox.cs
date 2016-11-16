using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class VerbBox : DraggableToken {

        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text; // Currently can be above boxes. Ideally should always be behind boxes - see shadow for solution?
        [SerializeField] Image countdownBar;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] GameObject selectedMarker;
        [SerializeField] private GameObject elementsInSituation;
        private Verb _verb;
        private Situation situation;
        public SituationState SituationState { get { return situation == null ? SituationState.Extinct : situation.State; } }


        [HideInInspector] public SituationWindow detailsWindow;
        public string verbId { get {
            return _verb == null ? null : _verb.Id;
        } }


        public bool isBusy { get { return situation != null; } }
        private float timeRemaining = 0f;
        private int numCompletions = 0; // Stands for the amount of completed cycles.

        public void BeginSituation(Recipe r)
        {
            situation=new Situation(r);
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


        public void ContinueSituation(float interval)
        {
            if (situation != null)
            {
                SituationState currentState = situation.Continue(interval);
                if (currentState == SituationState.Ongoing)
                { 
                    SetTimerVisibility(true);
                    DisplayTimeRemaining(situation.Warmup, situation.TimeRemaining);
                }
                else if(currentState==SituationState.Complete)
                { 
                    SetTimerVisibility(false);
                    IEffectCommand ec = situation.GetEffectCommand();
                    //er.... do I definitely want to do this? I guess a situation is kinda part of a verbbox, but in the cold light of day this looks like it needs refactoring 
                    _subscribers.ForEach(s=>s.TokenEffectCommandSent(this,ec));
                }
                else if (currentState == SituationState.Extinct)
                {
                    RecipeConductor rc=new RecipeConductor(Registry.Compendium);
                    situation.TryBeginNextRecipe(rc);
                    if(detailsWindow!=null)
                        detailsWindow.PopulateForVerb(this);
                }
            }
        }


        public void SetVerb(string id) {
            var verb = Registry.Compendium.GetVerbById(id);

            if (verb != null)
                SetVerb(verb);
        }

        public void SetVerb(Verb verb)
        {
            _verb = verb;
            name = "Verb_" + verbId;

            if (verb == null)
                return;

            DisplayName(verb);
            DisplayIcon(verb);
            SetSelected(false);
            countdownBar.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);
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

        public ElementStacksGateway GetStacksGateway()
        {
            IElementStacksWrapper verbBoxWrapper = new TabletopElementStacksWrapper(elementsInSituation.transform);
            ElementStacksGateway verbBoxStacks = new ElementStacksGateway(verbBoxWrapper);
            return verbBoxStacks;
        }


        public void DisplaySituationWindow(SituationWindow window)
        {
            window.transform.position = transform.position;
            window.PopulateForVerb(this);
            elementsInSituation.SetActive(true);
        }


        public void HideSituationWindow()
        {
            detailsWindow = null; // this is hacky. We're saving the window in the card so we don't double-open windows.
            elementsInSituation.SetActive(false);
        }

        public void StoreElementStacks(IEnumerable<IElementStack> stacks)
        {
            var containerGateway = GetStacksGateway();
  
            containerGateway.AcceptStacks(stacks);
        }
    }
}
