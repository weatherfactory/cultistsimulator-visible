#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.CS.TabletopUI {
    public class SituationResultsPositioning : MonoBehaviour {

        [SerializeField] RectTransform rect;
        [SerializeField] Vector2 margin = new Vector2(50f, 40f);

        //float moveDuration = 0.2f;
        float availableSpace;
        float xStart;
        //IEnumerable<IElementStack> elements

        void SetAvailableSpace() {
            availableSpace = rect.rect.width - margin.x - margin.x;
            xStart = availableSpace / 2f;
        }

        /*
        //commented out: for now, we're manually turning all cards.
        private void OnEnable() {
            //To turn over the first card automatically when the window opens, uncomment the code below.
            // Delay is to wait for the end of the window transition anim
            Invoke("DelayedFlip", 0.25f);
        }

        void DelayedFlip() {
              var stacks = GetComponentsInChildren<ElementStackToken>();
            if (stacks != null && stacks.Length > 0)
               stacks[stacks.Length - 1].FlipToFaceUp();
        }
        */

        public void ReorderCards(IEnumerable<IElementStack> elementStacks) {
            var sortedStacks = SortStacks(elementStacks);

            ElementStackToken token;
            int i = 1; // index starts at 1 for positioning math reasons
            int amount = 0;

            SetAvailableSpace();

            string debugText = "Reorder Results: ";

            foreach (var stack in sortedStacks)
                if (stack is ElementStackToken)
                    amount++;

            foreach (var stack in sortedStacks) {
                token = stack as ElementStackToken;

                if (token == null)
                    continue;

                MoveToPosition(token.transform as RectTransform, GetPositionForIndex(i, amount), 0f);
                token.transform.SetSiblingIndex(i-1); //each card is conceptually on top of the last. Set sibling index to make sure they appear that way.

                //if any stacks are fresh, flip them face down, otherwise face up
                if (token.StackSource.SourceType == SourceType.Fresh)
                    token.FlipToFaceDown(true); // flip down instantly
                else
                    token.FlipToFaceUp(gameObject.activeInHierarchy); // flip up with anim, if we're visible 

                debugText += stack.Id + " (" + token.StackSource.SourceType + ") ";

                i++;
            }

            NoonUtility.Log(debugText,10);
        }

        List<IElementStack> SortStacks(IEnumerable<IElementStack> elementStacks) {
            var freshStacks = new List<IElementStack>();
            var existingStacks = new List<IElementStack>();

            foreach (var stack in elementStacks) {
                if (stack.StackSource.SourceType == SourceType.Fresh)
                    freshStacks.Add(stack);
                else
                    existingStacks.Add(stack);
            }

            freshStacks.AddRange(existingStacks); //existing stacks go after fresh stacks

            return freshStacks;
        }

        Vector2 GetPositionForIndex(int i, int num) {
            return new Vector2(xStart - availableSpace * (i / (float) num) * 0.5f, 0f); 
        }

        public void MoveToPosition(RectTransform rectTrans, Vector2 pos, float duration) {
            // If we're disabled or our token is, just set us there
            if (rectTrans.gameObject.activeInHierarchy == false || gameObject.activeInHierarchy == false) {
                rectTrans.anchoredPosition = pos;
                return;
            }

            if (rectTrans.anchoredPosition == pos || Vector2.Distance(pos, rectTrans.anchoredPosition) < 0.2f)
                return;

            StartCoroutine(DoMove(rectTrans, pos, duration));
        }

        IEnumerator DoMove(RectTransform rectTrans, Vector2 targetPos, float duration) {
            float ease;
            float lerp;
            float time = 0f;
            Vector2 lastPos = rectTrans.anchoredPosition;

            while (time < duration) {
                lerp = time / duration;
                time += Time.deltaTime;
                ease = Easing.Ease(Easing.EaseType.SinusoidalInOut, lerp);
                rectTrans.anchoredPosition = Vector2.Lerp(lastPos, targetPos, ease);
                yield return null;
            }

            rectTrans.anchoredPosition = targetPos;
        }

    }
}