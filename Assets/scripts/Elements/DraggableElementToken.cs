using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DraggableElementToken: DraggableToken,IPointerClickHandler,INotifyLocator
    {

    public Element Element { get; set; }
    public int Quantity { get { return _quantity; } }
    private int _quantity;

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
        Sprite elementSprite = ContentRepository.Instance.GetSpriteForElement(e.Id);
        elementImage.sprite = elementSprite;
    }

    private void displayQuantity(int quantity)
    {
        Text quantityText = GetComponentsInChildren<Text>().Single(t => t.name == "txtQuantity");
        quantityText.text = quantity.ToString();

    }

        public void PopulateForElementId(string elementId,int change,ContentRepository cm)
        {
        Element = cm.GetElementById(elementId);
        DisplayName(Element);
        DisplayIcon(Element);
        _quantity = change;
        displayQuantity(_quantity);
    }


        public bool HasChildSlots()
        {
            return Element.ChildSlotSpecifications.Count > 0;
        }


    private void TakeElementTokenOutOfResourcesPanel()
    {
        transform.SetParent(BM.transform, true);
    }


    public override void OnBeginDrag(PointerEventData eventData)
    {
        //create a stack to leave behind, of all the element in this token minus one.
        if (OriginTransform == null)
        {
            if (_quantity > 1)
            {
                int quantityRemaining = _quantity - 1;
                int siblingIndexForNewStack = transform.GetSiblingIndex();
                SetQuantity(1);
                TakeElementTokenOutOfResourcesPanel();

                BM.ModifyElementQuantity(Element.Id,quantityRemaining, siblingIndexForNewStack);
               
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
         BM.ReturnElementTokenToStorage(GetComponent<DraggableElementToken>());
        }


        public void ModifyQuantity(int change)
        {
        _quantity += change;
        if (_quantity <= 0)

          BM.ExileToLimboThenDestroy(gameObject);
        displayQuantity(_quantity);
    }
    public void SetQuantity(int value)
    {
        _quantity = value;
        if (_quantity <= 0)
            BM.ExileToLimboThenDestroy(gameObject);
        displayQuantity(_quantity);
    }


        public void OnPointerClick(PointerEventData eventData)
        {
           BM.Notify(Element.Label,Element.Description, gameObject.GetComponent<INotifyLocator>());
        }

        public Vector3 GetNotificationPosition()
        {
            Vector3 v3 = transform.position;
            v3.x += 130;
            return v3;
        }
    }




