using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Localization responder - assumes a singleton centralized manager class (LanguageManager{})
/// that maintains fields for the font assets for different language sets being parsed
/// (in this case: CJK, RU, and EN for everything else).
///
/// It is expected the LanguageManager defines an event used to indicate a language change has occurred.
///
/// This script switches a UI image according to the current language


public class BabelfishImage : MonoBehaviour
{
    [Tooltip("Custom images per language")]
    #pragma warning disable 649
    [SerializeField] private bool			            usesOverride;
#pragma warning restore 649

	private Image image;

    private void Awake()
    {
		image = gameObject.GetComponent<Image>() as Image;
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

	public virtual void OnLanguageChanged()
    {
		if (LanguageManager.Instance==null)
			return;
		
		for (int i=0; i<(int)LanguageManager.eLanguage.maxLanguages; i++)
		{
			// Compare only first two letters of locale code because we can't use "zh-hans" as an enum

                if (usesOverride)
                {
                    image.overrideSprite =
                        ResourcesManager.GetSpriteLocalised("ui", image.sprite.name, LanguageTable.targetCulture);
                    // image.overrideSprite = sprites[i].sprite;
                } else
                {
                    image.sprite =
                        ResourcesManager.GetSpriteLocalised("ui", image.sprite.name, LanguageTable.targetCulture);

					//image.sprite = sprites[i].sprite;
				}
				return;
        }
    }

}
