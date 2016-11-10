using System.Collections.Generic;
using Assets.CS.TabletopUI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopToken" base class same as VerbBox
namespace Assets.CS.TabletopUI
{
    public class ElementCard : DraggableToken
    {
	
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GameObject selectedMarker;
        
        [HideInInspector] public ElementDetailsWindow detailsWindow;
        public string ElementId { get {
            return element.Id ?? null;
        } }

        public int Quantity
        {
            get { return _quantity; }
        }

        private Element element;
        private int _quantity;


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

            element = CompendiumHolder.compendium.GetElementById(id);
          SetQuantity(quantity);
     
            name = "Card_" + id;
            if (element == null)
                return;
		
            DisplayInfo();
            DisplayIcon();
        }

        private void DisplayInfo() {
            text.text = element.Label + "(" + Quantity + ")"; // Quantity is a hack, since later on cards always have quantity 1
        }

        private void DisplayIcon() {
            Sprite sprite = ResourcesManager.GetSpriteForElement(element.Id);
            artwork.sprite = sprite;
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
