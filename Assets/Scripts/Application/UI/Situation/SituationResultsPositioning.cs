#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SecretHistories.Fucine;


namespace SecretHistories.UI {
    public class SituationResultsPositioning : MonoBehaviour {

        [SerializeField] RectTransform rectTransform;
        [SerializeField] Vector2 margin = new Vector2(50f, 40f);

        //float moveDuration = 0.2f;
        float availableSpace;
        float startingX;
        //IEnumerable<ElementStackToken> elements

        void SetAvailableSpace() {
            availableSpace = rectTransform.rect.width - (margin.x*2);
            startingX = availableSpace / 2f;
        }


        public void ArrangeTokens(IEnumerable<Token> elementTokens) {
            var sortedTokens = SortStacks(elementTokens);

            int i = 1; // index starts at 1 for positioning math reasons
            
            SetAvailableSpace();

            string debugText = "Reorder Results: ";


            foreach (var token in sortedTokens)
            {

                var positionForThisToken = GetPositionForIndex(i, sortedTokens.Count);
                token.TokenRectTransform.anchoredPosition = positionForThisToken;

               // MoveToPosition(token.TokenRectTransform,positionForThisToken, 0f);
                token.transform.SetSiblingIndex(i-1); //each card is conceptually on top of the last. Set sibling index to make sure they appear that way.


                i++;
            }

            NoonUtility.Log(debugText,0,VerbosityLevel.Trivia);
        }

        List<Token> SortStacks(IEnumerable<Token> elementStacks) {
			var hiddenStacks = new List<Token>();	// Hidden fresh cards
            var freshStacks = new List<Token>();	// Face-up fresh cards
            var existingStacks = new List<Token>(); // Face-up existing cards

            foreach (var stack in elementStacks)
			{
                if (stack.Shrouded())
				{
					if (stack.Payload.GetTimeshadow().Transient)
	                    freshStacks.Add(stack);
					else
						hiddenStacks.Add(stack);
				}
                else
                    existingStacks.Add(stack);
            }

			hiddenStacks.AddRange(freshStacks); //fresh face-up stacks go after hidden stacks
            hiddenStacks.AddRange(existingStacks); //existing stacks go after fresh stacks

            return hiddenStacks;
        }

        Vector2 GetPositionForIndex(int currentTokenIndex, float totalTokensCount) {

            var interposition= currentTokenIndex / totalTokensCount;

            return new Vector2(startingX - availableSpace * interposition * 0.5f, 0f); 
        }

        //public void MoveToPosition(RectTransform rectTrans, Vector2 pos, float duration) {
        //    // If we're disabled or our token is, just set us there
        //    if (rectTrans.gameObject.activeInHierarchy == false || gameObject.activeInHierarchy == false) {
        //        rectTrans.anchoredPosition = pos;
        //        return;
        //    }

        //    if (rectTrans.anchoredPosition == pos || Vector2.Distance(pos, rectTrans.anchoredPosition) < 0.2f)
        //        return;

        //    StartCoroutine(DoMove(rectTrans, pos, duration));
        //}


        
        //IEnumerator DoMove(RectTransform rectTrans, Vector2 targetPos, float duration) {
        //    float ease;
        //    float lerp;
        //    float time = 0f;
        //    Vector2 lastPos = rectTrans.anchoredPosition;

        //    while (time < duration) {
        //        lerp = time / duration;
        //        time += Time.deltaTime;
        //        ease = Easing.Ease(Easing.EaseType.SinusoidalInOut, lerp);
        //        rectTrans.anchoredPosition = Vector2.Lerp(lastPos, targetPos, ease);
        //        yield return null;
        //    }

        //    rectTrans.anchoredPosition = targetPos;
        //}

    }
}