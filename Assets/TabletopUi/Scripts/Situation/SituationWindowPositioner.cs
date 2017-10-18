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
        public Transform token;

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

        // GENERAL MOVE BEHAVIOR

        public void SetToTokenPos() {
            SetPosition(token.position);

            // Check if one of our corners is outside the bounds
            var corners = GetCornerPos();
            float xOffset = 0f;
            float yOffset = 0f;
            Vector2 screenMargin = new Vector2(50f, 50f);

            for (int i = 0; i < corners.Length; i++) {                
                var screenPos = GetScreenPosFromWorld(corners[i]);

                // Note: This would not behave properly if multiple corners would be outside on both bounds. Can't happen tho
                if (screenPos.x > Screen.width) {
                    xOffset = Mathf.Min(Screen.width - screenMargin.x - screenPos.x, xOffset);
                }
                else if (screenPos.x < 0f) { 
                    xOffset = Mathf.Max(screenMargin.x - screenPos.x, xOffset);
                }

                if (screenPos.y > Screen.height) {
                    yOffset = Mathf.Min(Screen.height - screenMargin.y - screenPos.y, yOffset);
                }
                else if (screenPos.y < 0f) {
                    yOffset = Mathf.Max(screenMargin.y - screenPos.y, yOffset);
                }
            }

            // We have an offset? Shift the window position!
            if (xOffset != 0f || yOffset != 0f) { 
                Debug.Log("Offset to put back " + xOffset + ", " + yOffset);
                var screenPos = GetScreenPosFromWorld(rectTrans.position);
                screenPos += new Vector2(xOffset, yOffset);
                SetPosition(GetWorldPosFromScreen(screenPos));
            }
        }

        Vector3[] GetCornerPos() {
            var corners = new Vector3[4];

            corners[0] = rectTrans.position + Vector3.Scale(new Vector3(rectTrans.rect.x, rectTrans.rect.y, 0f), rectTrans.lossyScale);
            corners[1] = rectTrans.position + Vector3.Scale(new Vector3(rectTrans.rect.x + rectTrans.rect.width, rectTrans.rect.y, 0f), rectTrans.lossyScale);
            corners[2] = rectTrans.position + Vector3.Scale(new Vector3(rectTrans.rect.x + rectTrans.rect.width, rectTrans.rect.y + rectTrans.rect.height, 0f), rectTrans.lossyScale);
            corners[3] = rectTrans.position + Vector3.Scale(new Vector3(rectTrans.rect.x, rectTrans.rect.y + rectTrans.rect.height, 0f), rectTrans.lossyScale);

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

        void SetPosition(Vector3 pos) {
            rectTrans.position = new Vector3(pos.x, pos.y, rectTrans.position.z);
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