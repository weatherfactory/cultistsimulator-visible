using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DraggableElementToken: DraggableToken
    {

    public Element Element { get; set; }

        public void Awake()
        {
            Element=new Element("","","");
        }

        public void DisplayName(Element e)
    {
        Text nameText = GetComponentsInChildren<Text>().Single(t => t.name == "txtElementName");
        nameText.text = e.Label;
    }

    public void DisplayIcon(Element e)
    {
        Image elementImage = GetComponentsInChildren<Image>().Single(i => i.name == "imgElementIcon");
        Sprite elementSprite = Resources.Load<Sprite>("FlatIcons/png/32px/" + e.Id);
        elementImage.sprite = elementSprite;
    }

    public void DisplayQuantity(int quantity)
    {
        Text quantityText = GetComponentsInChildren<Text>().Single(t => t.name == "txtQuantity");
        quantityText.text = quantity.ToString();

    }

        public void PopulateForElementId(string elementId,ContentRepository cm)
        {
        Element = cm.PopulateElementForId(elementId);
        DisplayName(Element);
        DisplayIcon(Element);
    }


        public bool HasChildSlots()
        {
            return Element.ChildSlots.Count > 0;
        }


  

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (OriginTransform == null)
        {
            StorageSlot originSlot = transform.parent.gameObject.GetComponent<StorageSlot>();
            if (originSlot != null) //if we've just removed the token from a StorageSlot
            {
                transform.SetParent(BM.transform, true);
                originSlot.SplitContents(1);
                DraggableElementToken elementToken = GetComponentInChildren<DraggableElementToken>();
                elementToken.DisplayQuantity(1);
            }
         
             OriginTransform = transform.parent; //so we can return this to its original slot later
            
        }
        BM.CurrentDragItem = gameObject.GetComponent<DraggableToken>();
        StartPosition = transform.position;
        StartParent = transform.parent;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

        public override void ReturnToOrigin()
        {
            base.ReturnToOrigin();
        }



}




