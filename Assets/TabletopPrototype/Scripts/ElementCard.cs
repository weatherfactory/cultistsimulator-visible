using System.Collections.Generic;
using Assets.CS.TabletopUI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Should inherit from a "TabletopToken" base class same as VerbBox
namespace Assets.CS.TabletopUI
{
    public class ElementCard : Draggable {
	
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GameObject selectedMarker;
        
        [HideInInspector] public ElementDetailsWindow detailsWindow;
        public string ElementId { get {
            return element.Id ?? null;
        } }
        public int Quantity { private set; get; }
        private Element element;


        public void SetElement(string id, int quantity) {

            element = CompendiumHolder.compendium.GetElementById(id);
            Quantity = quantity;
            if (quantity <= 0)
            {
                DestroyObject(gameObject);
                return;
            }
            name = "Card_" + id;
            if (element == null)
                return;
		
            DisplayName();
            DisplayIcon();
            SetSelected(false);
        }

        private void DisplayName() {
            text.text = element.Label + "(" + Quantity + ")"; // Quantity is a hack, since later on cards always have quantity 1
        }

        private void DisplayIcon() {
            Sprite sprite = ResourcesManager.GetSpriteForElement(element.Id);
            artwork.sprite = sprite;
        }

        public void SetSelected(bool isSelected) {
            selectedMarker.gameObject.SetActive(isSelected);
        }

        public Sprite GetSprite() {
            return artwork.sprite;
        }


    }
}
