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

    /// <summary>
    /// SituationToken is, used as
    /// - an anchor for everything else in the situation to hang off, and
    /// - a gateway for UI or code interaction with the contents
    /// 
    /// but also
    /// 
    /// - a repository for elements stored in the situation
    /// - a repository for ongoing situation slots
    /// 
    /// ....so it should be two different objects
    /// </summary>
    public class SituationToken : DraggableToken,ISituationAnchor
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
            get { return situationController.SituationStateMachine == null ? SituationState.Extinct : situationController.SituationStateMachine.State; }
        }

        public bool IsOpen = false;

        public bool IsTransient { get { return _verb.Transient; } }

        public override string Id
        {
            get { return _verb == null ? null : _verb.Id; }
        }

        private void SetTimerVisibility(bool b)
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
            name = "Verb_" + Id;

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


        public IRecipeSlot GetSlotFromSituation(string locationInfo,string slotType)
        {
            return situationController.GetSlotBySaveLocationInfoPath(locationInfo,slotType);
        }


        public void DisplayTimeRemaining(float duration, float timeRemaining)
        {
            SetTimerVisibility(true);
            countdownBar.fillAmount = 1f - (timeRemaining / duration);
            countdownText.text = timeRemaining.ToString("0.0") + "s";
        }

        public void AbsorbOngoingSlotContents()
        {
            var inputStacks = GetStacksInOngoingSlots();
            var storageGateway = GetSituationStorageStacksManager();
            storageGateway.AcceptStacks(inputStacks);
        }

        public void ModifyStoredElementStack(string elementId, int quantity)
        {
            GetSituationStorageStacksManager().ModifyElementQuantity(elementId,quantity);
        }

        public IEnumerable<IElementStack> GetStoredStacks()
        {
            return GetSituationStorageStacksManager().GetStacks();
        }

        

        public void StoreStacks(IEnumerable<IElementStack> stacksToStore)
        {
            GetSituationStorageStacksManager().AcceptStacks(stacksToStore);
        }

        public IAspectsDictionary GetAspectsFromStoredElements()
        {
            return GetSituationStorageStacksManager().GetTotalAspects();
        }

        public IAspectsDictionary GetAspectsFromSlottedElements()
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


        public void OpenSituation()
        {

            situationController.OpenSituation();
        }


        public void CloseSituation()
        {

            situationController.CloseSituation();
        }


        public Hashtable GetSaveDataForSituation()
        {
            return situationController.GetSaveDataForSituation();
        }

        public void OpenToken()
        {
            DisplayInAir();
            situationStorage.gameObject.SetActive(true);
            IsOpen = true;
        }

        public void CloseToken()
        {
            if (DraggableToken.itemBeingDragged == null || DraggableToken.itemBeingDragged.gameObject != this.gameObject)
                DisplayOnTable();
            situationStorage.gameObject.SetActive(false);
            IsOpen = false;
        }

        public void DisplaySlotsForSituation(IList<SlotSpecification> ongoingSlots)
        {
            ongoingSlotsContainer.gameObject.SetActive(true);
            ongoingSlotsContainer.SetUpSlots(ongoingSlots);
        }

        public void SituationExtinct()
        {
            //hide the timer: we're done here
            SetTimerVisibility(false);
            //and we don't want anyone adding any more slots
            DeactivateOngoingSlots();
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

        public void AddOutput(IEnumerable<IElementStack> stacksForOutput, Notification notification)
        {
            situationController.AddOutput(stacksForOutput,notification);
        }

        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo)
        {
            return ongoingSlotsContainer.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            if(DraggableToken.itemBeingDragged!=null)
            DraggableToken.itemBeingDragged.InteractWithTokenDroppedOn(this);       
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            // pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)

            if (!IsOpen)
            { 
                OpenSituation();
             
                (container as TabletopContainer).CloseAllSituationWindowsExcept(this);

            }
            else
            { 
                CloseSituation();
           
                    
            }

        }

    }


}
