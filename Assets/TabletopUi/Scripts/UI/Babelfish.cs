using Assets.Core.Entities;
using TMPro;
using UnityEngine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TabletopUi.Scripts.UI;
using UnityEngine.UI; // For accessing high contrast mode


public class Babelfish : MonoBehaviour,ISettingSubscriber
{
	[SerializeField] private string						locLabel;
#pragma warning disable 649
    [Tooltip("Which font set should this text use?\nFont sets assigned in LanguageManager")]
	[SerializeField] private LanguageManager.eFontStyle fontStyle;
    [Tooltip("Force this string to use the font for a specific language (empty by default)")]
    [SerializeField] private string						forceFontLanguage;
#pragma warning restore 649
	[SerializeField] private bool						highContrastPermissibleInContext = true;
	[SerializeField] private bool						highContrastBold = true;
    [SerializeField] private bool                       forceBold = false;


	private Color		defaultColor;
	private FontStyles	defaultStyle;
    private TMP_Text _tmpText;

    private bool _initialised = false;

    protected TMP_Text tmpText
    {
        get 
    {
        if(_tmpText==null)
            _tmpText = GetComponent<TMP_Text>();
        return _tmpText;
    }

    }
    private bool HighContrastEnabledInGlobalSettings=false;

    private void OnEnable()
    {
        if (!_initialised)
            Initialise();
    }

    private void Initialise()
    {
        defaultColor = tmpText.color;
        defaultStyle = tmpText.fontStyle;

        var concursum = Registry.Get<Concursum>();

        concursum.ChangingCulture.AddListener(OnCultureChanged);
        concursum.ContentUpdatedEvent.AddListener(OnContentUpdated);

        var highContrastSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.HIGHCONTRAST);
        if (highContrastSetting != null)
        {
            highContrastSetting.AddSubscriber(this);
            WhenSettingUpdated(highContrastSetting.CurrentValue);
        }
        else
            NoonUtility.Log("Missing setting entity: " + NoonConstants.HIGHCONTRAST);


        _initialised = true;
    }

    

    public void SetValuesForCurrentCulture()
    {
        string currentCultureId = Registry.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY);

        var currentCulture = Registry.Get<ICompendium>().GetEntityById<Culture>(currentCultureId);

        SetValuesFromCulture(currentCulture);
    }

    public void SetValuesFromCulture(Culture culture)
    {

        ILocStringProvider lm = Registry.Get<ILocStringProvider>();

        if (lm == null)
            lm = new NullLocStringProvider();
        

        string fontscript;


        if (!string.IsNullOrEmpty(forceFontLanguage))
            fontscript = forceFontLanguage;
        else
            fontscript = culture.FontScript;

        TMP_FontAsset font = lm.GetFont(fontStyle, fontscript);
        if (font != null)
        {
            tmpText.font = font;
        }

        // If using a specific font material, map the material to the
        // appropriate font texture atlas, then set the font asset's material.
        Material fontMaterial = lm.GetFontMaterial(fontStyle);
        if (fontMaterial != null)
        {
            fontMaterial.SetTexture("_MainTex", tmpText.font.material.mainTexture);
            tmpText.fontMaterial = fontMaterial;
        }

        // Localization label: only applies if set.
        if (locLabel != "")
        {
            tmpText.text = lm.Get(locLabel);
        }

        SetFontStyle(culture, lm);
    }

    public void WhenSettingUpdated(object newValue)
    {
        HighContrastEnabledInGlobalSettings = ((newValue is float ? (float)newValue : 0) > 0.5f);
        string currentCultureId = Registry.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY); 
        var currentCulture = Registry.Get<ICompendium>().GetEntityById<Culture>(currentCultureId);
        SetValuesFromCulture(currentCulture);

        ILocStringProvider lm = Registry.Get<ILocStringProvider>();

        if (lm == null)
            lm = new NullLocStringProvider();

        SetFontStyle(currentCulture,lm);
    }

    private void SetFontStyle(Culture culture, ILocStringProvider lm)
    {
        if (highContrastPermissibleInContext)
        {
            //highContrastBold = true;	// Force all text to go bold

            var highContrastEnabled = Registry.Get<Config>().GetConfigValueAsInt(NoonConstants.HIGHCONTRAST);

            if (highContrastEnabled!=null && highContrastEnabled>0)
            {
                Color light = lm.HighContrastLight;
                Color dark = lm.HighContrastDark;
                light.a = 1.0f; // ensure color is opaque
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

        // Always disable bold for Chinese, since it can make the text
        // unreadable
        if (!culture.BoldAllowed)
        {
            tmpText.fontStyle &= ~FontStyles.Bold;
        }

        if (forceBold)
        {
            tmpText.fontStyle |= FontStyles.Bold;
        }
    }


    public void UpdateLocLabel( string label )	
	{
		locLabel = label;
        SetValuesForCurrentCulture();
	}


    public virtual void OnContentUpdated(ContentUpdatedArgs args)
    {
        SetValuesForCurrentCulture();
    }

	public virtual void OnCultureChanged(CultureChangedArgs args)
    {
     SetValuesFromCulture(args.NewCulture);
    }


}
