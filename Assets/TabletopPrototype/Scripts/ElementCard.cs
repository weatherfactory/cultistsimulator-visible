using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

// Should inherit from a "TabletopToken" base class same as VerbBox
public class ElementCard : MonoBehaviour, IElementQuantityDisplay {
	
	[SerializeField] Image artwork;
	[SerializeField] TextMeshProUGUI text;

	public string elementId { private set; get; }
	public ElementDetailsWindow detailsWindow;

	public void SetElement(string id, int quantity, Compendium cm) {
		name = "Card_" + id;

		elementId = id;
		var element = cm.GetElementById(id);

		if (element == null)
			return;
		
		DisplayName(element, quantity);
		DisplayIcon(element);
	}

	private void DisplayName(Element e, int quantity) {
		text.text = e.Label + "(" + quantity + ")"; // Quantity is a hack, since later on cards always have quantity 1
	}

	private void DisplayIcon(Element e) {
		Sprite sprite = ResourcesManager.GetSpriteForElement(e.Id);
		artwork.sprite = sprite;
	}

	public void ElementQuantityUpdate(string elementId, int currentQuantityInStockpile, int workspaceQuantityAdjustment) {
		throw new System.NotImplementedException();
	}
}
