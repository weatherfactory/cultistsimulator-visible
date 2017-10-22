using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public class RecipeSlotViz : MonoBehaviour {

        public RectTransform rectTrans;
        [SerializeField] RecipeSlot slot;
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

        void ShowDefault() {
            anim["recipe-slot-hide"].time = 0f;
            anim["recipe-slot-hide"].enabled = true;
            anim["recipe-slot-hide"].weight = 1;
            anim.Sample();
            anim["recipe-slot-hide"].enabled = false;
        }

        public void TriggerHideAnim() {
            isHidden = true;
            slot.Defunct = true;
            anim.Play("recipe-slot-hide");
        }

        private void OnDisable() {
            if (isHidden)
                OnHideEnd();
            else
                ShowDefault();
        }

        public void OnHideEnd() {
            slot.Retire();
        }

        // POSITION

        public void SetPosition(Vector2 pos) {
            rectTrans.anchoredPosition = pos;
        }

        public void MoveToPosition(Vector2 pos, float duration) {
            if (lastPos == pos || Vector2.Distance(pos, lastPos) < minDistToMove)
                return;

            lastPos = rectTrans.anchoredPosition;
            targetPos = pos;
            moveTime = 0f;

            if (moving == null)
                moving = StartCoroutine(DoMove(duration));
        } 

        IEnumerator DoMove(float duration) {
            float easeX, easeY;
            float lerp;

            while (moveTime < duration) { 
                yield return null;
                lerp = moveTime / duration;
                easeX = Easing.Ease(easeTypeX, lerp);
                easeY = Easing.Ease(easeTypeY, lerp);
                SetPosition(new Vector2(Mathf.Lerp(lastPos.x, targetPos.x, easeX),
                                        Mathf.Lerp(lastPos.y, targetPos.y, easeY)));
                moveTime += Time.deltaTime;
            }

            SetPosition(targetPos);
            moving = null;
        }

    }
 }