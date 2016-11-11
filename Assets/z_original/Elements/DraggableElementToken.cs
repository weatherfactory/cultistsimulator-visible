using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// An element on the board. Old, much-refactored code that still needs some refactoring
/// </summary>
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
        Sprite elementSprite = ResourcesManager.GetSpriteForElement(e.Id);
        elementImage.sprite = elementSprite;
    }

    private void displayQuantity(int quantity)
    {
        Text quantityText = GetComponentsInChildren<Text>().Single(t => t.name == "txtQuantity");
        quantityText.text = quantity.ToString();

    }

        public void PopulateForElementId(string elementId,int quantity,Compendium cm)
        {
        Element = cm.GetElementById(elementId);
        DisplayName(Element);
        DisplayIcon(Element);
        _quantity = quantity;
        displayQuantity(_quantity);
    }


        public bool HasChildSlots()
        {
            return Element.HasChildSlots();
        }


    private void TakeElementTokenOutOfStockpile(int quantity)
    {
        transform.SetParent(BM.transform, true);
        BM.ElementOutOfStockpile(Element.Id, quantity);
    }


    public override void OnBeginDrag(PointerEventData eventData)
    {
        

        if (GetComponentInParent<IStockpileLocation>()!=null)
            TakeElementTokenOutOfStockpile(1);
        else
            Assert.IsFalse(_quantity>1); //we aren't set up to cater for a >1 quantity if this hasn't come from a stockpile

        //create a stack to leave behind, of all the element in this token minus one.
        if (_quantity > 1)
        {
            SetQuantity(1); //we never take more than one token out of a stack
        }


        BM.CurrentDragItem = gameObject.GetComponent<DraggableToken>();
        StartPosition = transform.position;
        StartParent = transform.parent;
        if (GetComponent<CanvasGroup>() != null)
            GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

        public override bool DestroyIfContainsElementId(string elementId)
        {
           if(Element.Id==elementId)
        { 
            SetQuantity(0);
            return true;
        }
            return false;
        }


        public override void ReturnToOrigin()
        {
         BM.ReturnElementTokenToStorage(GetComponent<DraggableElementToken>());
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




