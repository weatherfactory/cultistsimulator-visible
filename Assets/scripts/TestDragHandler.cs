using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TestDragHandler : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler
{
    private Vector3 startPosition;
    public static GameObject itemBeingDragged;


    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("began drag");
        itemBeingDragged = gameObject;
        startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        transform.position = startPosition;
        
    }
}
