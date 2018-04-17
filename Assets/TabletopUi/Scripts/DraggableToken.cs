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

namespace Assets.CS.TabletopUI {
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class DraggableToken : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler,
        IGlowableView, IPointerEnterHandler, IPointerExitHandler {

        // STATIC FIELDS

        public static event System.Action<bool> onChangeDragState;

        private static Camera dragCamera;
        public static DraggableToken itemBeingDragged;
        public static bool draggingEnabled = true;
        private static bool resetToStartPos = false;
        // This is used in DelayedEndDrag, which occurs one frame after EndDrag. If it's set to true, the token will be returned to where it began the drag (default is false).

        public static void SetReturn(bool value, string reason = "") {
            resetToStartPos = value;
            //log here if necessary
        }


        // INSTANCE FIELDS

        [HideInInspector] public RectTransform RectTransform;
        [SerializeField] protected bool useDragOffset = true;
        [SerializeField] protected bool rotateOnDrag = true;
        [SerializeField] protected GraphicFader glowImage;
        [HideInInspector] public Vector2? lastTablePos = null; // if it was pulled from the table, save that position

        protected Transform startParent;
        protected Vector3 startPosition;
        protected int startSiblingIndex;
        protected Vector3 dragOffset;
        protected RectTransform rectCanvas;
        protected CanvasGroup canvasGroup;

        private float perlinRotationPoint = 0f;
        private float dragHeight = -8f; // Draggables all drag on a specifc height and have a specific "default height"

        public ITokenContainer TokenContainer;
        protected ITokenContainer OldTokenContainer; // Used to tell OldContainsTokens that this thing was dropped successfully
        protected INotifier notifier;

        bool lastGlowState;
        Color lastGlowColor;

        protected virtual void Awake() {
            RectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            lastGlowColor = glowImage.currentColor;
        }

        #region -- Basic Getters ------------------------------------

        public abstract string Id { get; }
        public bool IsBeingAnimated { get; set; }

        public bool Defunct { get; protected set; }
        public bool IsInAir { protected set; get; }

        protected virtual bool AllowsDrag() {
            return !IsBeingAnimated;
        }

        protected virtual bool AllowsInteraction() {
            return !Defunct && TokenContainer != null && !IsBeingAnimated && TokenContainer.AllowDrag && AllowsDrag();
        }


        /// <summary>
        /// This is an underscore-separated x, y localPosition in the current transform/containsTokens
        /// but could be anything
        /// </summary>
        public string SaveLocationInfo {
            set {
                var locs = value.Split('_');
                var x = float.Parse(locs[0]);
                var y = float.Parse(locs[1]);
                RectTransform.localPosition = new Vector3(x, y);
            }
            get {
                return TokenContainer.GetSaveLocationInfoForDraggable(this) + "_" + Guid.NewGuid();
            }
        }

        #endregion

        #region -- Basic Setters ------------------------------------

        public void SubscribeNotifier(INotifier n) {
            notifier = n;
        }

        public virtual void SetTokenContainer(ITokenContainer newContainer, Context context) {
            OldTokenContainer = TokenContainer;
            TokenContainer = newContainer;
        }

        public virtual bool IsInContainer(ITokenContainer compareContainer, Context context) {
            return compareContainer == TokenContainer;
        }

        #endregion

        #region -- Begin Drag ------------------------------------

        public void OnBeginDrag(PointerEventData eventData) {
            if (itemBeingDragged != null)
                itemBeingDragged.DelayedEndDrag();

            if (CanDrag(eventData))
                StartDrag(eventData);
        }

        bool CanDrag(PointerEventData eventData) {
            if (!TokenContainer.AllowDrag || !AllowsDrag())
                return false;

            if (itemBeingDragged != null || draggingEnabled == false) {
                Debug.LogWarningFormat("DraggableToken: Can not Drag.\nDragging Enabled: {0}, Item Being Dragged: {1}", draggingEnabled, itemBeingDragged != null ? itemBeingDragged.name : "NULL");
                return false;
            }

            // pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
            if (eventData != null && eventData.pointerId < -1)
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

            DisplayInAir();

            startPosition = RectTransform.position;
            startParent = RectTransform.parent;
            startSiblingIndex = RectTransform.GetSiblingIndex();

            lastTablePos = RectTransform.anchoredPosition;

            RectTransform.SetParent(Registry.Retrieve<IDraggableHolder>().RectTransform);
            RectTransform.SetAsLastSibling();

            if (useDragOffset) {
                Vector3 pressPos;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Retrieve<IDraggableHolder>().RectTransform, eventData.pressPosition, DraggableToken.dragCamera, out pressPos);
                dragOffset = startPosition - pressPos;
            }
            else {
                dragOffset = Vector3.zero;
            }

            SoundManager.PlaySfx("CardPickup");

            if (onChangeDragState != null)
                onChangeDragState(true);
        }

        #endregion
        
        #region -- Do Drag ------------------------------------

        public void OnDrag(PointerEventData eventData) {
            if (itemBeingDragged != this)
                return;

            if (draggingEnabled) {
                MoveObject(eventData);
            }
            else {
                eventData.pointerDrag = null; // cancel the drag
                OnEndDrag(eventData);
            }
        }

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

        #endregion

        #region -- End Drag ------------------------------------

        public static void CancelDrag() {
            if (itemBeingDragged == null)
                return;

            if (itemBeingDragged.gameObject.activeInHierarchy)
                itemBeingDragged.DelayedEndDrag();
        }

