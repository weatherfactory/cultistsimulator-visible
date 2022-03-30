using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Services;

using OrbCreationExtensions;
using SecretHistories.Constants;
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
    #pragma warning disable 649
    [Tooltip("If true, uses overrideSprite rather than Sprite")]
    [SerializeField] private bool			            usesOverride;
#pragma warning restore 649

	private Image image;

    private void Start()
    {
		image = gameObject.GetComponent<Image>() as Image;

        string currentCultureId = Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY);

        var currentCulture = Watchman.Get<Compendium>().GetEntityById<Culture>(currentCultureId);

        DisplayImageForCulture(currentCulture);

    }

    private void DisplayImageForCulture(Culture culture)
    {
        if (usesOverride)
        {
            image.overrideSprite =
                ResourcesManager.GetSpriteLocalised("ui", image.sprite.name, culture.Id);
        }
        else
        {
            image.sprite =
                ResourcesManager.GetSpriteLocalised("ui", image.sprite.name, culture.Id);
        }
        return;
    }

    private void OnEnable()
    {
        var c = Watchman.Get<Concursum>();
       c.AfterChangingCulture.AddListener(OnCultureChanged);
        
    }

    private void OnDisable()
    {
        Watchman.Get<Concursum>().AfterChangingCulture.RemoveListener(OnCultureChanged);

    }

    public virtual void OnCultureChanged(CultureChangedArgs args)
    {
        DisplayImageForCulture(args.NewCulture);

        
    }

}
