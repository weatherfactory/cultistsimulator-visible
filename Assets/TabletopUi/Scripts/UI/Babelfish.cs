using TMPro;
using UnityEngine;
using Assets.CS.TabletopUI;	// For accessing high contrast mode

/// Localization responder - assumes a singleton centralized manager class (LanguageManager{})
/// that maintains fields for the font assets for different language sets being parsed
/// (in this case: CJK, RU, and EN for everything else).
///
/// It is expected the LanguageManager defines an event used to indicate a language change has occurred.
///
/// You can set the fontStyle to BodyText, Heading or Button (or add new styles) to tell it which font to use.
/// That way you only have one place to update the font (LanguageManager.cs) and it is reflected throughout the game.

public class Babelfish : MonoBehaviour
{
	[Tooltip("Strings.csv label\n(if null then TextMeshPro string is left alone)")]
    [SerializeField] private string						locLabel;
#pragma warning disable 649
    [Tooltip("Which font set should this text use?\nFont sets assigned in LanguageManager")]
	[SerializeField] private LanguageManager.eFontStyle fontStyle;

    [Tooltip("Force this string to use the font for a specific language (empty by default)")]
    [SerializeField] private string						forceFontLanguage;
#pragma warning restore 649
	[SerializeField] private bool						highContrastEnabled = true;
	[SerializeField] private bool						highContrastBold = true;


	private Color		defaultColor;
	private FontStyles	defaultStyle;
    private TMP_Text	tmpText;       // text mesh pro text object.
	//private bool initComplete = false; //doesn't seem to be used, throwing warning; commenting out in case it's used in some unexpected Unity way

    private void Awake()
    {
        // cache the TMP component on this object
        tmpText = GetComponent<TMP_Text>();
		defaultColor = tmpText.color;
		defaultStyle = tmpText.fontStyle;
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

	public void SetLocLabel( string label )	// Allows code to modify string label such that it can swap languages later
	{
		locLabel = label;
	}

	public virtual void OnLanguageChanged()
    {
		if (LanguageManager.Instance==null)
			return;
		
		TMP_FontAsset font = LanguageManager.Instance.GetFont( fontStyle, (forceFontLanguage==null || forceFontLanguage.Length==0) ? LanguageTable.targetCulture : forceFontLanguage );
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

		if (highContrastEnabled)
		{
			//highContrastBold = true;	// Force all text to go bold

			if (TabletopManager.GetHighContrast())
			{
				Color light = LanguageManager.Instance.highContrastLight;
				Color dark = LanguageManager.Instance.highContrastDark;
				light.a = 1.0f;	// ensure color is opaque
				dark.a = 1.0f;
				tmpText.color = defaultColor.grayscale > 0.5f ? light : dark;
				if (highContrastBold)
					tmpText.fontStyle |= FontStyles.Bold;
			}
			else
			{
				tmpText.color = defaultColor;
				if (highContrastBold)
					tmpText.fontStyle = defaultStyle;
			}
		}
    }
}
