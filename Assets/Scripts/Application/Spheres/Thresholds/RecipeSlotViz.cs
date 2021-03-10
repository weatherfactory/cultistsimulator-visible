#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Enums;
using UnityEngine;

namespace SecretHistories.UI {
    public class RecipeSlotViz : MonoBehaviour {

        public RectTransform rectTrans;
        [SerializeField] ThresholdSphere slot;
        [SerializeField] Animation anim;

        bool isHidden;

        const float minDistToMove = 2f;
        Vector2 lastPos;
        Vector2 targetPos;
        float moveTime;
        [SerializeField] Easing.EaseType easeTypeX = Easing.EaseType.CircularInOut;
        [SerializeField] Easing.EaseType easeTypeY = Easing.EaseType.SinusoidalInOut;
        Coroutine moving;

        // SHOW / HIDE

        public void TriggerShowAnim() {
            anim.Play("recipe-slot-show");
        }


        public IEnumerator TriggerHideAnim(Action onComplete) {
            isHidden = true;

            anim.Play("recipe-slot-hide");

            while (anim.isPlaying)
            {
                yield return null;
            };
            
            onComplete();
        }

        private void OnDisable() {
            anim["recipe-slot-hide"].time = 0f;
            anim["recipe-slot-hide"].enabled = true;
            anim["recipe-slot-hide"].weight = 1;
            anim.Sample();
            anim["recipe-slot-hide"].enabled = false;
        }


        // POSITION

        public void SetPosition(Vector2 pos) {
            rectTrans.anchoredPosition = pos;
            lastPos = pos;
        }

        public void MoveToPosition(Vector2 pos, float duration)
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (lastPos == pos || Vector2.Distance(pos, lastPos) < minDistToMove)
                return;
            
            targetPos = pos;
            moveTime = 0f;
            gameObject.SetActive(true);
            if (moving == null)
            {
                moving = StartCoroutine(DoMove(duration));
            }

            // otherwise the changed pos will automatically move the thing to where it has to go
        } 

        IEnumerator DoMove(float duration) {
            float easeX, easeY;
            float lerp;

            while (moveTime < duration) { 
                yield return null;
                lerp = moveTime / duration;
                easeX = Easing.Ease(easeTypeX, lerp);
                easeY = Easing.Ease(easeTypeY, lerp);
                rectTrans.anchoredPosition = new Vector2(Mathf.Lerp(lastPos.x, targetPos.x, easeX),
                                        Mathf.Lerp(lastPos.y, targetPos.y, easeY));
                moveTime += Time.deltaTime;
            }

            SetPosition(targetPos);
            moving = null;
        }

    }
 }