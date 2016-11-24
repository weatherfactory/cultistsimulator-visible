using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.SlotsContainers;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class SituationToken : DraggableToken
    {

        [SerializeField] Image artwork;

        [SerializeField] TextMeshProUGUI text;
            // Currently can be above boxes. Ideally should always be behind boxes - see shadow for solution?

        [SerializeField] Image countdownBar;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] GameObject selectedMarker;
        [SerializeField] public SituationStorage situationStorage;
        [SerializeField] private OngoingSlotsContainer ongoingSlotsContainer;

        private IVerb _verb;
        private SituationController situationController;

        public SituationState SituationState
        {
            get { return situationController.situation == null ? SituationState.Extinct : situationController.situation.State; }
        }

        public bool IsOpen = false;

        public bool IsTransient { get { return _verb.Transient; } }

        public string VerbId
        {
            get { return _verb == null ? null : _verb.Id; }
        }

  

        public void SetTimerVisibility(bool b)
        {
            countdownBar.gameObject.SetActive(b);
            countdownText.gameObject.SetActive(b);
        }


        public HeartbeatResponse ExecuteHeartbeat(float interval)
        {
       return  situationController.ExecuteHeartbeat(interval);
        }


      

        public void Initialise(IVerb verb,SituationController sc) {
            _verb = verb;
            situationController = sc;
            name = "Verb_" + VerbId;

            DisplayName(verb);
            DisplayIcon(verb);
            SetSelected(false);
            countdownBar.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);

            ongoingSlotsContainer.Initialise(situationController);

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

        public AspectsDictionary GetAspectsFromStoredElements()
        {
            return GetSituationStorageStacksManager().GetTotalAspects();
        }

        public AspectsDictionary GetAspectsFromSlottedElements()
        {
            return ongoingSlotsContainer.GetAspectsFromSlottedCards();
        }

        public IEnumerable<IElementStack> GetStacksInOngoingSlots()
        {
            return ongoingSlotsContainer.GetStacksInSlots();
        }


        public ElementStacksManager GetSituationStorageStacksManager()
        {
            return situationStorage.GetElementStacksManager();
        }


        public void OpenController()
        {
            situationController.Open();
        }


        public void CloseController()
        {
            situationController.Close();
        }


        public void Open()
        {
            situationStorage.gameObject.SetActive(true);
            IsOpen = true;
        }

        public void Close()
        {
            situationStorage.gameObject.SetActive(false);
            IsOpen = false;
        }

        public void ActivateOngoingSlots(IList<SlotSpecification> slotsToBuild)
        {
            ongoingSlotsContainer.gameObject.SetActive(true);
            ongoingSlotsContainer.UpdateSlots(slotsToBuild);
        }

        public void DeactivateOngoingSlots()
        {
            ongoingSlotsContainer.DestroyAllSlots();
            ongoingSlotsContainer.gameObject.SetActive(false);
        }

        public IList<IRecipeSlot> GetUnfilledGreedySlots()
        {
 
            return ongoingSlotsContainer.GetUnfilledGreedySlots();
           
        }

        public void Retire()
        {
            Destroy(gameObject);
        }
    }


}
