using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutoCompletionSuggestion : MonoBehaviour
{
	[SerializeField] private Text _text;
	[SerializeField] private Image _icon;
	[SerializeField] private Button _button;

	public string GetText()
	{
		return _text.text;
	}

	public void SetText(string suggestedId)
	{
		_text.text = suggestedId;
	}

	public void SetIconForElement(Element element)
	{
		if (element == null)
			return;

		_icon.sprite = element.IsAspect ? ResourcesManager.GetSpriteForAspect(element.Icon)
			: ResourcesManager.GetSpriteForElement(element.Icon);
	}

	public void AddClickListener(UnityAction call)
	{
		_button.onClick.AddListener(call);
	}
}
