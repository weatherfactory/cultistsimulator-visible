using Assets.CS.TabletopUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI { 
    /// <summary>
    /// Class that manages the movement of the situation Window. Tries to stick to token, stay on screen and remain draggable
    /// </summary>
    public class SituationWindowPositioner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

        public bool canDrag = true;
        Transform token;

        static SituationWindowPositioner windowBeingDragged;
        
        RectTransform rectTrans;
        CanvasGroup canvasGroup;

        RectTransform parentTrans;
        Camera uiCamera;

        Vector3 dragOffset;

        public void Initialise(Transform token) {
            this.token = token;
            this.rectTrans = GetComponent<RectTransform>();
            this.canvasGroup = GetComponent<CanvasGroup>();
            this.parentTrans = GetComponentInParent<RectTransform>();
            this.uiCamera = Camera.main; // there is only one camera in our scene so this works.

            SetToTokenPos();
        }

        Vector2 GetScreenPosFromWorld(Vector3 worldPos) {
            return RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);
        }

        // SHOW ANIM

        public void Show(float duration, Vector3 targetPosOverride) {
            StopAllCoroutines();
			Vector3 pos = token.position;
			if (targetPosOverride.sqrMagnitude > 0.0f)	// Ugly, but no way to pass a null Vector3 reference in C#, so using zero vec as "invalid" - CP
				pos = targetPosOverride;
            StartCoroutine(DoShowAnim(duration, pos));
        }

        IEnumerator DoShowAnim(float duration, Vector3 targetPosition)
		{
			TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.WindowAnim, true );
            var time = 0f;
            var targetPos = GetBoundCorrectedWorldPos(targetPosition);
            var startPos = token.position;
            float lerp;

            while (time < duration) {
                time += Time.deltaTime;
                lerp = Easing.Circular.Out(time / duration);
                transform.localScale = new Vector3(lerp, lerp, lerp);
                SetPosition(Vector3.Lerp(startPos, targetPos, lerp));
                yield return null;
            }

            transform.localScale = Vector3.one;
            SetPosition(targetPos);
			TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.WindowAnim, false );
        }

        // GENERAL MOVE BEHAVIOR

        public void SetToTokenPos() {
            SetToWorldPosInScreenBounds(token.position);
        }

        void SetToWorldPosInScreenBounds(Vector3 worldPos) {
            SetPosition(GetBoundCorrectedWorldPos(worldPos));
        }

        Vector3 GetBoundCorrectedWorldPos(Vector3 worldPos) {
            // Check if one of our corners would be outside the bounds
            var outOfBoundsOffset = GetScreenPosOffsetForCornerOverlap(token.position);

            // We have an offset? Shift the window position!
            if (outOfBoundsOffset.x != 0f || outOfBoundsOffset.y != 0f) {
                var screenPos = GetScreenPosFromWorld(worldPos);
                screenPos += outOfBoundsOffset;
                return GetWorldPosFromScreen(screenPos);
            }
            else {
                return worldPos;
            }
        }

        Vector2 GetScreenPosOffsetForCornerOverlap(Vector3 pos) {
            float xBoundUpper = Screen.width - 50f;
            float xBoundLower = 0f + 50f;
            float yBoundUpper = Screen.height - 30f; 
            float yBoundLower = 0f + 80f; // more margin than other y-end because of status bar

            Vector2 offset = Vector2.zero;
            var corners = GetCornerPos(pos);

            for (int i = 0; i < corners.Length; i++) {
                var screenPos = GetScreenPosFromWorld(corners[i]);

                // Note: This would not behave properly if multiple corners would be outside on both bounds. Can't happen tho
                if (screenPos.x > xBoundUpper) {
                    offset.x = Mathf.Min(xBoundUpper - screenPos.x, offset.x);
                }
                else if (screenPos.x < xBoundLower) {
                    offset.x = Mathf.Max(xBoundLower - screenPos.x, offset.x);
                }

                if (screenPos.y > yBoundUpper) {
                    offset.y = Mathf.Min(yBoundUpper - screenPos.y, offset.y);
                }
                else if (screenPos.y < yBoundLower) {
                    offset.y = Mathf.Max(yBoundLower - screenPos.y, offset.y);
                }
            }

            return offset;
        }

        Vector3[] GetCornerPos(Vector3 pos) {
            var corners = new Vector3[4];

            corners[0] = pos + Vector3.Scale(new Vector3(rectTrans.rect.x, rectTrans.rect.y, 0f), rectTrans.lossyScale);
            corners[1] = pos + Vector3.Scale(new Vector3(rectTrans.rect.x + rectTrans.rect.width, rectTrans.rect.y, 0f), rectTrans.lossyScale);
            corners[2] = pos + Vector3.Scale(new Vector3(rectTrans.rect.x + rectTrans.rect.width, rectTrans.rect.y + rectTrans.rect.height, 0f), rectTrans.lossyScale);
            corners[3] = pos + Vector3.Scale(new Vector3(rectTrans.rect.x, rectTrans.rect.y + rectTrans.rect.height, 0f), rectTrans.lossyScale);

            return corners;
        }
        
        // DRAG BEHAVIOR

        public void OnBeginDrag(PointerEventData eventData) {
            if (CanDrag(eventData))
                StartDrag(eventData);
        }

        bool CanDrag(PointerEventData eventData) {
            if (!canDrag)
                return false;
            // pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
            else if (eventData.pointerId < -1)
                return false;
            // Is the player dragging another draggable token?
            else if (DraggableToken.itemBeingDragged != null)
                return false;
            // Is the player dragging another draggable window?
            else if (windowBeingDragged != null)
                return false;

            return true;
        }

        void StartDrag(PointerEventData eventData) {
            windowBeingDragged = this;
            canvasGroup.blocksRaycasts = false;

            Vector3 pressPos = GetWorldPosFromScreen(eventData.pressPosition);
            dragOffset = rectTrans.position - pressPos;
        }

        Vector3 GetWorldPosFromScreen(Vector2 screenPos) {
            Vector3 pressPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(parentTrans, screenPos, uiCamera, out pressPos);
            return pressPos;
        }

        public void OnDrag(PointerEventData eventData) {
            if (windowBeingDragged == this)
                MoveObject(eventData);
        }

        void MoveObject(PointerEventData eventData) {
            if (!canDrag) {
                DelayedEndDrag();
                return;
            }

            Vector3 dragPos = GetWorldPosFromScreen(eventData.position);
            SetPosition(new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y));
        }

		// Raw position get/set are exposed to allow saving of window coords - CP
        public void SetPosition(Vector3 pos) {
            rectTrans.position = new Vector3(pos.x, pos.y, rectTrans.position.z);
        }

        public Vector3 GetPosition() {
            return rectTrans.position;
        }

        public virtual void OnEndDrag(PointerEventData eventData) {
            // This delays by one frame, because disabling and setting parent in the same frame causes issues
            // Also helps to avoid dropping and picking up in the same click
            if (windowBeingDragged == this)
                Invoke("DelayedEndDrag", 0f);
        }

        void OnDisable() {
            OnEndDrag(null);
        }

        void DelayedEndDrag() {
            windowBeingDragged = null;
            canvasGroup.blocksRaycasts = true;
        }
    }
}