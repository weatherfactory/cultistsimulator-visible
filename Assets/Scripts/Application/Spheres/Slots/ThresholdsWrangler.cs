#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI {
    public class ThresholdsWrangler : MonoBehaviour,ISphereEventSubscriber {

        public ThresholdSphere thresholdSpherePrefab;


        [SerializeField] RectTransform rect;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Text clickCatcher;
        [SerializeField] Vector2 slotSize = new Vector2(80f, 120f);
        [SerializeField] Vector2 spacing = new Vector2(20f, 20f);
        [SerializeField] Vector2 margin = new Vector2(20f, 40f);

        [SerializeField] float sizeTransitionDuration = 0.2f;
        [SerializeField] float slotMoveTransitionDuration = 0.2f;
        public readonly OnSphereAddedEvent OnSphereAdded = new OnSphereAddedEvent();
        public readonly OnSphereRemovedEvent OnSphereRemoved = new OnSphereRemovedEvent();

        private Dictionary<ThresholdSphere, FucinePath> _thresholds =
          new Dictionary<ThresholdSphere, FucinePath>();

        int numPerRow = 1;
        int numRows = 1;
		float slotSpacing;
      //  private SituationPath _situationPath;
        private IVerb _verb;

        void OnEnable() {
			SetNumPerRow();
            float height = GetHeightForSlotCount();
            SetHeight(height, true);
            ToggleScrollOnSize(height);
        }

        /// <summary>
        /// Removes any existing thresholds, so we only ever have one primary
        /// </summary>
        /// <param name="sphereSpec"></param>
        /// <param name="situationPath"></param>
        /// <param name="verb"></param>
        /// <returns></returns>
        public virtual ThresholdSphere BuildPrimaryThreshold(SphereSpec sphereSpec,SituationPath situationPath, IVerb verb)
        {
            RemoveAllThresholds();

            _verb = verb;

            return AddThreshold(sphereSpec,situationPath);

        }



        public void RemoveAllThresholds()
        {
            var thresholdsToRetire = new List<ThresholdSphere>(_thresholds.Keys);
            
            foreach(var t in thresholdsToRetire)
                RemoveThreshold(t);
        }

        public void RemoveThreshold(ThresholdSphere thresholdToRemove) {

            OnSphereRemoved.Invoke(thresholdToRemove);
            _thresholds.Remove(thresholdToRemove);

            if (gameObject.activeInHierarchy)
                thresholdToRemove.viz.TriggerHideAnim();
            else
                thresholdToRemove.Retire(SphereRetirementType.Graceful);
        }

        public void ReorderThresholds() {

            int oldRowCount = numRows;

            // Set dimension values
            SetNumPerRow();

            // Set height if our row count has changed
            if (numRows != oldRowCount) {
                float targetHeight = GetHeightForSlotCount();

                if (gameObject.activeInHierarchy) { 
                    StartCoroutine(AdjustHeight(targetHeight, sizeTransitionDuration));
                }
                else { 
                    SetHeight(targetHeight, true);
                    ToggleScrollOnSize(targetHeight);
                }
            }

            int thresholdIndex = 0;
            
            foreach(var t in _thresholds.Keys)
                t.viz.MoveToPosition(GetPositionForIndex(thresholdIndex), slotMoveTransitionDuration);
          

        }

        public void SetNumPerRow() {
            // one extra spacing added to width to compensate for spacing added to n slots, not n-1 slots.
			numPerRow = Mathf.Max(1, Mathf.FloorToInt((rect.rect.width - margin.x - margin.x + spacing.x) / (slotSize.x + spacing.x)));
			numRows = Mathf.Max(1, Mathf.CeilToInt(_thresholds.Count / (float) numPerRow));
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

        void SetHeight(float height, bool resetPos = false) {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            // we also want to reset the start pos
            if (resetPos)
                rect.anchoredPosition = Vector3.zero;
        }

        void ToggleScrollOnSize(float height, bool? onlyToggleTo = null) {
            bool toggleTo = height > scrollRect.viewport.rect.height;

            // If we only want to turn off or on, throw us out
            if (onlyToggleTo != null && toggleTo != onlyToggleTo)
                return;

            scrollRect.vertical = toggleTo;
            clickCatcher.enabled = toggleTo;
        }

        protected ThresholdSphere AddThreshold(SphereSpec sphereSpec,FucinePath parentPath)
        {
            var threshold = GameObject.Instantiate(thresholdSpherePrefab, rect);

            SpherePath newThresholdPath = new SpherePath(parentPath, sphereSpec.Id);
            threshold.Initialise(sphereSpec, newThresholdPath);

            threshold.viz.rectTrans.SetParent(rect);
            threshold.viz.rectTrans.localScale = Vector3.one;
            threshold.viz.rectTrans.localPosition = Vector3.zero;
            threshold.viz.rectTrans.localRotation = Quaternion.identity;
            threshold.viz.rectTrans.anchorMin = new Vector2(0f, 1f);
            threshold.viz.rectTrans.anchorMax = threshold.viz.rectTrans.anchorMin;
            threshold.viz.SetPosition(GetPositionForIndex(_thresholds.Keys.Count));


            _thresholds.Add(threshold, parentPath);
            OnSphereAdded.Invoke(threshold);


            threshold.Subscribe(this);

            // do not animate if we're not visible - usually only for first slot being created in Initialise
            if (gameObject.activeInHierarchy)
                threshold.viz.TriggerShowAnim();

            return threshold;
        }

        protected void AddChildThresholdsForStack(ElementStack stack, FucinePath parentPath)
        {

            foreach (var childSlotSpecification in stack.GetChildSlotSpecificationsForVerb(_verb.Id))
            {
                AddThreshold(childSlotSpecification, parentPath);
            }
        }
        
        private void RemoveChildrenOfThreshold(Sphere thresholdToOrphan)
        {
            if(thresholdToOrphan.GetElementStacks().Any())
                NoonUtility.LogWarning($"This code currently assumes thresholds can only contain one stack token. One ({thresholdToOrphan.GetElementStacks().First().Element.Id}) has been removed from {thresholdToOrphan.GetPath()}, but at least one remains - you may see unexpected results.");

            var thresholdstoRemove=
                new List<ThresholdSphere>(_thresholds.Where(kvp=>kvp.Value.Equals(thresholdToOrphan.GetPath())).Select(kvp=>kvp.Key));
            foreach(var t in thresholdstoRemove)
                RemoveThreshold(t);
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            //if a token has been added: add any necessary child thresholds
            if(args.TokenAdded!=null && args.TokenRemoved != null)
                NoonUtility.LogWarning($"Tokens with valid element stacks seem to have been added ({args.TokenAdded.name}) and removed ({args.TokenRemoved.name}) in a single event. This will likely cause issues, but we'll go ahead with both.");

            if(args.TokenAdded!=null && args.TokenAdded.ElementStack.IsValidElementStack())
                AddChildThresholdsForStack(args.TokenAdded.ElementStack,args.Sphere.GetPath());

            if (args.TokenRemoved!=null)
                RemoveChildrenOfThreshold(args.Sphere);

            //if a token has been removed: remove any child thresholds
        }

   
        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }
    }
}