using System;
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
using UnityEngine.Assertions;
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
        // NOTE MARTIN: Possibly something that can be solved by the sorting layer?

        [SerializeField] GameObject[] countdownVisuals;
        [SerializeField] Image countdownBar;
		[SerializeField] Image countdownBadge;
		[SerializeField] TextMeshProUGUI countdownText;
		[SerializeField] Image completionBadge;
		[SerializeField] TextMeshProUGUI completionText;
        [SerializeField] GameObject selectedMarker;
        [SerializeField] public SituationStorage situationStorage;

        [SerializeField] Image ongoingSlotImage;
        [SerializeField] Image ongoingSlotArtImage;

        private IVerb _verb;
        private SituationController situationController;

        public SituationState SituationState
        {
            get { return situationController.SituationStateMachine.State; }
        }

        public bool IsOpen = false;

        public bool IsTransient { get { return _verb.Transient; } }

        public override string Id
        {
            get { return _verb == null ? null : _verb.Id; }
        }

        private void SetTimerVisibility(bool b)
        {
            foreach (var go in countdownVisuals) 
                go.SetActive(b);

			countdownBar.gameObject.SetActive(b);
			countdownBadge.gameObject.SetActive(b);
            //countdownText.gameObject.SetActive(b); // Is child of countdownBadge, doesn't need to be toggled by itself
        }


		// NOTE MARTIN: New method to show amount of fixed events:
		void ShowCompletionCount(int newCount) {
			completionBadge.gameObject.SetActive(newCount > 0);
			completionText.text = newCount.ToString(); // Is child of completionBadge, doesn't need to be toggled by itself
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
			SetTimerVisibility(false);
			ShowCompletionCount(0);

            ongoingSlotImage.gameObject.SetActive(false);

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

        public void UpdateMiniSlotDisplay(IEnumerable<IElementStack> stacksInOngoingSlots)
        {
            var stack = stacksInOngoingSlots.SingleOrDefault(); //THERE CAN BE ONLY ONE (currently)

            if(stack==null)
            {
                ongoingSlotArtImage.sprite = null;
                ongoingSlotArtImage.color = Color.black;
            }
            else
            {
                ongoingSlotArtImage.sprite = ResourcesManager.GetSpriteForElement(stack.Id);
                ongoingSlotArtImage.color = Color.white;
            }
        }



        public void DisplayMiniSlotDisplay(IList<SlotSpecification> ongoingSlots) {

            if(ongoingSlots.Count>1)
                throw new InvalidOperationException("More than one ongoing slot specified for this recipe, and we don't currently know how to deal with that");

            ongoingSlotImage.gameObject.SetActive(true);

            foreach (var slot in ongoingSlots) {
                if (slot.Greedy)
                    ongoingSlotImage.color = new Color32(0x94, 0xE2, 0xEF, 0xFF);
                else
                    ongoingSlotImage.color = Color.black; 

                break; //We assume there's only one SLOT
            }


    }

        public void SituationComplete()
        {
            //hide the timer: we're done here
            SetTimerVisibility(false);
            //and we don't want anyone adding any more slots
            DeactivateOngoingSlots();
        }

        public void DeactivateOngoingSlots() {
            // This was removed, may break things.
            //ongoingSlotsContainer.DestroyAllSlots();
            ongoingSlotImage.gameObject.SetActive(false);
        }



        public void AddOutput(IEnumerable<IElementStack> stacksForOutput, Notification notification)
        {
            situationController.AddOutput(stacksForOutput,notification);
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
