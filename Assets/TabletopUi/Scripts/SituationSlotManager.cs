using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI {
    public class SituationSlotManager : MonoBehaviour {

        [SerializeField] RectTransform rect;
        [SerializeField] Vector2 slotSize = new Vector2(80f, 120f);
        [SerializeField] Vector2 spacing = new Vector2(20f, 20f);
        [SerializeField] Vector2 margin = new Vector2(20f, 40f);

        const float sizeTransitionDuration = 0.2f;

		List<RecipeSlot> slots = new List<RecipeSlot>();
        int numPerRow;
        int numRows;
		float slotSpacing;

		void OnEnable() {
			SetNumPerRow();
		}

        public void AddSlot(RecipeSlot slot) {
			if (slot == null)
				return;
			
			slot.transform.SetParent(transform);
			slot.transform.localScale = Vector3.one;
			slot.rectTrans.localPosition = Vector3.zero;
			slot.rectTrans.anchorMin = new Vector2(0f, 1f);
			slot.rectTrans.anchorMax = slot.rectTrans.anchorMin;
			slot.rectTrans.anchoredPosition = GetPositionForIndex(slots.Count);
			slots.Add(slot); // add after index was used
            // Do add anim;
            // Set starting position
        }

        public void RemoveSlot(RecipeSlot slot) {
			if (slot == null)
				return;

			slots.Remove(slot);
            // Do remove anim
			Destroy(slot.gameObject);
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

            while (time < duration) {
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(current, target, time / duration)); // TODO: Add some nice easing?
				time += Time.deltaTime;
				yield return null;
            }

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target);
        }

    }
}