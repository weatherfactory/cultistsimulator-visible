using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent (typeof (RectTransform))]
[RequireComponent (typeof (CanvasGroup))]
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	
	public static event System.Action<bool> onChangeDragState;

	public static bool draggingEnabled = true;
	public static Draggable itemBeingDragged;
	public static bool resetToStartPos = true; // Maybe change draggable so it doesn't reset by default. makes dragging around the base case. Only force non-reset on actions.
	private static Camera dragCamera;
	private static RectTransform draggableHolder;

	protected Transform startParent;
	protected Vector3 startPosition;
	protected int startSiblingIndex;
	protected Vector3 dragOffset;
	protected RectTransform rectTransform;
	protected RectTransform rectCanvas;
	protected CanvasGroup canvasGroup;

	private float perlinRotationPoint = 0f;
	private float dragHeight = -5f;
	// Draggables all drag on a specic height and have a specific "default height"

	public bool rotateOnDrag = true;


	void Awake() {
		rectTransform = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();
		draggableHolder = GameObject.FindGameObjectWithTag("DraggableHolder").transform as RectTransform;
	}

	public void OnBeginDrag (PointerEventData eventData) {
		if (CanDrag(eventData))
			StartDrag(eventData);
    }

	bool CanDrag(PointerEventData eventData) {
		if ( itemBeingDragged != null || draggingEnabled == false )
			return false;
		
		// pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
        if ( eventData.pointerId < -1 ) 
            return false;

		return true;
	}

	void StartDrag(PointerEventData eventData) {
		if (rectCanvas == null)
			rectCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>(); 

		Draggable.itemBeingDragged = this;
        Draggable.resetToStartPos = true;
		Draggable.dragCamera = eventData.pressEventCamera;
		canvasGroup.blocksRaycasts = false;
		
		startPosition = rectTransform.position;
		startParent = rectTransform.parent;
		startSiblingIndex = rectTransform.GetSiblingIndex();
		
		rectTransform.SetParent(draggableHolder);
		rectTransform.SetAsLastSibling();
		
		Vector3 pressPos;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(draggableHolder, eventData.pressPosition, Draggable.dragCamera, out pressPos);
        dragOffset = startPosition - pressPos;

        if (onChangeDragState != null)
            onChangeDragState(true);
	}

	public void OnDrag (PointerEventData eventData) {
		if (itemBeingDragged == this) 
			MoveObject(eventData);
	}

	// Would solve this differently: By sending the object the drag position and allowing it to position itself as it desires
	// This allows us to animate a "moving up" animation while you're dragging

	public void MoveObject(PointerEventData eventData) {
		Vector3 dragPos;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(draggableHolder, eventData.position, Draggable.dragCamera, out dragPos);

		// Potentially change this so it is using UI coords and the RectTransform?
		rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);
		
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
		Draggable.itemBeingDragged = null;
		canvasGroup.blocksRaycasts = true;
		
		if (Draggable.resetToStartPos) {
			rectTransform.position = startPosition;
			rectTransform.SetParent(startParent);
			rectTransform.SetSiblingIndex(startSiblingIndex);
		}
		
		if (onChangeDragState != null)
			onChangeDragState(false);
	}
}