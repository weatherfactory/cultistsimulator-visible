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
using Assets.TabletopUi.Scripts.Infrastructure;
#pragma warning disable 0649
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

    public class SituationToken : DraggableToken, ISituationAnchor
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

        [SerializeField] Image ongoingSlotImage;
        [SerializeField] Image ongoingSlotArtImage;

        private IVerb _verb;
        public SituationController SituationController { get; private set; }

        public SituationState SituationState
        {
            get { return SituationController.Situation.State; }
        }

        public override void ReturnToTabletop(INotification reason = null)
        {
            Registry.Retrieve<Choreographer>().ArrangeTokenOnTable(this);
            if (reason != null)
                notifier.TokenReturnedToTabletop(this, reason);
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
            countdownBar.gameObject.GetComponent<Image>().color = UIStyle.brightPink;
			countdownBadge.gameObject.SetActive(b);
        }

		public void SetCompletionCount(int newCount) {
			completionBadge.gameObject.SetActive(newCount > 0);
			completionText.text = newCount.ToString();

            if (newCount > 0) { 
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                ShowGlow(true);
            }
            else { 
                ShowGlow(false);
            }
        }

        public int GetCompletionCount()
        {
            int count = 0;
            bool isNonZero = Int32.TryParse(completionText.text, out count);
            if (isNonZero)
                return count;
            else return 0;
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
			SetTimerVisibility(false);
			SetCompletionCount(0);
            ShowGlow(false, false);

            ongoingSlotImage.gameObject.SetActive(false);

        }
        

        private void DisplayName(IVerb v) {
            text.text = v.Label;
        }

        private void DisplayIcon(IVerb v) {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(v.Id);
            artwork.sprite = sprite;
        }

        public Sprite GetSprite() {
            return artwork.sprite;
        }

		public Vector3 GetOngoingSlotPosition() {
			return RectTransform.anchoredPosition3D + ongoingSlotImage.rectTransform.anchoredPosition3D;
		}


        public void DisplayTimeRemaining(float duration, float timeRemaining, Recipe recipe)
        {
            SetTimerVisibility(true);
            
            countdownBar.gameObject.GetComponent<Image>().color = UIStyle.GetColorForCountdownBar(recipe);
            
            countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
            countdownText.text = timeRemaining.ToString("0.0") + "s";
        }

        public Hashtable GetSaveData()
        {
            return SituationController.GetSaveData();
        }

        public void OpenToken()
        {
            DisplayInAir();
            IsOpen = true;
            ShowGlow(false);
        }

        public void CloseToken()
        {
            if (DraggableToken.itemBeingDragged == null || DraggableToken.itemBeingDragged.gameObject != this.gameObject)
                DisplayOnTable();

            IsOpen = false;

            if (SituationController.IsSituationOccupied())
            { 
                ShowGlow(true);
            }
            else {
                SetCompletionCount(0);
            }
        }

        public void UpdateMiniSlotDisplay(IEnumerable<IElementStack> stacksInOngoingSlots)
        {
            IElementStack stack;

            if (stacksInOngoingSlots != null)
                stack = stacksInOngoingSlots.SingleOrDefault(); //THERE CAN BE ONLY ONE (currently)
            else
                stack = null;

            if (stack == null)
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
                    ongoingSlotImage.color = UIStyle.slotPink;
                else
                    ongoingSlotImage.color = UIStyle.slotDefault; 

                break; //We assume there's only one SLOT
            }
    }

        public void DisplayComplete()
        {
            //hide the timer: we're done here
            SetTimerVisibility(false);
            ongoingSlotImage.gameObject.SetActive(false);
            SetCompletionCount( SituationController.GetNumOutputCards() );
        }

        
        public override void OnDrop(PointerEventData eventData)
        {
            if (DraggableToken.itemBeingDragged!=null)
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

        public void ShowDestinationsForStack(IElementStack stack)
        {
            SituationController.ShowDestinationsForStack(stack);
        }

        public override void InteractWithTokenDroppedOn(SituationToken tokenDroppedOn)
        {

            bool moveAsideFor = false;
            tokenDroppedOn.container.TryMoveAsideFor(this, tokenDroppedOn, out moveAsideFor);

            if (moveAsideFor)
                DraggableToken.SetReturn(false, "was moved aside for");
            else
                DraggableToken.SetReturn(true);


        }
    }


}
