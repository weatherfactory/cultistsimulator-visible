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

[System.Serializable]
public class SpriteMapping
{
	public LanguageManager.eLanguage	language;
	public Sprite						sprite;
}

public class BabelfishImage : MonoBehaviour
{
//#pragma warning disable 649
    [Tooltip("Custom images per language")]
    [SerializeField] private SpriteMapping[]			sprites = new SpriteMapping[ (int)LanguageManager.eLanguage.maxLanguages ];
    [SerializeField] private bool			            usesOverride;
//#pragma warning restore 649

	private Image image;

    private void Awake()
    {
		image = gameObject.GetComponent<Image>() as Image;
		for (int i=0; i<(int)LanguageManager.eLanguage.maxLanguages; i++)
		{
			sprites[i].language = (LanguageManager.eLanguage)i;
		}
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
			bool shouldBeActive = (0 == string.Compare( LanguageTable.targetCulture, 0, ((LanguageManager.eLanguage)sprites[i].language).ToString(), 0, 2 ));
			if (shouldBeActive)
			{
                if (usesOverride)
                {
				    image.overrideSprite = sprites[i].sprite;
                } else
                {
                    image.sprite = sprites[i].sprite;
                }
				return;
			}
		}
    }
}
