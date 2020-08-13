using System.Globalization;
using System.IO;
using System.Threading;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
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

    
	private bool timeStringsUpdated = false;
    private string fixedspace = "<mspace=1.6em>";    // defaults are overriden by strings.csv
    private string secondsPostfix = "s";
    private string timeSeparator = ".";

    public Culture CurrentCulture;


	public void Initialise(ICompendium withCompendium)
	{
	
		DontDestroyOnLoad(this.gameObject);
		
		string startingCultureId;

		// Try to auto-detect the culture from the system language first
	    switch (Application.systemLanguage)
	    {
		    case SystemLanguage.Russian:
			    startingCultureId = "ru";
			    break;
		    case SystemLanguage.Chinese:
			case SystemLanguage.ChineseSimplified:
			case SystemLanguage.ChineseTraditional:
			    startingCultureId = "zh-hans";
			    break;
		    default:
			    switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
			    {
				    case "zh":
					    startingCultureId = "zh-hans";
					    break;
				    case "ru":
					    startingCultureId = "ru";
					    break;
					default:
						startingCultureId = "en";
						break;
			    }
			    break;
	    }

	    // If the player has already chosen a culture, use that one instead
		if (PlayerPrefs.HasKey(NoonConstants.CULTURE_SETTING_KEY))
		{
			startingCultureId = PlayerPrefs.GetString(NoonConstants.CULTURE_SETTING_KEY);
		}

		// If an override is specified, ignore everything else and use that
		if (Config.Instance.culture != null)
		{
			startingCultureId = Config.Instance.culture;
		}


        var startingCulture = withCompendium.GetEntityById<Culture>(startingCultureId);

		if(startingCulture==null)
			NoonUtility.Log($"Unrecognised culture: {startingCultureId}",2);

		FixFontStyleSlots();

		// force invariant culture to fix Linux save file issues
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var concursum = Registry.Retrieve<Concursum>();
		concursum.CultureChangedEvent.AddListener(CultureChangeHasOccurred);
		concursum.CultureChangedEvent.Invoke(new CultureChangedArgs{NewCulture = startingCulture});
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

    public void CultureChangeHasOccurred(CultureChangedArgs args)
    {
        PlayerPrefs.SetString(NoonConstants.CULTURE_SETTING_KEY, args.NewCulture.Id);
		
		CurrentCulture = args.NewCulture;
		
		fixedspace = Get("UI_FIXEDSPACE");                // Contains rich text fixed spacing size (and <b> for some langs)
            secondsPostfix = Get("UI_SECONDS_POSTFIX_SHORT"); // Contains localised abbreviation for seconds, maybe a space and maybe a </b>
            timeSeparator = Get("UI_TIME_SEPERATOR");         // '.' for most langs but some prefer ','
            timeStringsUpdated = true;

	}


	public TMP_FontAsset GetFont( eFontStyle fs, string culture )
    {
        var fontscript = Registry.Retrieve<ICompendium>().GetEntityById<Culture>(culture)?.FontScript;
		

		int style = (int)fs;
		// determine which language is being used:
        switch (fontscript)
        {
            case "cjk":
                if (fontStyles[style].fontCJK != null)
                    return fontStyles[style].fontCJK;
                break;

            case "cyrillic":
                if (fontStyles[style].fontRu != null)
                    return fontStyles[style].fontRu;
                break;

			case "latin":
                if (fontStyles[style].fontEn != null)
                    return fontStyles[style].fontEn;
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

    public string GetTimeStringForCurrentLanguage(float time)
    {
        var formattedTime = time.ToString("0.0").Replace('.', timeSeparator[0]);
        return fixedspace + formattedTime + secondsPostfix;
    }


    public string Get(string id)
    {

        

        if (CurrentCulture.UILabels.TryGetValue(id.ToLower(), out string localisedValue))
            return localisedValue;

        if (CurrentCulture.Id != NoonConstants.DEFAULT_CULTURE)
        {
            var defaultCulture = Registry.Retrieve<ICompendium>().GetEntityById<Culture>(NoonConstants.DEFAULT_CULTURE);
            if (defaultCulture.UILabels.TryGetValue(id, out string defaultCultureValue))
                return defaultCultureValue;

        }


        return "MISSING_" + id.ToUpper();
    }
}
