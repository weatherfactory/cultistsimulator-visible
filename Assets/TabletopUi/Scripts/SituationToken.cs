using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
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
        [SerializeField] private SlotsContainer slotsInSituation;
        
        private IVerb _verb;
        private Situation situation;
        public SituationState SituationState { get { return situation == null ? SituationState.Extinct : situation.State; } }
        public bool IsOpen = false;
        private IList<INotification> queuedNotifications=new List<INotification>();

        [HideInInspector] public SituationWindow linkedWindow;
        public string VerbId { get {
            return _verb == null ? null : _verb.Id;
        } }

        /// <summary>
        /// should return number of executions waiting; 0 if it's currently running a situation, -1 if it's not currently running anything
        /// </summary>
        public bool IsBusy
        {
            get { return situation != null; }
        }

        private float timeRemaining = 0f;
        private int numCompletions = 0; // Stands for the amount of completed cycles.

        public void BeginSituation(Recipe r)
        {
            situation=new Situation(r);
            situation.Subscribe(this);
            SetTimerVisibility(true);
            slotsInSituation.InitialiseSlotsForRecipe(r);

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

        public IList<INotification> FlushNotifications()
        {
            List<INotification> flushed=new List<INotification>();
            flushed.AddRange(queuedNotifications);
            queuedNotifications.Clear();
            return flushed;
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
            foreach (var kvp in command.GetElementChanges())
            {
                GetSituationStacksGateway().ModifyElementQuantity(kvp.Key,kvp.Value);
            }
            queuedNotifications.Add(new Notification (command.Title,command.Description));

          //  _subscribers.ForEach(s => s.TokenEffectCommandSent(this, command));

        }



        public void SituationExtinct()
        {
            IElementStacksGateway storedStacksGateway = GetSituationStacksGateway();

            //currently just retrieving everything
            var stacksToRetrieve = storedStacksGateway.GetStacks();

            linkedWindow.GetStacksGatewayForOutput().AcceptStacks(stacksToRetrieve);

            SetTimerVisibility(false);
            situation = null;
        }

        public void Initialise(IVerb verb) {
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


        public void Open()
        {
            linkedWindow.transform.position = transform.position;
            linkedWindow.PopulateAndShow(this);
            elementsInSituation.SetActive(true);
            IsOpen = true;
        }


        public void Close()
        {
            elementsInSituation.SetActive(false);
            linkedWindow.Hide();
            IsOpen = false;
        }

        public void StoreElementStacks(IEnumerable<IElementStack> stacks)
        {
            var containerGateway = GetSituationStacksGateway();
            containerGateway.AcceptStacks(stacks);
        }
    }


}
