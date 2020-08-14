using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
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
#pragma warning disable 649
    [Tooltip("Language that this object's children should be visible in")]
    [SerializeField] private LanguageManager.eLanguage			language;
#pragma warning restore 649
    private void Awake()
    {
    }

    private void OnEnable()
    {
        Registry.Get<Concursum>().CultureChangedEvent.AddListener(OnCultureChanged);

    }

    private void OnDisable()
    {
        Registry.Get<Concursum>().CultureChangedEvent.RemoveListener(OnCultureChanged);

    }

    private void SetActiveAllChildren(Transform transform, bool value)
    {
         foreach (Transform child in transform)
         {
             child.gameObject.SetActive(value);
 
             SetActiveAllChildren(child, value);
         }
    }

	public virtual void OnCultureChanged(CultureChangedArgs args)
    {

		// Compare only first two letters of locale code because we can't use zh-hans as an enum
		bool shouldBeActive = (0 == string.Compare(args.NewCulture.Id, 0, language.ToString(), 0, 2 ));
		SetActiveAllChildren(transform, shouldBeActive);
    }
}
