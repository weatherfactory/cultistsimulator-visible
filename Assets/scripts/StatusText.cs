using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ContentClasses;
using OrbCreationExtensions;
using UnityEngine.EventSystems;

public class StatusText : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public static GameObject ItemBeingDragged;
    private Vector3 startPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Got it!");
        ItemBeingDragged = gameObject; // the script is a component; all components have a parent gameObject property
        startPosition = transform.position; //and most gameObjects have a transform; this goes to the parent gameObject's transform, components don't have a transform
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ItemBeingDragged = null;
        transform.position = startPosition;
    }
}
