using System.Collections.Generic;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopToken" base class same as VerbBox
namespace Assets.CS.TabletopUI
{
    public class ElementStack : DraggableToken,IElementStack
    {
	
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GameObject selectedMarker;
        private Element _element;
        private int _quantity;

        public string ElementId { get {
            return _element==null ? null : _element.Id;
        } }

        public int Quantity
        {
            get { return _quantity; }
        }

        public bool Defunct { get; private set; }

        public void SetQuantity(int quantity)
        {
            _quantity = quantity;
            if (quantity <= 0)
            {
                Remove();
                return;
            }
            DisplayInfo();
        }


        public void ModifyQuantity(int change)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove()
        {
            DestroyObject(gameObject);
            if (Defunct)
                return false;
            Defunct = true;
                return true;
        }

        public void SetElement(string id, int quantity) {

            _element = Registry.compendium.GetElementById(id);
          SetQuantity(quantity);
     
            name = "Card_" + id;
            if (_element == null)
                return;
		
            DisplayInfo();
            DisplayIcon();
        }

        private void DisplayInfo() {
            text.text = _element.Label + "(" + Quantity + ")"; 
        }

        private void DisplayIcon() {
            Sprite sprite = ResourcesManager.GetSpriteForElement(_element.Id);
            artwork.sprite = sprite;
        }

        public Dictionary<string,int> GetAspects()
        {
            return _element.AspectsIncludingSelf;
        }


        public Sprite GetSprite() {
            return artwork.sprite;
        }

        public override void OnDrop(PointerEventData eventData)
        {
            ElementStack droppedCard = DraggableToken.itemBeingDragged as ElementStack;
            ElementStack droppedOnStack = this as ElementStack;
            if (droppedOnStack != null && droppedCard != null && droppedOnStack.ElementId == droppedCard.ElementId)
            {
                droppedOnStack.SetQuantity(droppedOnStack.Quantity + droppedCard.Quantity);
                DraggableToken.resetToStartPos = false;
                droppedCard.SetQuantity(0);
            }
        }






    }
}
