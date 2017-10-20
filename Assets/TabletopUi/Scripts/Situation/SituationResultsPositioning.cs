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

        float availableSpace;
        float xStart;

        void SetAvailableSpace() {
            availableSpace = rect.rect.width - margin.x - margin.x;
            xStart = availableSpace / 2f;
        }

        public void ReorderCards(IEnumerable<IElementStack> elements) {
            ElementStackToken token;
            int i = 1; // count starts at 1 for positioning math reasons
            int count = 0;

            SetAvailableSpace();

            foreach (var stack in elements)
                if (stack is ElementStackToken)
                    count++;

            foreach (var stack in elements) {
                token = stack as ElementStackToken;

                if (token == null)
                    continue;

                (token.transform as RectTransform).anchoredPosition = GetPositionForIndex(i, count);

                // make sure all look down
                token.FlipToFaceDown(true);

                // turn over last card
                if (i == count)
                    token.FlipToFaceUp();

                i++;
            }
        }

        Vector2 GetPositionForIndex(int i, int num) {
            return new Vector2(xStart - availableSpace * (i / (float) num) * 0.5f, 0f); 
        }
        
        // TODO: Animate position

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