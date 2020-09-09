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

public interface ILanguageManager
{
    Color HighContrastLight { get; set; }
    Color HighContrastDark { get; set; }

    TMP_FontAsset GetFont( LanguageManager.eFontStyle fs, string fontscript);
    Material GetFontMaterial( LanguageManager.eFontStyle fs );
    string GetTimeStringForCurrentLanguage(float time);
    string Get(string id);
}

public class NullLanguageManager : ILanguageManager
{
    public Color HighContrastLight { get; set; }

    public Color HighContrastDark { get; set; }

    public TMP_FontAsset GetFont(LanguageManager.eFontStyle fs, string fontscript)
    {
        return null;
    }

    public Material GetFontMaterial(LanguageManager.eFontStyle fs)
    {
        return null;
    }

    public string GetTimeStringForCurrentLanguage(float time)
    {
        return "[neither form nor void]";
    }

    public string Get(string id)
    {
        return "[neither form nor void]";
    }
}

public class LanguageManager : MonoBehaviour,ILanguageManager
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
	public Color		HighContrastLight { get; set; }=Color.white;


    public Color		HighContrastDark { get; set; }=Color.black;

    

	public void Initialise()
	{
		
		FixFontStyleSlots();
        // force invariant culture to fix Linux save file issues
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        var concursum = Registry.Get<Concursum>();
        concursum.CultureChangedEvent.AddListener(OnCultureChanged);

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


    public void OnCultureChanged(CultureChangedArgs args)
    {
        
	//actually, we don't need to do anything here currently

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

        var fixedspace = Get("UI_FIXEDSPACE");                // Contains rich text fixed spacing size (and <b> for some langs)
        var secondsPostfix = Get("UI_SECONDS_POSTFIX_SHORT"); // Contains localised abbreviation for seconds, maybe a space and maybe a </b>
        var timeSeparator = Get("UI_TIME_SEPARATOR");         // '.' for most langs but some prefer ','


        var formattedTime = time.ToString("0.0");

        var formatttedTimeWithLocalisedSeparator = formattedTime.Replace(".", timeSeparator);

		return fixedspace + formatttedTimeWithLocalisedSeparator + secondsPostfix;
    }


    public string Get(string id)
    {
        var compendium = Registry.Get<ICompendium>();
        var conc = Registry.Get<Concursum>();

        var currentCulture = compendium.GetEntityById<Culture>(Registry.Get<Config>().CultureId);


        if (currentCulture.UILabels.TryGetValue(id.ToLower(), out string localisedValue))
            return localisedValue;


        return "MISSING_" + id.ToUpper();
    }
}
