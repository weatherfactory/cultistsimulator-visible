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

    
    private string fixedspace = "<mspace=1.6em>";    // defaults are overriden by strings.csv
    private string secondsPostfix = "s";
    private string timeSeparator = ".";

    private Culture CurrentCulture;

    public string GetCurrentCultureId()
    {
		//there's a chicken and egg situation with LanguageManager and Compendium: hence this
        if (CurrentCulture == null)
            return NoonConstants.DEFAULT_CULTURE_ID;

        return CurrentCulture.Id;
    }
	public void Initialise(ICompendium withCompendium,string startingCultureId)
	{
		
		FixFontStyleSlots();
        // force invariant culture to fix Linux save file issues
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        var concursum = Registry.Retrieve<Concursum>();
        concursum.CultureChangedEvent.AddListener(CultureChangeHasOccurred);



        var initialiseWithCulture = withCompendium.GetEntityById<Culture>(startingCultureId);

        if (initialiseWithCulture == null)
            NoonUtility.Log($"Unrecognised culture: {startingCultureId}", 2);
        else
            SetCulture(initialiseWithCulture);



        
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

    public void SetCulture(Culture culture)
    {
        var concursum = Registry.Retrieve<Concursum>();
        CurrentCulture = culture;
        concursum.CultureChangedEvent.Invoke(new CultureChangedArgs { NewCulture = culture });
	}

    public void CultureChangeHasOccurred(CultureChangedArgs args)
    {
        PlayerPrefs.SetString(NoonConstants.CULTURE_SETTING_KEY, args.NewCulture.Id);

        if (CurrentCulture != args.NewCulture)
            CurrentCulture = args.NewCulture;

		fixedspace = Get("UI_FIXEDSPACE");                // Contains rich text fixed spacing size (and <b> for some langs)
            secondsPostfix = Get("UI_SECONDS_POSTFIX_SHORT"); // Contains localised abbreviation for seconds, maybe a space and maybe a </b>
            timeSeparator = Get("UI_TIME_SEPERATOR");         // '.' for most langs but some prefer ','

    }


	public TMP_FontAsset GetFont( eFontStyle fs, string fontscript)
    {
        
		

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

        if (CurrentCulture.Id != NoonConstants.DEFAULT_CULTURE_ID)
        {
            var defaultCulture = Registry.Retrieve<ICompendium>().GetEntityById<Culture>(NoonConstants.DEFAULT_CULTURE_ID);
            if (defaultCulture.UILabels.TryGetValue(id, out string defaultCultureValue))
                return defaultCultureValue;

        }


        return "MISSING_" + id.ToUpper();
    }
}
