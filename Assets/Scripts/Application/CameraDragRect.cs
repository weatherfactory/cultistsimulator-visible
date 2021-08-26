using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Enums;
using SecretHistories.Events;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using Vector2 = UnityEngine.Vector2;
using Vector4 = UnityEngine.Vector4;

public class CameraDragRect : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler,ISphereCatalogueEventSubscriber  {
	
	//ScrollRect scrollRect;
    // Vector4 order is Top, Right, Bottom, Left
#pragma warning disable 649
	[SerializeField] private float driftAfterDrag;


    

#pragma warning restore 649
#pragma warning disable 414
	bool pointerInRect;

#pragma warning restore 414

    Vector2 firstMousePos;
    Vector2 lastMousePos;
    Vector2 mousePos;
	

    private Vector2 lastChangeVector;
    
    

    void Start() {

        Watchman.Get<HornedAxe>().Subscribe(this);
    }

	public void OnPointerEnter(PointerEventData eventData) {
		pointerInRect = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		pointerInRect = false;
	}

	public void OnBeginDrag(PointerEventData eventData)
    {
        firstMousePos = eventData.position;
        lastMousePos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        mousePos = eventData.position;
        if (lastMousePos != mousePos)
        {
            lastChangeVector = lastMousePos - mousePos;
            Watchman.Get<CamOperator>().ApplySmoothInputVector(lastChangeVector,0f);
            lastMousePos = mousePos;
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 finalChangevector = firstMousePos - eventData.position;
        Watchman.Get<CamOperator>().ApplySmoothInputVector(finalChangevector * driftAfterDrag);
    }




    void Update()
    {
        //This still isn't working perfectly - I don't understand the differing decelerations when using keys -
        //but I'm going to move on and come back to it

        //if (Mathf.Approximately(magnitude, 0f))
        //{
        //    pointerEnterEdgeTime = 0f;
        //    return;
        //}

        //// Increment our edgeTimer if we're not over the timeout
        //if (pointerEnterEdgeTime < timeout)
        //{
        //    pointerEnterEdgeTime += Time.deltaTime;

        //    // Still not enough, then get us out of here
        //    if (pointerEnterEdgeTime < timeout)
        //        return;
        //}

        //magnitude = Mathf.Lerp(minAcceleration, maxAcceleration, magnitude);

        //// -1f to invert the vector 
        //var vector = mousePos.normalized * magnitude * -1f * Time.deltaTime;

        //// Set horizontal velocity
        //// if we change direction, do so immediately! Otherwise add on top up to our max speed.
        //if (Mathf.Sign(vector.x) != Mathf.Sign(scrollRect.velocity.x))
        //    vector.x = Mathf.Min(vector.x, maxVelocity);
        //else 
        //    vector.x = Mathf.Min(scrollRect.velocity.x + vector.x, maxVelocity);

        //if (Mathf.Sign(vector.y) != Mathf.Sign(scrollRect.velocity.y))
        //    vector.y = Mathf.Min(vector.y, maxVelocity);
        //else 
        //    vector.y = Mathf.Min(scrollRect.velocity.y + vector.y, maxVelocity);

        //// Push the velocity into the scrollRect
        //scrollRect.velocity = vector;
	}





 //   void SetMagnitudeFromMouse() {
	//	magnitude = 0f;
	//	// Vertical
	//	// up
	//	if (mousePos.y > innerBounds.x) {
	//		magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.y - innerBounds.x) / marginVect.y);
	//	}
	//	// down
	//	else if (mousePos.y < innerBounds.z) {
	//		magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.y - innerBounds.z) / marginVect.y);
	//	}

	//	// Horizontal
	//	// right
	//	if (mousePos.x > innerBounds.y) {
	//		magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.x - innerBounds.y) / marginVect.x);
	//	}
	//	// left
	//	else if (mousePos.x < innerBounds.w) {
	//		magnitude = Mathf.Max(magnitude, Mathf.Abs(mousePos.x - innerBounds.w) / marginVect.x);
	//	}
	//}


    public void OnSphereChanged(SphereChangedArgs args)
    {
        //
    }

    public void OnTokensChanged(SphereContentsChangedEventArgs args)
    {
        //
    }

    public void OnTokenInteraction(TokenInteractionEventArgs args)
    {
        if(args.Interaction==Interaction.OnDrag)
        {
            // if we're dragging a token, check if the mouse is in the scroll zone near the edge of the screen.

            // point ranging from (-0.5, -0.5) to (0.5, 0.5)
        //    mousePos = new Vector2(Pointer.current.position.x.ReadValue() / Screen.width - 0.5f, Pointer.current.position.y.ReadValue() / Screen.height - 0.5f);
        //    SetMagnitudeFromMouse();
        }
    }



}