        public virtual void OnEndDrag(PointerEventData eventData) {
            // This delays by one frame, because disabling and setting parent in the same frame causes issues
            // Also helps to avoid dropping and picking up in the same click
            if (itemBeingDragged == this) {
                if (eventData != null)
                    Invoke("DelayedEndDrag", 0f);
                else // if we don't have event data, we're forcing this, so don't wait the frame.
                    DelayedEndDrag();
            }
        }

        // Also called directly if we start a new drag while we have a drag going
        protected virtual void DelayedEndDrag() {
            canvasGroup.blocksRaycasts = true;

            if (DraggableToken.resetToStartPos)
                ReturnToStartPosition();

            if (onChangeDragState != null)
                onChangeDragState(false);

            // Last call so that when the event hits it's still available
            DraggableToken.itemBeingDragged = null;

            ShowGlow(false, false);
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

        private void ReturnToStartPosition() {
            if (startParent == null) {
                //newly created token! If we try to set it to startposition, it'll disappear into strange places
                ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
                return; // no sound on new token
            }

            SoundManager.PlaySfx("CardDragFail");
            var tabletopContainer = startParent.GetComponent<TabletopTokenContainer>();
            
            // Token was from tabletop - return it there. This auto-merges it back in case of ElementStacks
            // The map is not the tabletop but inherits from it, so we do the IsTabletop check
            if (tabletopContainer != null && tabletopContainer.IsTabletop) {
                ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }
            else {
                RectTransform.position = startPosition;
                RectTransform.localRotation = Quaternion.identity;
                RectTransform.SetParent(startParent);
                RectTransform.SetSiblingIndex(startSiblingIndex);
            }
        }

        #endregion

        #region -- On Drop ------------------------------------

        public abstract void OnDrop(PointerEventData eventData);

        private bool CanInteractWithTokenDroppedOn(DraggableToken token) {
            var element = token as IElementStack;

            if (element != null)
                return CanInteractWithTokenDroppedOn(element);
            else
                return CanInteractWithTokenDroppedOn(token as SituationToken);
        }

        public abstract bool CanInteractWithTokenDroppedOn(SituationToken tokenDroppedOn);
        public abstract bool CanInteractWithTokenDroppedOn(IElementStack stackDroppedOn);

        public abstract void InteractWithTokenDroppedOn(SituationToken tokenDroppedOn);
        public abstract void InteractWithTokenDroppedOn(IElementStack stackDroppedOn);

        #endregion

        #region -- On Click ------------------------------------

        public abstract void OnPointerClick(PointerEventData eventData);

        #endregion
        
        #region -- Move & Retire Token ------------------------------------

        public abstract void ReturnToTabletop(Context context);

        public virtual void DisplayInAir() {
            IsInAir = true;
        }

        public virtual void DisplayAtTableLevel() {
            RectTransform.anchoredPosition3D = new Vector3(RectTransform.anchoredPosition3D.x, RectTransform.anchoredPosition3D.y, 0f);
            RectTransform.localRotation = Quaternion.identity;
            RectTransform.localScale = Vector3.one;
            IsInAir = false;
        }

        public virtual bool Retire() {
            Destroy(gameObject);
            Defunct = true;
            return true;
        }

        #endregion

        #region -- Hover & Glow ------------------------------------

        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged != null && DraggableToken.itemBeingDragged.CanInteractWithTokenDroppedOn(this))
                DraggableToken.itemBeingDragged.ShowHoveringGlow(true);

            // TODO: actual check if need to show the glow - is there a possible action?

            ShowHoverGlow(true);
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged != null)
                DraggableToken.itemBeingDragged.ShowHoveringGlow(false);

            ShowHoverGlow(false);
        }

        public virtual void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

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

        // Used when a dragged object is hovering something
        public virtual void ShowHoveringGlow(bool show) {
            // always use default color for the "draggable-item-can-be-dropped" hover glow
            // never trigger SFX, since the token you're hovering over already does that, 
            // since that allows us to use the default hover glow for click-hover with sound too
            ShowHoverGlow(show, false, UIStyle.brightPink);
        }

        // Separate method from ShowGlow so we can restore the last state when unhovering
        protected virtual void ShowHoverGlow(bool show, bool playSFX = true, Color? hoverColor = null) {
            if (show) {
                if (DraggableToken.itemBeingDragged == this) {
                    // If we're trying to glow the dragged token, then let's just allow us to show it if we want.
                }
                // We're dragging something and our last state was not "this is a legal drop target" glow, then don't show
                else if (DraggableToken.itemBeingDragged != null && !lastGlowState) {
                    show = false;
                }
                // If we can not interact, don't show the hover highlight
                else if (!AllowsInteraction()) {
                    show = false;
                }
            }

            if (show) {
                if (playSFX)
                    SoundManager.PlaySfx("TokenHover");

                glowImage.SetColor(hoverColor == null ? UIStyle.GetGlowColor(UIStyle.TokenGlowColor.OnHover) : hoverColor.Value);
                glowImage.Show();
            }
            else {
                //if (playSFX)
                //    SoundManager.PlaySfx("TokenHoverOff");

                glowImage.SetColor(lastGlowColor);

                if (lastGlowState) 
                    glowImage.Show();
                else  
                    glowImage.Hide();
            }
        }

        #endregion
    }
}