using TMPro;
using UnityEngine;

/// Localization responder - assumes a singleton centralized manager class (LanguageManager{})
/// that maintains fields for the font assets for different language sets being parsed
/// (in this case: CJK, RU, and EN for everything else).
///
/// It is expected the LanguageManager defines an event used to indicate a language change has occurred.
///
/// This script hides/unhides it's children according to the current language
/// It doesn't hide itself because it needs to be active to receive the language change events

public class BabelfishFilter : MonoBehaviour
{
	[Tooltip("Language that this object's children should be visible in")]
    [SerializeField] private LanguageManager.eLanguage			language;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        // subscribe to event for language change
        LanguageManager.LanguageChanged += OnLanguageChanged;
        
        // Initialize the component on enable to make sure this object
        // has the most current language configuration.
        OnLanguageChanged();
    }

    private void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
    }

	private void SetActiveAllChildren(Transform transform, bool value)
    {
         foreach (Transform child in transform)
         {
             child.gameObject.SetActive(value);
 
             SetActiveAllChildren(child, value);
         }
    }

	public virtual void OnLanguageChanged()
    {
		if (LanguageManager.Instance==null)
			return;
		
		// Compare only first two letters of locale code because we can't use zh-hans as an enum
		bool shouldBeActive = (0 == string.Compare( LanguageTable.targetCulture, 0, language.ToString(), 0, 2 ));
		SetActiveAllChildren(transform, shouldBeActive);
    }
}
