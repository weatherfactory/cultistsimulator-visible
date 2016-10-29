using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


public class DraggableVerbToken : DraggableToken,IPointerClickHandler,INotifyLocator
{
    public Verb Verb;
    public GameObject HomeFrame { get; set; }


    public override void OnBeginDrag(PointerEventData eventData)
    {

        BM.CurrentDragItem = gameObject.GetComponent<DraggableToken>();
        StartPosition = transform.position;
        StartParent = transform.parent;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public override bool DestroyIfContainsElementId(string elementId)
    {
        throw new NotImplementedException();
    }

    public override void ReturnToOrigin()
    {
      transform.SetParent(HomeFrame.transform);
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

