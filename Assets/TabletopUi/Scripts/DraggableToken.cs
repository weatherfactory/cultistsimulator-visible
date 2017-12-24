#pragma warning disable 0649
using System;
using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI
{
    [RequireComponent (typeof (RectTransform))]
    [RequireComponent (typeof (CanvasGroup))]
    public abstract class DraggableToken : MonoBehaviour, 
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, 
        IGlowableView, IPointerEnterHandler, IPointerExitHandler
    {
	
        public static event System.Action<bool> onChangeDragState;

        public static bool draggingEnabled = true;
        public static DraggableToken itemBeingDragged;
        /// <summary>
        /// This is used in DelayedEndDrag, which occurs one frame after EndDrag. If it's set to true, the token will be returned to where it began the drag (default is false).
        /// </summary>
        private static bool resetToStartPos = false;

        public static void SetReturn(bool value, string reason = "")
        {
            resetToStartPos = value;
            //log here if necessary
        }

        private static Camera dragCamera;

        public bool Defunct { get; protected set; }
        protected virtual bool AllowDrag { get { return true; } }
        protected Transform startParent;
        protected Vector3 startPosition;
        protected int startSiblingIndex;
        protected Vector3 dragOffset;
        public RectTransform RectTransform;
        protected RectTransform rectCanvas;
        protected CanvasGroup canvasGroup;
        public bool IsSelected { protected set; get; }

        public Vector3? lastTablePos = null; // if it was pulled from the table, save that position

        private float perlinRotationPoint = 0f;
        private float dragHeight = -5f;
        // Draggables all drag on a specifc height and have a specific "default height"

        public bool rotateOnDrag = true;
        protected INotifier notifier;
        public IContainsTokens containsTokens;
        public IContainsTokens OldContainsTokens; // Used to tell OldContainsTokens that this thing was dropped successfully

        [SerializeField] GraphicFader glowImage;

        protected virtual void Awake() {
            RectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public abstract string Id { get; }

        /// <summary>
        /// This is an underscore-separated x,y localPosition in the current transform/containsTokens
        /// but could be anything
        /// </summary>
        public string SaveLocationInfo
        {
            set
            {
                var locs = value.Split('_');
                var x = float.Parse(locs[0]);
                var y = float.Parse(locs[1]);
                RectTransform.localPosition = new Vector3(x, y);

            }
            get { return containsTokens.GetSaveLocationInfoForDraggable(this) +"_" + Guid.NewGuid(); }
        }


        public void SubscribeNotifier(INotifier n)
        {
            notifier = n;
        }

        public virtual void SetContainer(IContainsTokens newContainsTokens)
        {
            OldContainsTokens = containsTokens; 
            containsTokens = newContainsTokens;
        }

        public virtual bool IsInContainer(IContainsTokens candidateContainsTokens)
        {
            return candidateContainsTokens == containsTokens;
        }

        public void OnBeginDrag (PointerEventData eventData) {
           if (CanDrag(eventData))
                StartDrag(eventData);
        }

        bool CanDrag(PointerEventData eventData)
        {
            if (!containsTokens.AllowDrag || !AllowDrag)
                return false;

            if ( itemBeingDragged != null || draggingEnabled == false )
               return false;
		
            // pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
            if (eventData != null && eventData.pointerId < -1 ) 
                return false;

            return true;
        }

        protected virtual void StartDrag(PointerEventData eventData) {
            if (rectCanvas == null)
                rectCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>(); 

            DraggableToken.itemBeingDragged = this;
            DraggableToken.resetToStartPos = true;
            DraggableToken.dragCamera = eventData.pressEventCamera;
            canvasGroup.blocksRaycasts = false;

            startPosition = RectTransform.position;
            startParent = RectTransform.parent;
            startSiblingIndex = RectTransform.GetSiblingIndex();
		
            RectTransform.SetParent(Registry.Retrieve<IDraggableHolder>().RectTransform);
            RectTransform.SetAsLastSibling();
		
            Vector3 pressPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Retrieve<IDraggableHolder>().RectTransform, eventData.pressPosition, DraggableToken.dragCamera, out pressPos);
            dragOffset = startPosition - pressPos;

            SoundManager.PlaySfx("CardPickup");

            if (onChangeDragState != null)
                onChangeDragState(true);
        }

        public void OnDrag (PointerEventData eventData) {
            if (itemBeingDragged == this) 
                MoveObject(eventData);
        }

        public abstract void ReturnToTabletop(INotification reason = null);
    

        // Would solve this differently: By sending the object the drag position and allowing it to position itself as it desires
        // This allows us to animate a "moving up" animation while you're dragging

        public void MoveObject(PointerEventData eventData) {
            Vector3 dragPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Retrieve<IDraggableHolder>().RectTransform, eventData.position, DraggableToken.dragCamera, out dragPos);

            // Potentially change this so it is using UI coords and the RectTransform?
            RectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);
		
            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f) {			
                // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }
        }

        protected virtual void OnDisable()
        {
            //OnEndDrag(null);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            // This delays by one frame, because disabling and setting parent in the same frame causes issues
            // Also helps to avoid dropping and picking up in the same click
            if (itemBeingDragged == this && eventData != null)
                Invoke ("DelayedEndDrag", 0f);
        }



        protected virtual void DelayedEndDrag() {
            canvasGroup.blocksRaycasts = true;
		
            if (DraggableToken.resetToStartPos) 
                returnToStartPosition();

            OldContainsTokens = null;

            if (onChangeDragState != null)
                onChangeDragState(false);

            // Last call so that when the event hits it's still available
            DraggableToken.itemBeingDragged = null;
        }

        // In case the object is destroyed
        protected virtual void AbortDrag() {
            if (itemBeingDragged != this)
                return;

            if (onChangeDragState != null)
                onChangeDragState(false);

            // Last call so that when the event hits it's still available
            DraggableToken.itemBeingDragged = null;
        }

        private void returnToStartPosition() {
            if (startParent == null) {
                //newly created token! If we try to set it to startposition, it'll disappear into strange places
                ReturnToTabletop(null);
                return; // no sound on new token
            }

            SoundManager.PlaySfx("CardDragFail");

            if (startParent.GetComponent<TabletopContainsTokens>()) {
                //Token was from tabletop - return it there. This auto-merges it back in case of ElementStacks
                ReturnToTabletop(null);
            }
            else {
                RectTransform.position = startPosition;
                RectTransform.SetParent(startParent);
                RectTransform.SetSiblingIndex(startSiblingIndex);
            }
        }

        public abstract void OnDrop(PointerEventData eventData);

        public abstract void InteractWithTokenDroppedOn(SituationToken tokenDroppedOn);

        public virtual void InteractWithTokenDroppedOn(IElementStack stackDroppedOn)
        {
            DraggableToken.SetReturn(true);
        }

        public virtual bool Retire()
        {
            Destroy(gameObject);
            Defunct = true;
            return true;
        }

        public abstract void OnPointerClick(PointerEventData eventData);

        public void DisplayOnTable()
        {
            RectTransform.anchoredPosition3D = new Vector3(RectTransform.anchoredPosition3D.x, RectTransform.anchoredPosition3D.y, 0f);
            RectTransform.localRotation = Quaternion.identity;
        }

        public void DisplayInAir()
        {
            transform.SetAsLastSibling();
            float windowZOffset = -10f;

            RectTransform.anchoredPosition3D = new Vector3(RectTransform.anchoredPosition3D.x, RectTransform.anchoredPosition3D.y, windowZOffset);
            RectTransform.localRotation = Quaternion.Euler(0f, 0f, RectTransform.eulerAngles.z);
        }

        // Hover & Glow

        public virtual void OnPointerEnter(PointerEventData eventData) {
            ShowHoverGlow(true);
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            ShowHoverGlow(false);
        }

        public virtual void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
         }

        bool lastGlowState;
        Color lastGlowColor;

        public virtual void SetGlowColor(Color color) {
            glowImage.SetColor(color);
            lastGlowColor = color;
        }

        public virtual void ShowGlow(bool glowState, bool instant = false) {
            lastGlowState = glowState;

            if (glowState)
                glowImage.Show(instant);
            else
                glowImage.Hide(instant);
        }

        // Separate method from ShowGlow so we can restore the last state when unhovering
        protected virtual void ShowHoverGlow(bool show) {
            // We're dragging something and our last state was not "this is a legal drop target" glow, then don't show
            if (DraggableToken.itemBeingDragged != null && !lastGlowState)
                show = false;
            // If we can not drag, don't show the hover highlight
            else if (!containsTokens.AllowDrag || !AllowDrag)
                show = false;

            if (show) {
                SoundManager.PlaySfx("TokenHover");
                glowImage.SetColor(UIStyle.GetGlowColor(UIStyle.TokenGlowColor.Hover));
                glowImage.Show(true);
            }
            else {
                glowImage.SetColor(lastGlowColor);

                if (lastGlowState)
                    glowImage.Show(true);
                else
                    glowImage.Hide(true);
            }
        }

    }
}