using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Should inherit from a "TabletopToken" base class same as VerbBox
namespace Assets.CS.TabletopUI
{
    public class ElementCard : MonoBehaviour, IElementQuantityDisplay {
	
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GameObject selectedMarker;

        public string elementId { private set; get; }
        [HideInInspector] public ElementDetailsWindow detailsWindow;

        public void SetElement(string id, int quantity) {
            name = "Card_" + id;

            elementId = id;
            var element = CompendiumHolder.compendium.GetElementById(id);

            if (element == null)
                return;
		
            DisplayName(element, quantity);
            DisplayIcon(element);
            SetSelected(false);
        }

        private void DisplayName(Element e, int quantity) {
            text.text = e.Label + "(" + quantity + ")"; // Quantity is a hack, since later on cards always have quantity 1
        }

        private void DisplayIcon(Element e) {
            Sprite sprite = ResourcesManager.GetSpriteForElement(e.Id);
            artwork.sprite = sprite;
        }

        public void SetSelected(bool isSelected) {
            selectedMarker.gameObject.SetActive(isSelected);
        }

        public Sprite GetSprite() {
            return artwork.sprite;
        }

        public void ElementQuantityUpdate(string elementId, int currentQuantityInStockpile, int workspaceQuantityAdjustment) {
            throw new System.NotImplementedException();
        }
    }
}
