using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Core.Interfaces;

namespace Assets.CS.TabletopUI {
    public class SituationResultsPositioning : MonoBehaviour {

        [SerializeField] RectTransform rect;
        [SerializeField] Vector2 cardSize = new Vector2(80f, 120f);
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
            // Try to turn over the first card in case we set up the cards while the window was closed.
            // Delay is to wait for the end of the window transition anim
            Invoke("DelayedFlip", 0.25f);
        }

        void DelayedFlip() {
            var stacks = GetComponentsInChildren<ElementStackToken>();

            if (stacks != null && stacks.Length > 0)
                stacks[stacks.Length - 1].FlipToFaceUp();
        }

        public void ReorderCards(IEnumerable<IElementStack> elements) {
            ElementStackToken token;
            int i = 1; // index starts at 1 for positioning math reasons
            int amount = 0;

            SetAvailableSpace();

            foreach (var stack in elements)
                if (stack is ElementStackToken)
                    amount++;

            foreach (var stack in elements) {
                token = stack as ElementStackToken;

                if (token == null)
                    continue;

                MoveToPosition(token.transform as RectTransform, GetPositionForIndex(i, amount), moveDuration);

                // make sure all look down
                // TODO: This still flips left-behind stack cards for a useless anim. Right now no way to tell that it was such a card
                token.FlipToFaceDown(true);
                token.ShowGlow(false, true);

                // turn over last card if we're visible
                if (i == amount && token.gameObject.activeInHierarchy)
                    token.FlipToFaceUp();

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