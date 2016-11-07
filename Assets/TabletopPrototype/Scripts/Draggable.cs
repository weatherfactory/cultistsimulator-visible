using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent (typeof (RectTransform))]
[RequireComponent (typeof (CanvasGroup))]
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	
	public static event System.Action<bool> onChangeDragState;

	public static bool draggingEnabled = true;
	public static Draggable itemBeingDragged;
	public static bool resetToStartPos = true;
	private static Camera dragCamera;


	protected Transform startParent;
	protected Vector3 startPosition;
	protected int startSiblingIndex;
	protected Vector3 dragOffset;
	protected RectTransform rectTransform;
	protected RectTransform rectCanvas;
	protected CanvasGroup canvasGroup;

	private float perlinRotationPoint = 0f;

	void Awake() {
		rectTransform = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();
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
		
		rectTransform.SetParent(rectCanvas);
		rectTransform.SetAsLastSibling();
		
		Vector3 pressPos;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(rectCanvas, eventData.pressPosition, Draggable.dragCamera, out pressPos);
        dragOffset = startPosition - pressPos;
        
        if (onChangeDragState != null)
            onChangeDragState(true);
	}

	public void OnDrag (PointerEventData eventData) {
		if (itemBeingDragged == this) 
			MoveObject(eventData);
	}

	public void MoveObject(PointerEventData eventData) {
		Vector3 dragPos;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(rectCanvas, eventData.position, Draggable.dragCamera, out dragPos);
		rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, startPosition.z);
		
		// rotate object slightly based on pointer Delta
		if (eventData.delta.sqrMagnitude > 10f) {			
			// This needs some tweaking
			perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, -5 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 10));
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