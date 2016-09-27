using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler
{
    private Vector3 startPosition;
    public static GameObject itemBeingDragged;
    private Transform startParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeingDragged = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        if(transform.parent==startParent)
        transform.position = startPosition;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
