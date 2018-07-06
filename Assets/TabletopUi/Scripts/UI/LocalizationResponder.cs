using TMPro;
using UnityEngine;

/// Localization responder - assumes a singleton centralized manager class (LanguageManager{}) that maintains 
/// fields for the font assets for different language sets being parsed (in this case: CJK, RU,
/// and EN -- all other). These centralized font assets may be overridden in the public fields here.
///
/// It is expected the LanguageManager defines an event used to indicate a language change has occurred.


public class LocalizationResponder : MonoBehaviour
{

    [SerializeField] private string						locLabel;   	// localization label for display text
	[SerializeField] private LanguageManager.eFontStyle fontStyle;		// determines only which font to use (no size/colour inferred)

    private TMP_Text tmpText;       // text mesh pro text object.

    private void Awake()
    {
        // cache the TMP component on this object
        tmpText = GetComponent<TMP_Text>();
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

    public void OnLanguageChanged()
    {
		TMP_FontAsset font = LanguageManager.Instance.GetFont( fontStyle );
		if (font != null)
		{
			tmpText.font = font;
		}

        // If using a specific font material, map the material to the
        // appropriate font texture atlas, then set the font asset's material.
		Material fontMaterial = LanguageManager.Instance.GetFontMaterial( fontStyle );
        if (fontMaterial != null)
        {
            fontMaterial.SetTexture("_MainTex", tmpText.font.material.mainTexture);
            tmpText.fontMaterial = fontMaterial;
        }

        // Localization label: only applies if set.
        if (locLabel != "")
		{
            tmpText.text = LanguageTable.Get(locLabel);
		}
    }
}
