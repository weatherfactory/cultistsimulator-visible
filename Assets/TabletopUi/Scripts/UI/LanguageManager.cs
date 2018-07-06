using TMPro;
using UnityEngine;

//
// Manager for global language settings
//

[System.Serializable]
public class FontStyle
{
	public LanguageManager.eFontStyle fontStyle;
	public TMP_FontAsset	fontCJK;	// centralized font asset list for Chinese/Japanese/Korean lang (common ideographs)
    public TMP_FontAsset	fontRu;		// for RU - cyrillic langs
    public TMP_FontAsset	fontEn;		// for all other latin/germanic langs
	public Material			fontMaterial;
};

public class LanguageManager : MonoBehaviour
{
	public enum eFontStyle
	{
		BodyText,
		Heading,
		Button,

		FontStyleCount		// always last
	};


    public FontStyle[]	fontStyles;

    // simple singleton declaration
    private static LanguageManager _instance;
    public static LanguageManager Instance
    {
        get
        {
            if (_instance == null)
			{
                _instance = GameObject.FindObjectOfType<LanguageManager>();
				LanguageTable.LoadCulture( "en" );	// Initial load
			}
            return _instance;
        }
    }

	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
		FixFontStyleSlots();
	}

	private void FixFontStyleSlots() // Naff way to label them in the Unity editor
	{
		int count = Mathf.Min( (int)eFontStyle.FontStyleCount, fontStyles.Length );
		for (int i=0; i<count; i++)
		{
			if (fontStyles[i] != null)
				fontStyles[i].fontStyle = (eFontStyle)i;
		}
	}

	// language change event definition
	public delegate void LanguageMgrHandler();
    public static event LanguageMgrHandler LanguageChanged;

    // call this method to properly fire the lang changed event
    private static void LanguageChangeHasOccurred()
    {
        if (LanguageChanged != null) LanguageChanged();
    }

    // Pass standard language codes.
    public void SetLanguage(string lang)
    {
        LanguageTable.LoadCulture( lang );

        // inform systems and components that the language has been changed.
        LanguageChangeHasOccurred();
    }

	public TMP_FontAsset GetFont( eFontStyle fs )
	{
		int style = (int)fs;
		// determine which language is being used:
        switch (LanguageTable.targetCulture)
        {
            case "cjk":
            case "ko":
            case "zh":
            case "jp":
                if (fontStyles[style].fontCJK != null)
                    return fontStyles[style].fontCJK;
                break;

            case "ru":
                if (fontStyles[style].fontRu != null)
                    return fontStyles[style].fontRu;
                break;

            default: // fall thru for all other languages; add additional cases if nec.
                if (fontStyles[style].fontEn != null)
                    return fontStyles[style].fontEn;
                break;
        }
		return null;
	}

	public Material GetFontMaterial( eFontStyle fs )
	{
		int style = (int)fs;
      	if (fontStyles[style].fontMaterial != null)
			return fontStyles[style].fontMaterial;
		return null;		
	}
}
