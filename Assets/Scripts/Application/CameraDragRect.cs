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
            Watchman.Get<CamOperator>().ApplySmoothInputVector(lastChangeVector);
            lastMousePos = mousePos;
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 finalChangevector = firstMousePos - eventData.position;
        Watchman.Get<CamOperator>().ApplySmoothInputVector(finalChangevector * driftAfterDrag);
    }



    


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
