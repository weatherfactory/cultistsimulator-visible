using System.Collections.Generic;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI
{
    [RequireComponent (typeof (RectTransform))]
    [RequireComponent (typeof (CanvasGroup))]
    public abstract class DraggableToken : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,IDropHandler, IPointerClickHandler
    {
	
        public static event System.Action<bool> onChangeDragState;

        public static bool draggingEnabled = true;
        public static DraggableToken itemBeingDragged;
        public static bool resetToStartPos = true; // Maybe change draggable so it doesn't reset by default. makes dragging around the base case. Only force non-reset on actions.
        private static Camera dragCamera;
        private static RectTransform draggableHolder;

        protected Transform startParent;
        protected Vector3 startPosition;
        protected int startSiblingIndex;
        protected Vector3 dragOffset;
        public RectTransform RectTransform;
        protected RectTransform rectCanvas;
        protected CanvasGroup canvasGroup;
        public bool IsSelected { protected set; get; }
        

        private float perlinRotationPoint = 0f;
        private float dragHeight = -5f;
        // Draggables all drag on a specifc height and have a specific "default height"

        public bool rotateOnDrag = true;
        protected Notifier notifier;
        protected ITokenContainer container;
 

        void Awake() {
            RectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            draggableHolder = GameObject.FindGameObjectWithTag("DraggableHolder").transform as RectTransform;
        }

        public abstract string Id { get; }

        public void SubscribeNotifier(Notifier n)
        {
            notifier = n;
        }

        public virtual void SetContainer(ITokenContainer newContainer)
        {
            container = newContainer;
        }

        public void OnBeginDrag (PointerEventData eventData) {
            if (CanDrag(eventData))
                StartDrag(eventData);
        }

        

        bool CanDrag(PointerEventData eventData)
        {
            if (!container.AllowDrag)
                return false;

            if ( itemBeingDragged != null || draggingEnabled == false )
                return false;
		
            // pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
            if ( eventData.pointerId < -1 ) 
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
		
            RectTransform.SetParent(draggableHolder);
            RectTransform.SetAsLastSibling();
		
            Vector3 pressPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(draggableHolder, eventData.pressPosition, DraggableToken.dragCamera, out pressPos);
            dragOffset = startPosition - pressPos;

            if (onChangeDragState != null)
                onChangeDragState(true);

            container.TokenPickedUp(this);


        }

        public void OnDrag (PointerEventData eventData) {
            if (itemBeingDragged == this) 
                MoveObject(eventData);
        }

        public void ReturnToTabletop(INotification reason)
        {
            notifier.TokenReturnedToTabletop(this,reason);
        }

        // Would solve this differently: By sending the object the drag position and allowing it to position itself as it desires
        // This allows us to animate a "moving up" animation while you're dragging

        public void MoveObject(PointerEventData eventData) {
            Vector3 dragPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(draggableHolder, eventData.position, DraggableToken.dragCamera, out dragPos);

            // Potentially change this so it is using UI coords and the RectTransform?
            RectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);
		
            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f) {			
                // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // This delays by one frame, because disabling and setting parent in the same frame causes issues
            // Also helps to avoid dropping and picking up in the same click
            if (itemBeingDragged == this)
                Invoke ("DelayedEndDrag", 0f);
        }

        void OnDisable() {
            OnEndDrag(null);
        }

        void DelayedEndDrag() {
            DraggableToken.itemBeingDragged = null;
            canvasGroup.blocksRaycasts = true;
		
            if (DraggableToken.resetToStartPos) {
                RectTransform.position = startPosition;
                RectTransform.SetParent(startParent);
                RectTransform.SetSiblingIndex(startSiblingIndex);
            }
		
            if (onChangeDragState != null)
                onChangeDragState(false);
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            
        }

        public virtual void InteractWithTokenDroppedOn(SituationToken tokenDroppedOn)
        {

        }

        public virtual void InteractWithTokenDroppedOn(IElementStack stackDroppedOn)
        {
            
        }

        public virtual bool Retire()
        {
            Destroy(gameObject);
            return true;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
            if (eventData.pointerId >= -1)
            {
                container.TokenInteracted(this);
            }
        }
    }
}