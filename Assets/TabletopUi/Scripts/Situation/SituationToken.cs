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


        [SerializeField] Image ongoingSlotImage;
        [SerializeField] Image ongoingSlotArtImage;

        private IVerb _verb;
        public SituationController SituationController { get; private set; }

        public SituationState SituationState
        {
            get { return SituationController.SituationStateMachine.State; }
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

        }


		// NOTE MARTIN: New method to show amount of fixed events:
		public void ShowCompletionCount(int newCount) {
			completionBadge.gameObject.SetActive(newCount > 0);
			completionText.text = newCount.ToString(); 
		}


        public HeartbeatResponse ExecuteHeartbeat(float interval)
        {
       		return  SituationController.ExecuteHeartbeat(interval);
        }

        

        public void Initialise(IVerb verb,SituationController sc) {
            _verb = verb;
            SituationController = sc;
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




        public void DisplayTimeRemaining(float duration, float timeRemaining)
        {
            SetTimerVisibility(true);
            countdownBar.fillAmount = 1f - (timeRemaining / duration);
            countdownText.text = timeRemaining.ToString("0.0") + "s";
        }




        public Hashtable GetSaveDataForSituation()
        {
            return SituationController.GetSaveDataForSituation();
        }

        public void OpenToken()
        {
            DisplayInAir();
            IsOpen = true;
        }

        public void CloseToken()
        {
            if (DraggableToken.itemBeingDragged == null || DraggableToken.itemBeingDragged.gameObject != this.gameObject)
                DisplayOnTable();
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
            if (ongoingSlots.Count>1)
                throw new InvalidOperationException("More than one ongoing slot specified for this recipe, and we don't currently know how to deal with that");

			ongoingSlotImage.gameObject.SetActive(ongoingSlots.Count > 0);

            foreach (var slot in ongoingSlots) {
                if (slot.Greedy)
                    ongoingSlotImage.color = new Color32(0x94, 0xE2, 0xEF, 0xFF);
                else
                    ongoingSlotImage.color = Color.black; 

                break; //We assume there's only one SLOT
            }
    }

        public void DisplayComplete()
        {
            //hide the timer: we're done here
            SetTimerVisibility(false);
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
                SituationController.OpenSituation();

                (container as TabletopContainer).CloseAllSituationWindowsExcept(this);

            }
            else
            CloseSituation();

        }

        public void CloseSituation()
        {
                SituationController.CloseSituation();

        }

    }


}
