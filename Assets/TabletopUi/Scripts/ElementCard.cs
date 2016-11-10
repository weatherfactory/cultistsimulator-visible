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
    public class ElementCard : DraggableToken,IElementCard
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

        public void SetQuantity(int quantity)
        {
            _quantity = quantity;
            if (quantity <= 0)
            {
                DestroyObject(gameObject);
                return;
            }
            DisplayInfo();
        }

        public void SetElement(string id, int quantity) {

            _element = CompendiumHolder.compendium.GetElementById(id);
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
            ElementCard droppedCard = DraggableToken.itemBeingDragged as ElementCard;
            ElementCard droppedOnCard = this as ElementCard;
            if (droppedOnCard != null && droppedCard != null && droppedOnCard.ElementId == droppedCard.ElementId)
            {
                droppedOnCard.SetQuantity(droppedOnCard.Quantity + droppedCard.Quantity);
                DraggableToken.resetToStartPos = false;
                droppedCard.SetQuantity(0);
            }
        }






    }
}
