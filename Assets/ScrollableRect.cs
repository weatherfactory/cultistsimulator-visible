using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ScrollRect))]
public class ScrollableRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler,ISphereEventSubscriber  {
	
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
	bool ManualScrollRectDragIsActive;
	bool blockScrolling;

	Vector2 mousePos;
	Vector4 innerBounds;
	Vector2 marginVect;
	float magnitude;

    private float currentTruckInput;
    private float currentPedestalInput;
    private const float KEY_MOVEMENT_EFFECT_MULTIPLIER=90f;


    void Start() {

        Registry.Get<SphereCatalogue>().Subscribe(this);


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
		//OnBeginDrag fired on the scrollrect itself: we're dragging it directly
		if (scrollRect.isActiveAndEnabled)
			ManualScrollRectDragIsActive = true;
	}

	public void OnEndDrag(PointerEventData eventData) {
        if (scrollRect.isActiveAndEnabled)
            scrollRect.velocity = Vector2.zero;
    }

    public void OnTruckEvent(TruckEventArgs args)
    {
        currentTruckInput = args.CurrentTruckInput * KEY_MOVEMENT_EFFECT_MULTIPLIER;
        mousePos = new Vector2(args.CurrentTruckInput, currentPedestalInput);
        }

    public void OnPedestalEvent(PedestalEventArgs args)
    {
        currentPedestalInput = args.CurrentPedestalInput * KEY_MOVEMENT_EFFECT_MULTIPLIER;
        mousePos = new Vector2(currentTruckInput, currentPedestalInput);

    }


    void Update()
    {
        //This still isn't working perfectly - I don't understand the differing decelerations when using keys -
        //but I'm going to move on and come back to it

        if (Mathf.Approximately(magnitude, 0f))
        {
            pointerEnterEdgeTime = 0f;
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


    public void OnTokenDragged(TokenEventArgs args)
    {

        // if we're dragging a token, check if the mouse is in the scroll zone near the edge of the screen.

            // point ranging from (-0.5, -0.5) to (0.5, 0.5)
            mousePos = new Vector2(Pointer.current.position.x.ReadValue() / Screen.width - 0.5f, Pointer.current.position.y.ReadValue() / Screen.height - 0.5f);
            SetMagnitudeFromMouse();
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


    public void NotifyTokensChangedForContainer(TokenEventArgs args)
    {
        //
    }

    public void OnTokenClicked(TokenEventArgs args)
    {
     //
    }

    public void OnTokenReceivedADrop(TokenEventArgs args)
    {
     //
    }

    public void OnTokenPointerEntered(TokenEventArgs args)
    {
     //
    }

    public void OnTokenPointerExited(TokenEventArgs args)
    {
      //
    }

    public void OnTokenDoubleClicked(TokenEventArgs args)
    {
      //
    }


}
