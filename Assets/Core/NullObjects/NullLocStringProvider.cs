using TMPro;
using UnityEngine;

public class NullLocStringProvider : ILocStringProvider
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

    public string Get(string locLabel)
    {
        return "[neither form nor void]";
    }
}