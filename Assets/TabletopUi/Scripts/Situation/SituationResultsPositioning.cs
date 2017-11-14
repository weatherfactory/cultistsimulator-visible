using System.Collections;
using System.Collections.Generic;
using Assets.Core.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Core.Interfaces;

namespace Assets.CS.TabletopUI {
    public class SituationResultsPositioning : MonoBehaviour {

        [SerializeField] RectTransform rect;
        [SerializeField] Vector2 margin = new Vector2(50f, 40f);

        float moveDuration = 0.2f;
        float availableSpace;
        float xStart;
        //IEnumerable<IElementStack> elements

        void SetAvailableSpace() {
            availableSpace = rect.rect.width - margin.x - margin.x;
            xStart = availableSpace / 2f;
        }

        private void OnEnable() {
            //To turn over the first card automatically when the window opens, uncomment the code below.
            // Delay is to wait for the end of the window transition anim
            //Invoke("DelayedFlip", 0.25f);
        }
        //commented out: for now, we're manually turning all cards.
        // void DelayedFlip() {
        //      var stacks = GetComponentsInChildren<ElementStackToken>();
        //    if (stacks != null && stacks.Length > 0)
        //       stacks[stacks.Length - 1].FlipToFaceUp();
    //}

        public void ReorderCards(IEnumerable<IElementStack> elementStacks) {


            //Order the stacks to put the existing ones first.
            List<IElementStack> existingStacks = new List<IElementStack>();
            List<IElementStack> freshStacks = new List<IElementStack>();

            foreach (var stack in elementStacks)
            {
                if (stack.StackSource.SourceType == SourceType.Fresh)
                    freshStacks.Add(stack);
                else
                    existingStacks.Add(stack);
            }

            var sortedStacks = new List<IElementStack>();
            sortedStacks.AddRange(freshStacks); //new stacks go after that
            sortedStacks.AddRange(existingStacks); //existing stacks go first


            ElementStackToken token;
            int i = 1; // index starts at 1 for positioning math reasons
            int amount = 0;

            SetAvailableSpace();

            foreach (var stack in sortedStacks)
                if (stack is ElementStackToken)
                    amount++;

            foreach (var stack in sortedStacks) {
                token = stack as ElementStackToken;

                if (token == null)
                    continue;

                MoveToPosition(token.transform as RectTransform, GetPositionForIndex(i, amount), 0f);

                token.transform.SetSiblingIndex(i-1); //each card is conceptually on top of the last. Set sibling index to make sure they appear that way.

                //if any stacks are fresh, flip them face down,
                //then mark them as existing
                if(token.StackSource.SourceType==SourceType.Fresh)
                { 
                    token.FlipToFaceDown(true);
                    token.ShowGlow(false, true);
                }
                
                i++;
            }
        }

        Vector2 GetPositionForIndex(int i, int num) {
            return new Vector2(xStart - availableSpace * (i / (float) num) * 0.5f, 0f); 
        }

        public void MoveToPosition(RectTransform rectTrans, Vector2 pos, float duration) {
            // If we're disabled, just set us there
            if (rectTrans.gameObject.activeInHierarchy == false) {
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