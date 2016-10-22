using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


public class DraggableVerbToken : DraggableToken
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
    

 
}

