using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Services;

using TMPro;
using UnityEngine;

//
// Manager for global language settings
//




[System.Serializable]
public class FontStyle
{
	public LanguageManager.eFontStyle fontStyle;
	public TMP_FontAsset	fontCJK;	// centralized font asset list for Chinese/Japanese/Korean lang (common ideographs) - but we don't use it for Japanese, because some characters are subtly different.
    public TMP_FontAsset fontJP;// Specifically Japanese fonts
    public TMP_FontAsset	fontRu;		// for RU - cyrillic langs
    public TMP_FontAsset	fontEn;		// for all other latin/germanic langs
    public TMP_FontAsset fontX;
    public Material			fontMaterial;
};

public interface ILocStringProvider
{
    Color HighContrastLight { get; set; }
    Color HighContrastDark { get; set; }

    TMP_FontAsset GetFont( LanguageManager.eFontStyle fs, string fontscript);
    Material GetFontMaterial( LanguageManager.eFontStyle fs );
    string GetTimeStringForCurrentLanguage(float time);
    string Get(string locLabel);
}



public class LanguageManager : MonoBehaviour,ILocStringProvider
{
    
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
        var concursum = Watchman.Get<Concursum>();
        concursum.ChangingCulture.AddListener(OnCultureChanged);

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

            case "jp":
                if (fontStyles[style].fontJP != null)
                    return fontStyles[style].fontJP;
                break;

            case "cyrillic":
                if (fontStyles[style].fontRu != null)
                    return fontStyles[style].fontRu;
                break;
                
            case "latin":
                if (fontStyles[style].fontEn != null)
                    return fontStyles[style].fontEn;
                break;

            case "latinplus": //functionally identical to the default! But maybe some day it won't be.
                if (fontStyles[style].fontX != null)
                    return fontStyles[style].fontX;
                break;

            default: // fall thru for all other languages; add additional cases if nec.
                if (fontStyles[style].fontX != null)
                    return fontStyles[style].fontX;
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


    public string Get(string locLabel)
    {

        string currentCultureId = Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY);
        var currentCulture = Watchman.Get<Compendium>().GetEntityById<Culture>(currentCultureId);


        if (locLabel.StartsWith(NoonConstants.TEMPLATE_MARKER))
            return GetTemplatedResult(locLabel);
        else
        {
            if (currentCulture.UILabels.TryGetValue(locLabel, out string localisedValue))
            {
                if (localisedValue.StartsWith(NoonConstants.TEMPLATE_MARKER))
                    return GetTemplatedResult(localisedValue);
                else
                    return localisedValue;
            }

            NoonUtility.LogWarning($"Missing UI label {locLabel} for {currentCulture.Id}. ({currentCulture.UILabels.Count} UI labels were found for this culture.)");
            return "MISSING_" + locLabel.ToUpper();
        }
    }

    private string GetTemplatedResult(string template)
    {
        
        const string SETTINGMARKER="{SETTING:";

        while (template.Contains(SETTINGMARKER))
        {
            int settingIdStartsAt = template.LastIndexOf(SETTINGMARKER, StringComparison.Ordinal) +SETTINGMARKER.Length;
            int settingIdEndsAt = template.IndexOf("}", settingIdStartsAt, StringComparison.Ordinal);
            int substringLength = settingIdEndsAt - settingIdStartsAt;

            string settingId= template.Substring(settingIdStartsAt, substringLength);

            Setting setting = Watchman.Get<Compendium>().GetEntityById<Setting>(settingId);
            if(setting==null)
                break;

            string toReplace= $"{SETTINGMARKER}{settingId}}}";

            string replacement = $"{setting.GetCurrentValueAsHumanReadableString()}";

            template = template.Replace(toReplace, replacement);
            
        }


        //if the string contains a token that matches a loc label, replace that token with the value of the loc label.
        //nb it is possible to pass a loc label that references another loc label, if the first loc label has a $
        Regex ParameterPattern = new Regex(@"\{(\w+)\}");
        string templatedResult = ParameterPattern.Replace(template, match => Get(match.Groups[1].Value));

        var resultWithoutMarker = templatedResult.Substring(1); //remove the dollar sign before returning it
        return resultWithoutMarker;
    }
}
