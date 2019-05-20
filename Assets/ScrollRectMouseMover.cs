using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.TabletopUi.Scripts.Infrastructure;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectMouseMover : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler  {
	
	ScrollRect scrollRect;
    // Vector4 order is Top, Right, Bottom, Left
#pragma warning disable 649
    [Range(0.01f, 0.49f)]
	[SerializeField] float edgeAreaSize = 0.1f;
	[SerializeField] Vector4 edgePadding;

	[SerializeField] float minAcceleration = 100f;
	[SerializeField] float maxAcceleration = 1000f;
	[SerializeField] float maxVelocity = 2000f;

	[SerializeField] float timeout = 0.1f;
#pragma warning restore 649
#pragma warning disable 414
	bool pointerInRect;
#pragma warning restore 414
	float pointerEnterEdgeTime = 0f;
	bool isManualDragActive;
	bool blockScrolling;

	Vector2 mousePos;
	Vector4 innerBounds;
	Vector2 marginVect;
	float magnitude;

	void Start() {
		scrollRect = GetComponent<ScrollRect>();
		// TODO: Disable on touch?

		marginVect = new Vector2();

		if (Screen.width < Screen.height) {
			marginVect.x = edgeAreaSize;
			marginVect.y = edgeAreaSize * Screen.width / Screen.height;
		}
		else {
			marginVect.x = edgeAreaSize * Screen.height / Screen.width;
			marginVect.y = edgeAreaSize;
		}
		
		innerBounds = new Vector4(0.5f - edgePadding.x - marginVect.y, 0.5f - edgePadding.y - marginVect.x, -0.5f + edgePadding.z + marginVect.y, -0.5f + edgePadding.w + marginVect.x);
	}

	public void OnPointerEnter(PointerEventData eventData) {
		pointerInRect = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		pointerInRect = false;
	}

	public void OnBeginDrag(PointerEventData eventData) {
		if (scrollRect.isActiveAndEnabled)
			isManualDragActive = true;
	}

	public void OnEndDrag(PointerEventData eventData) {
		isManualDragActive = false;
	}

	void Update()
	{
		// We are dragging manually? then block this thing and stop
		if (isManualDragActive)
		{
			blockScrolling = true;
			return;
		}
		// We're pressing a hotkey? Then move!
		if (PressingMoveKey())
		{
			mousePos = GetMousePosFromKeys();
			magnitude = 3f;
			pointerEnterEdgeTime = timeout;
		}
		// Pointer is in our rect? Then move
		else if (Assets.CS.TabletopUI.DraggableToken.itemBeingDragged!=null)
		{
			// point ranging from (-0.5, -0.5) to (0.5, 0.5)
			mousePos = new Vector2(Input.mousePosition.x / Screen.width - 0.5f, Input.mousePosition.y / Screen.height - 0.5f);
			SetMagnitudeFromMouse();
		}
		// We got neither a button nor a pointer? Nothing.
		else
		{
			blockScrolling = false; // enable scrolling starting with the next frame
			return;
		}

		// We are not in a zone? Then stop doing this and unblock us if needed
		if (Mathf.Approximately(magnitude, 0f))
		{
			blockScrolling = false; // enable scrolling starting with the next frame
			pointerEnterEdgeTime = 0f;	
			return;
		}
		// We are blocked - IE we had a manual drag and we're still in the scroll-zone?
		else if (blockScrolling)
		{
			return;
		}

		// Increment our edgeTimer if we're not over the timeout
		if (pointerEnterEdgeTime < timeout)
		{
			pointerEnterEdgeTime += Time.deltaTime;

			// Still not enough, then get us out of here
			if (pointerEnterEdgeTime < timeout) 
				return;
		}

		magnitude = Mathf.Lerp(minAcceleration, maxAcceleration, magnitude);

		// -1f to invert the vector 
		var vector = mousePos.normalized * magnitude * -1f * Time.deltaTime;

		// Set horizontal velocity
		// if we change direction, do so immediately! Otherwise add on top up to our max speed.
		if (Mathf.Sign(vector.x) != Mathf.Sign(scrollRect.velocity.x))
			vector.x = Mathf.Min(vector.x, maxVelocity);
		else 
			vector.x = Mathf.Min(scrollRect.velocity.x + vector.x, maxVelocity);

		if (Mathf.Sign(vector.y) != Mathf.Sign(scrollRect.velocity.y))
			vector.y = Mathf.Min(vector.y, maxVelocity);
		else 
			vector.y = Mathf.Min(scrollRect.velocity.y + vector.y, maxVelocity);

		// Push the velocity into the scrollRect
		scrollRect.velocity = vector;
	}

	bool PressingMoveKey()
	{
	    return ((int) Input.GetAxis("Horizontal") != 0 || (int) Input.GetAxis("Vertical") != 0);
	    //	return (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow));
	}

	void SetMagnitudeFromMouse() {
		magnitude = 0f;
		// Vertical
		// up
		if (mousePos.y > innerBounds.x) {
			magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.y - innerBounds.x) / marginVect.y);
		}
		// down
		else if (mousePos.y < innerBounds.z) {
			magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.y - innerBounds.z) / marginVect.y);
		}

		// Horizontal
		// right
		if (mousePos.x > innerBounds.y) {
			magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.x - innerBounds.y) / marginVect.x);
		}
		// left
		else if (mousePos.x < innerBounds.w) {
			magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.x - innerBounds.w) / marginVect.x);
		}
	}

	Vector2 GetMousePosFromKeys()
	{
		if (HotkeyWatcher.IsInInputField())
			return Vector2.zero;

		float y;
		float x;
		/*
		if (Input.GetKey(KeyCode.UpArrow))
			y = 0.5f;
		else if (Input.GetKey(KeyCode.DownArrow))
			y = -0.5f;
		else 
			y = 0f;
		*/
		y = Input.GetAxis( "Vertical" );

		/*
		if (Input.GetKey(KeyCode.RightArrow)) 
			x = 0.5f;
		else if (Input.GetKey(KeyCode.LeftArrow)) 
			x = -0.5f;
		else 
			x = 0f;
		*/
		x = Input.GetAxis( "Horizontal" );

		return new Vector2(x, y);
	}

}
