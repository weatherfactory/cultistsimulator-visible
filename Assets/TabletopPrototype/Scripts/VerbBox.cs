using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class VerbBox : MonoBehaviour, IPointerClickHandler {
	public event System.Action<VerbBox> onVerbBoxClicked;

	[SerializeField] Image artwork;
	[SerializeField] TextMeshProUGUI text;

	public string verbId { private set; get; }
	public RecipeDetailsWindow detailsWindow;

	public void SetVerb(string id, Compendium cm) {
		name = "Verb_" + id;

		verbId = id;
		var verb = cm.GetVerbById(id);

		if (verb != null)
			SetVerb(verb);
	}

	public void SetVerb(Verb verb) {
		name = "Verb" + verb.Id;

		if (verb == null)
			return;

		DisplayName(verb);
		DisplayIcon(verb);
	}

	private void DisplayName(Verb v) {
		text.text = v.Label;
	}

	private void DisplayIcon(Verb v) {
		Sprite sprite = ResourcesManager.GetSpriteForVerb(v.Id);
		artwork.sprite = sprite;
	}

	public void OnPointerClick(PointerEventData eventData) {
		// pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
		if ( eventData.pointerId >= -1 && onVerbBoxClicked != null)
			onVerbBoxClicked( this );
	}
}
