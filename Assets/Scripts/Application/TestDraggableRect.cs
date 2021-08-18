using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestDraggableRect : MonoBehaviour,IPointerEnterHandler,IBeginDragHandler,IEndDragHandler,IDragHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Entered test rect");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin drag test rect");

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End drag test rect");
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }
}
 