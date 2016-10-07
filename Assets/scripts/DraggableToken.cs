using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DraggableToken : BoardMonoBehaviour, IDragHandler,IBeginDragHandler,IEndDragHandler
{
    private Vector3 startPosition;
    private Transform startParent;
    public Transform originSlot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!originSlot)
        { 
            if(transform.parent)
            originSlot = transform.parent; //so we can return this to its original slot later
        }
        BM.itemBeingDragged = gameObject;
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
        BM.itemBeingDragged = null;
        if(transform.parent==startParent)
        transform.position = startPosition;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void ReturnToOrigin()
    {
        transform.SetParent(originSlot);
    }
}
