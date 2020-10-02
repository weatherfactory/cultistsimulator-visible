#pragma warning disable 0649
using System;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI {

    public interface IToken
    {
        string EntityId { get; }
        void SetTokenContainer(ITokenContainer newContainer, Context context);
        string name { get; }
        Transform transform { get; }
        RectTransform RectTransform { get; }
        GameObject gameObject { get; }
        void DisplayAtTableLevel();
        void SnapToGrid();
        bool NoPush { get; }

    }


        [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class AbstractToken : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler,
        IGlowableView, IPointerEnterHandler, IPointerExitHandler,IToken,IAnimatable {

        // STATIC FIELDS

        // ReSharper disable once NotAccessedField.Local
        // This is used in DelayedEndDrag, which occurs one frame after EndDrag. If it's set to true, the token will be returned to where it began the drag (default is false).


        // INSTANCE FIELDS

        [HideInInspector] public RectTransform rectTransform;
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
        protected Chronicler subscribedChronicler;

        public RectTransform RectTransform
        {
            get { return rectTransform; }
        }

        bool lastGlowState;
        Color lastGlowColor;

        protected virtual void Awake() {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            lastGlowColor = glowImage.currentColor;
            SubscribeChronicler(Registry.Get<Chronicler>());
        }

        public abstract void StartArtAnimation();

        public abstract IEnumerator DoAnim(float duration, int frameCount, int frameIndex);

        public abstract bool CanAnimate();

        public abstract string EntityId { get; }
        public bool IsBeingAnimated { get; set; }

        public bool Defunct { get; protected set; }
        public bool IsInAir { protected set; get; }
		public bool NoPush { protected set; get; }

        protected virtual bool AllowsDrag() {
            return !IsBeingAnimated;
        }

        protected virtual bool ShouldShowHoverGlow() {
            return !Defunct && TokenContainer != null && !IsBeingAnimated && (TokenContainer.AllowDrag || TokenContainer.AlwaysShowHoverGlow) && AllowsDrag();
        }

		public bool IsGlowing()
		{
		    if (glowImage == null)
		        return false;
			return glowImage.gameObject.activeSelf;
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
                rectTransform.localPosition = new Vector3(x, y);
            }
            get {
                return TokenContainer.GetSaveLocationInfoForDraggable(this) + "_" + Guid.NewGuid();
            }
        }


        public void SubscribeChronicler(Chronicler c)
        {
            subscribedChronicler = c;
        }

		public virtual void SnapToGrid()
		{
			transform.localPosition = Registry.Get<Choreographer>().SnapToGrid( transform.localPosition );
		}

        public virtual void SetTokenContainer(ITokenContainer newContainer, Context context) {
            OldTokenContainer = TokenContainer;
            TokenContainer = newContainer;
        }

        public virtual bool IsInContainer(ITokenContainer compareContainer, Context context) {
            return compareContainer == TokenContainer;
        }


        public void OnBeginDrag(PointerEventData eventData) {
            if (HornedAxe.itemBeingDragged != null) HornedAxe.itemBeingDragged.DelayedEndDrag();

            if (CanDrag(eventData))
                StartDrag(eventData);
        }

        bool CanDrag(PointerEventData eventData) {
            if (!TokenContainer.AllowDrag || !AllowsDrag())
                return false;

            if (HornedAxe.itemBeingDragged != null || HornedAxe.draggingEnabled == false) {
                Debug.LogWarningFormat("AbstractToken: Can not Drag.\nDragging Enabled: {0}, Item Being Dragged: {1}", HornedAxe.draggingEnabled, HornedAxe.itemBeingDragged != null ? HornedAxe.itemBeingDragged.name : "NULL");
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

            HornedAxe.itemBeingDragged = this;
            HornedAxe.dragCamera = eventData.pressEventCamera;
            HornedAxe.SetReturn( true, "start drag" );
            canvasGroup.blocksRaycasts = false;

            DisplayInAir();

            startPosition = rectTransform.localPosition;
            startParent = rectTransform.parent;
            startSiblingIndex = rectTransform.GetSiblingIndex();

			if (rectTransform.anchoredPosition.sqrMagnitude > 0.0f)	// Never store 0,0 as that's a slot position and we never auto-return to slots - CP
			{
	            lastTablePos = rectTransform.anchoredPosition;
			}

            rectTransform.SetParent(Registry.Get<IDraggableHolder>().RectTransform);
            rectTransform.SetAsLastSibling();

            if (useDragOffset) {
                Vector3 pressPos;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Get<IDraggableHolder>().RectTransform, eventData.pressPosition, HornedAxe.dragCamera, out pressPos);
                dragOffset = (startPosition + startParent.position) - pressPos;
            }
            else {
                dragOffset = Vector3.zero;
            }

            SoundManager.PlaySfx("CardPickup");
			TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.Drag, true );

            Registry.Get<LocalNexus>().OnChangeDragStateEvent.Invoke(true);
        }

        

        public void OnDrag(PointerEventData eventData)
		{
            if (HornedAxe.itemBeingDragged != this)
                return;

            if (HornedAxe.draggingEnabled)
			{
                MoveObject(eventData);
            }
            else
			{
                eventData.pointerDrag = null; // cancel the drag
                OnEndDrag(eventData);
            }
        }

        public void OnMove(PointerEventData eventData)
		{
			if (TabletopManager.GetStickyDrag())
			{
				if (HornedAxe.itemBeingDragged == this)
				{
					OnDrag(eventData);
				}
			}
        }

        public void MoveObject(PointerEventData eventData) {
            Vector3 dragPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Get<IDraggableHolder>().RectTransform, eventData.position, HornedAxe.dragCamera, out dragPos);

            // Potentially change this so it is using UI coords and the RectTransform?
            rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f) {
                // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }
        }


        public virtual void OnEndDrag(PointerEventData eventData) {
            // This delays by one frame, because disabling and setting parent in the same frame causes issues
            // Also helps to avoid dropping and picking up in the same click
            if (HornedAxe.itemBeingDragged == this) {
                if (eventData != null)
                    Invoke("DelayedEndDrag", 0f);
                else // if we don't have event data, we're forcing this, so don't wait the frame.
                    DelayedEndDrag();
            }
        }

        // Also called directly if we start a new drag while we have a drag going
        public virtual void DelayedEndDrag() {
            canvasGroup.blocksRaycasts = true;

            if (HornedAxe.resetToStartPos)
                ReturnToStartPosition();

            Registry.Get<LocalNexus>().OnChangeDragStateEvent.Invoke(false);

            // Last call so that when the event hits it's still available
            HornedAxe.itemBeingDragged = null;

			TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.Drag, false );	// There is also a failsafe to catch unexpected aborts of Drag state - CP


            ShowGlow(false, false);
        }

        // In case the object is destroyed 
        protected virtual void AbortDrag() {
            if (HornedAxe.itemBeingDragged != this)
                return;

            Registry.Get<LocalNexus>().OnChangeDragStateEvent.Invoke(false);

            // Last call so that when the event hits it's still available
            HornedAxe.itemBeingDragged = null;
        }

        public void ReturnToStartPosition() {
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
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.SetParent(startParent);
                rectTransform.SetSiblingIndex(startSiblingIndex);
                rectTransform.localPosition = startPosition;
           }
        }

        public abstract void OnDrop(PointerEventData eventData);

        private bool CanInteractWithTokenDroppedOn(AbstractToken token) {
            var element = token as ElementStackToken;

            if (element != null)
                return CanInteractWithTokenDroppedOn(element);
            else
                return CanInteractWithTokenDroppedOn(token as VerbAnchor);
        }

        public abstract bool CanInteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn);
        public abstract bool CanInteractWithTokenDroppedOn(ElementStackToken stackDroppedOn);

        public abstract void InteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn);
        public abstract void InteractWithTokenDroppedOn(ElementStackToken stackDroppedOn);

        #region -- On Click ------------------------------------

        public abstract void OnPointerClick(PointerEventData eventData);

        #endregion
        
        #region -- Move & Retire Token ------------------------------------

        public abstract void ReturnToTabletop(Context context);

        public virtual void DisplayInAir() {
            IsInAir = true;
        }

        public virtual void DisplayAtTableLevel() {
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0f);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
			lastTablePos = rectTransform.anchoredPosition3D;
            IsInAir = false;
            NotifyChroniclerPlacedOnTabletop();
        }

        protected abstract void NotifyChroniclerPlacedOnTabletop();

        public virtual bool Retire() {
            Destroy(gameObject);
            Defunct = true;
            return true;
        }

        #endregion

        #region -- Hover & Glow ------------------------------------

        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (HornedAxe.itemBeingDragged != null && HornedAxe.itemBeingDragged.CanInteractWithTokenDroppedOn(this))
                HornedAxe.itemBeingDragged.ShowHoveringGlow(true);

            // TODO: actual check if need to show the glow - is there a possible action?

            ShowHoverGlow(true);
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (HornedAxe.itemBeingDragged != null)
                HornedAxe.itemBeingDragged.ShowHoveringGlow(false);

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
                if (HornedAxe.itemBeingDragged == this) {
                    // If we're trying to glow the dragged token, then let's just allow us to show it if we want.
                }
                // We're dragging something and our last state was not "this is a legal drop target" glow, then don't show
                else if (HornedAxe.itemBeingDragged != null && !lastGlowState) {
                    show = false;
                }
                // If we can not interact, don't show the hover highlight
                else if (!ShouldShowHoverGlow()) {
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
        
        public IEnumerator PulseGlow()
        {
            ShowHoverGlow(true, false, Color.white);
            yield return new WaitForSeconds(0.5f);
            ShowHoverGlow(false);
        }

        #endregion
    }
}