using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI
{
    public class ThresholdsGridArrangement: AbstractSphereArrangement
    {
        [SerializeField] RectTransform rect;
        [SerializeField] Text clickCatcher;
        [SerializeField] private Vector2 expectedThresholdSize;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private Vector2 margin;

        [SerializeField] private float sizeTransitionDuration;
        [SerializeField] private float slotMoveTransitionDuration;
        int numPerRow = 1;
        int numRows = 1;
        float slotSpacing;
        void OnEnable()
        {
            SetNumPerRow();
            float height = GetHeightForSlotCount();
            SetHeight(height, true);
        }

        //This is currently not in use, but I'm not sure if it might need to be used for reference again

        //public void ReorderThresholds()
        //{

        //    int oldRowCount = numRows;

        //    // Set dimension values
        //    SetNumPerRow();

        //    // Set height if our row count has changed
        //    if (numRows != oldRowCount)
        //    {
        //        float targetHeight = GetHeightForSlotCount();

        //        if (gameObject.activeInHierarchy)
        //        {
        //            StartCoroutine(AdjustHeight(targetHeight, sizeTransitionDuration));
        //        }
        //        else
        //        {
        //            SetHeight(targetHeight, true);
        //        }
        //    }

        //    int thresholdIndex = 0;

        //    foreach (var t in _thresholds.Keys)
        //        t.viz.MoveToPosition(GetPositionForIndex(thresholdIndex), slotMoveTransitionDuration);


        //}

        public override void AddNewSphereToArrangement(Sphere sphere, int index)
        {
            var threshold = sphere as ThresholdSphere;

            threshold.viz.rectTrans.SetParent(rect);
            threshold.viz.rectTrans.localScale = Vector3.one;
            threshold.viz.rectTrans.localPosition = Vector3.zero;
            threshold.viz.rectTrans.localRotation = Quaternion.identity;
            threshold.viz.rectTrans.anchorMin = new Vector2(0f, 1f);
            threshold.viz.rectTrans.anchorMax = threshold.viz.rectTrans.anchorMin;
            threshold.viz.SetPosition(GetPositionForIndex(index));

            // do not animate if we're not visible - usually only for first slot being created in Initialise
            if (gameObject.activeInHierarchy)
                threshold.viz.TriggerShowAnim();
        }

        protected void SetNumPerRow()
        {
            // one extra spacing added to width to compensate for spacing added to n slots, not n-1 slots.
            numPerRow = Mathf.Max(1, Mathf.FloorToInt((rect.rect.width - margin.x - margin.x + spacing.x) / (expectedThresholdSize.x + spacing.x)));
            // numRows = Mathf.Max(1, Mathf.CeilToInt(_thresholds.Count / (float)numPerRow));
            slotSpacing = (rect.rect.width - margin.x - margin.x) / numPerRow;
        }

        float GetHeightForSlotCount()
        {
            // remove extra spacing because we add a spacing with each row and only need n-1 spaces
            return margin.y + margin.y + numRows * (expectedThresholdSize.y + spacing.y) - spacing.y;
        }

        Vector2 GetPositionForIndex(int i)
        {
            // ReSharper disable once PossibleLossOfFraction
            int rowNumberForThisThreshold = Mathf.FloorToInt(i / numPerRow);
            int overflowXModuloBasedOnIndexAndNumPerRow = i % numPerRow;


            float xPos = overflowXModuloBasedOnIndexAndNumPerRow * slotSpacing + expectedThresholdSize.x * 0.5f;

            float yPos = rowNumberForThisThreshold * (expectedThresholdSize.y + spacing.y) + expectedThresholdSize.y * 0.5f;

            float xPosWithMargin = margin.x + xPos;
            float yPosWithMargin = margin.y + yPos;

            float yPosWithMarginDownwards = yPosWithMargin * -1f;

            return new Vector2(xPosWithMargin,
                yPosWithMarginDownwards);


        }

        IEnumerator AdjustHeight(float target, float duration)
        {
            float time = 0f;
            float current = rect.rect.height;


            while (time < duration)
            {
                SetHeight(Mathf.Lerp(current, target, time / duration)); // TODO: Add some nice easing?
                time += Time.deltaTime;
                yield return null;
            }

            SetHeight(target);
        }

        void SetHeight(float height, bool resetPos = false)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            // we also want to reset the start pos
            if (resetPos)
                rect.anchoredPosition = Vector3.zero;
        }


    }
}
