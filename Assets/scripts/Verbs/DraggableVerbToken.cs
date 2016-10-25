using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


public class DraggableVerbToken : DraggableToken,IPointerClickHandler,INotifyLocator
{
    public Verb Verb;
  

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (OriginTransform == null)
        {
            if (transform.parent)
                OriginTransform = transform.parent; //so we can return this to its original slot later
        }
        BM.CurrentDragItem = gameObject.GetComponent<DraggableToken>();
        StartPosition = transform.position;
        StartParent = transform.parent;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = false;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        BM.Notify(Verb.Label, Verb.Description, gameObject.GetComponent<INotifyLocator>());
    }

    public Vector3 GetNotificationPosition()
    {
        Vector3 v3 = transform.position;
        v3.x += 80;
        v3.y -= 100;
        return v3;
    }
}

