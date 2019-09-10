using System.Globalization;
using System.IO;
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
	private const string CULTURE="Culture";

	public enum eLanguage
	{
		en,
		ru,
		zh,

		maxLanguages
	};

	public enum eFontStyle
	{
		BodyText,
		Heading,
		Button,
		Numbers,

		FontStyleCount		// always last
	};


    public FontStyle[]	fontStyles;
	public Color		highContrastLight = Color.white;
	public Color		highContrastDark = Color.black;

    // simple singleton declaration
    private static LanguageManager _instance;
    public static LanguageManager Instance
    {
        get
        {
            if (_instance == null)
			{
				// First init
			}
            return _instance;
        }
    }

	private void Awake()
	{
		if (_instance)
		{
			Debug.LogError("More than one instance of LanguageManager created!");
		}
		_instance = this;
		DontDestroyOnLoad(this.gameObject);

		string defaultCulture = null;
		string defaultConfigLocation = "Assets/StreamingAssets/content/defaults.ini";

	    if (File.Exists(defaultConfigLocation))
	    {
			string contents = File.ReadAllText(defaultConfigLocation);

			// Quick and dirty mapping for the three langs we support...
			if (contents.Contains("lang=en"))
	        {
	            defaultCulture = "en";
	        }
			else if (contents.Contains("lang=ru"))
	        {
	            defaultCulture = "ru";
	        }
			else if (contents.Contains("lang=zh"))
	        {
	            defaultCulture = "zh-hans";
	        }
		}

	    if (defaultCulture == null)
	    {
		    switch (Application.systemLanguage)
		    {
			    case SystemLanguage.Russian:
				    defaultCulture = "ru";
				    break;
			    case SystemLanguage.Chinese:
				case SystemLanguage.ChineseSimplified:
				case SystemLanguage.ChineseTraditional:
				    defaultCulture = "zh-hans";
				    break;
			    default:
				    switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
				    {
					    case "zh":
						    defaultCulture = "zh-hans";
						    break;
					    case "ru":
						    defaultCulture = "ru";
						    break;
						default:
							defaultCulture = "en";
							break;
				    }
				    break;
		    }
	    }

		if (PlayerPrefs.HasKey(CULTURE))
		{
			defaultCulture = PlayerPrefs.GetString(CULTURE);
		}
		LanguageTable.LoadCulture( defaultCulture );	// Initial load

		FixFontStyleSlots();

		// force invariant culture to fix Linux save file issues
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        // inform systems and components that the language has been changed (to catch UI that was initialised before this Awake call).
        LanguageChangeHasOccurred();
	}



	private void FixFontStyleSlots() // Naff way to initialise font styles so they'll readable in the Unity editor
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
    public static void LanguageChangeHasOccurred()
    {
        if (LanguageChanged != null) LanguageChanged();
    }

    // Pass standard language codes.
    public void SetLanguage(string lang)
    {
		PlayerPrefs.SetString( CULTURE, lang );

        LanguageTable.LoadCulture( lang );

        // inform systems and components that the language has been changed.
        LanguageChangeHasOccurred();
    }

	public TMP_FontAsset GetFont( eFontStyle fs, string culture )
	{
		int style = (int)fs;
		// determine which language is being used:
        switch (culture)
        {
            case "cjk":
            case "ko":
            case "zh":
			case "zh-hans":
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

#if UNITY_EDITOR
	bool showDebugLanguageSelect = false;

	void OnGUI()
	{
		if (GUI.Button( new Rect(Screen.width - 400, 10, 90, 20), "LANGUAGE"))
		{
			showDebugLanguageSelect = !showDebugLanguageSelect;
		}
		if (showDebugLanguageSelect)
		{
			for (int i=0; i<LanguageTable.GetSupportedCultures(); i++)
			{
				if (GUI.Button( new Rect(Screen.width - 400, 35 + 25*i, 190, 20), LanguageTable.GetCultureName(i)))
				{
					SetLanguage( LanguageTable.GetCultureCode(i) );
				}
			}
		}
	}
#endif
}
