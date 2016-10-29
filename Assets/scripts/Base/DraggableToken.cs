using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;



public abstract class DraggableToken : BoardMonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Vector3 StartPosition;
    public Transform StartParent;

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
    
    }

    public abstract Boolean DestroyIfContainsElementId(string elementId);

    public virtual void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }


    public virtual void OnEndDrag(PointerEventData eventData)
    {
        BM.CurrentDragItem = null;
        if (transform.parent == StartParent)
            transform.position = StartPosition;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = true;
    }


    public abstract void ReturnToOrigin();

}
