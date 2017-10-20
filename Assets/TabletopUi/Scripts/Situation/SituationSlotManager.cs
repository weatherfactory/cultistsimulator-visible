using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class SituationSlotManager : MonoBehaviour {

#if UNITY_EDITOR
        public RecipeSlot slotPrefab;
#endif

        [SerializeField] RectTransform rect;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Text clickCatcher;
        [SerializeField] Vector2 slotSize = new Vector2(80f, 120f);
        [SerializeField] Vector2 spacing = new Vector2(20f, 20f);
        [SerializeField] Vector2 margin = new Vector2(20f, 40f);

        [SerializeField] float sizeTransitionDuration = 0.2f;
        [SerializeField] float slotMoveTransitionDuration = 0.2f;

        List<RecipeSlot> slots = new List<RecipeSlot>();
        int numPerRow = 1;
        int numRows = 1;
		float slotSpacing;

		void OnEnable() {
			SetNumPerRow();
            float height = GetHeightForSlotCount();
            SetHeight(height);
            ToggleScrollOnSize(height);

        }

        public void AddSlot(RecipeSlot slot) {
			if (slot == null)
				return;

            slot.viz.rectTrans.SetParent(transform);
			slot.viz.rectTrans.localScale = Vector3.one;
			slot.viz.rectTrans.localPosition = Vector3.zero;
            slot.viz.rectTrans.localRotation = Quaternion.identity;
			slot.viz.rectTrans.anchorMin = new Vector2(0f, 1f);
			slot.viz.rectTrans.anchorMax = slot.viz.rectTrans.anchorMin;
			slot.viz.rectTrans.anchoredPosition = GetPositionForIndex(slots.Count);
			slots.Add(slot); // add after index was used

            // do not animate if we're not visible - usually only for first slot being created in Initialise
            if (gameObject.activeInHierarchy) 
                slot.viz.TriggerShowAnim();
        }

        public void RemoveSlot(RecipeSlot slot) {
			if (slot == null)
				return;

			slots.Remove(slot);

            if (gameObject.activeInHierarchy)
                slot.viz.TriggerHideAnim();
            else
                slot.Retire();
        }

        public void ReorderSlots() {
            int oldRowCount = numRows;

            // Set dimension values
            SetNumPerRow();

            // Set height if our row count has changed
            if (numRows != oldRowCount) { 
                StartCoroutine(AdjustHeight(GetHeightForSlotCount(), sizeTransitionDuration));
            }

            // Set target positions for all remaining slots
            for (int i = 0; i < slots.Count; i++) {
                slots[i].viz.MoveToPosition(GetPositionForIndex(i), slotMoveTransitionDuration);
            }
        }

        void SetNumPerRow() {
            // one extra spacing added to width to compensate for spacing added to n slots, not n-1 slots.
			numPerRow = Mathf.Max(1, Mathf.FloorToInt((rect.rect.width - margin.x - margin.x + spacing.x) / (slotSize.x + spacing.x)));
			numRows = Mathf.Max(1, Mathf.CeilToInt(slots.Count / (float) numPerRow));
			slotSpacing = (rect.rect.width - margin.x - margin.x) / numPerRow;
        }

        float GetHeightForSlotCount() {
            // remove extra spacing because we add a spacing with each row and only need n-1 spaces
            return margin.y + margin.y + numRows * (slotSize.y + spacing.y) - spacing.y;
        }

        Vector2 GetPositionForIndex(int i) {
            int yPos = Mathf.FloorToInt(i / numPerRow);
            int xPos = i % numPerRow;

			return new Vector2(margin.x + xPos * slotSpacing + slotSize.x * 0.5f, 
				( margin.y + yPos * (slotSize.y + spacing.y) + slotSize.y * 0.5f) * -1f); // TODO: spacing still missing in positioning
			/*
			// positioning exactly according to margins
			return new Vector2(margin.x + xPos * (slotSize.x + spacing.x) + slotSize.x * 0.5f, 
				( margin.y + yPos * (slotSize.y + spacing.y) + slotSize.y * 0.5f) * -1f); // TODO: spacing still missing in positioning
			*/
        }

        IEnumerator AdjustHeight(float target, float duration) {
            float time = 0f;
            float current = rect.rect.height;

            ToggleScrollOnSize(target, true); // If we're going to be scrollable at the end of this, turn it on now

            while (time < duration) {
                SetHeight(Mathf.Lerp(current, target, time / duration)); // TODO: Add some nice easing?
				time += Time.deltaTime;
				yield return null;
            }

            SetHeight(target);
            ToggleScrollOnSize(target, false); // if we're not going to be scrollable at the end of this, turn it off now
        }

        void SetHeight(float height) {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        void ToggleScrollOnSize(float height, bool? onlyToggleTo = null) {
            bool toggleTo = height > scrollRect.viewport.rect.height;

            // If we only want to turn off or on, throw us out
            if (onlyToggleTo != null && toggleTo != onlyToggleTo)
                return;

            scrollRect.vertical = toggleTo;
            clickCatcher.enabled = toggleTo;
        }


    }
}