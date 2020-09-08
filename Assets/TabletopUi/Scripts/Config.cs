using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Config
{
    public bool skiplogo = false;
    public int verbosity = 0;
    public bool knock = false;


    public string CultureId { get; set; }

    private Dictionary<string, string> _configValues;

    public Config(string defauCultureId)
    {
        CultureId = defauCultureId;
        _configValues = new Dictionary<string, string>();
        ReadFromIniFile();
    }


    /// <summary>
    /// returns an empty string if the value doeesn't exist, or is '0', or 'false' or an empty string
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetConfigValue(string key)
    {
        _configValues.TryGetValue(key, out string value);

        if (string.IsNullOrEmpty(value) || value == "0" || value == "false")
            value = string.Empty;
        return value;
    }

    public string SetConfigValue(string key,string value)
    {
        if (string.IsNullOrEmpty(value) || value == "0" || value == "false")
            value = "0";

        _configValues[key] = value;

        return value;
    }


    private string GetConfigFileLocation()
    {
        return Application.persistentDataPath + "/config.ini";
    }

    public void ReadFromIniFile()
    {
        


        if (File.Exists(GetConfigFileLocation()))
        {

            _configValues = PopulateConfigValues(GetConfigFileLocation());


            CultureId = GetStartingCultureId(GetConfigValue("culture"));

            if (GetConfigValue("skiplogo") != String.Empty)
            {
                skiplogo = true;
            }

            if (GetConfigValue("verbosity") == String.Empty)
                verbosity = (int) VerbosityLevel.Significants;
            else
            {
                int.TryParse(GetConfigValue("verbosity"), out verbosity);
                NoonUtility.CurrentVerbosity = verbosity;
            }

            if (GetConfigValue("knock") != String.Empty)
            {
                knock = true;
            }
        }
        else
        {
            SetConfigValue("skiplogo", "0");
            WriteIniFile();
        }

    }

    private void WriteIniFile()
    {
        var output = _configValues.Select(kvp => kvp.Key + "=" + kvp.Value);
        File.WriteAllLines(GetConfigFileLocation(),output);
    }

    public void PersistSettingValue(ChangeSettingArgs args)
    {
        SetConfigValue(args.Key, args.Value.ToString());
        WriteIniFile();

    }

    protected object GetPersistedSettingValue(string forId)
    {
        return GetConfigValue(forId);
    }

    public float? GetPersistedSettingValueAsFloat (string forId)
    {
    if(Single.TryParse(GetConfigValue(forId), out Single singleValue))
        return singleValue;
    return null;
    }

    public int? GetPersistedSettingValueAsInt(string forId)
    {
        if (Int32.TryParse(GetConfigValue(forId), out int intValue))
            return intValue;
        return null;
    }

    public string GetPersistedSettingValueAsString(string forId)
    {
        return GetConfigValue(forId);
    }

    //public void PersistSettingValue(ChangeSettingArgs args)
    //{
    //    if(args.Value is float floatValue)
    //        PlayerPrefs.SetFloat(args.Key, floatValue);
    //    else if (args.Value is int intValue)
    //        PlayerPrefs.SetInt(args.Key, intValue);
    //    else
    //        PlayerPrefs.SetString(args.Key,args.Value as string);
    //}

    //public float? GetPersistedSettingValue(string forId)
    //{
    //    if (PlayerPrefs.HasKey(forId))
    //        return PlayerPrefs.GetFloat(forId);
    //    else
    //        return null;
    //}

    private Dictionary<string,string> PopulateConfigValues(string configLocation)
    {
        Dictionary<string, string> dictToPopulate=new Dictionary<string, string>();
        var lines = File.ReadLines(configLocation);

        foreach (var line in lines)
        {
 
            try
            {
                var splitLine = line.Split('=');
                dictToPopulate.Add(splitLine[0], splitLine[1]);
            }
            catch (Exception e)
            {
                NoonUtility.Log($"Couldn't read config line {line}: {e.Message}", 2);
            }
        }


        return dictToPopulate;
    }


    private string GetStartingCultureId(string cultureValue)
    {
        //a culture set in config.ini overrides everything
        if (cultureValue.Contains("lang=en"))
        {
            return "en";
        }
        else if (cultureValue.Contains("lang=ru"))
        {
            return "ru";
        }
        else if (cultureValue.Contains("lang=zh"))
        {
            return "zh-hans";
        }



        // If the player has already chosen a culture in Options, use that one 
        if (PlayerPrefs.HasKey(NoonConstants.CULTURE_SETTING_KEY))
        {
            return PlayerPrefs.GetString(NoonConstants.CULTURE_SETTING_KEY);
        }



        // Try to auto-detect the culture from the system language 
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Russian:
                return "ru";
                
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
                return "zh-hans";
                
            default:
                switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                {
                    case "zh":
                        return "zh-hans";
                        
                    case "ru":
                        return "ru";
                        
                    default:
                        return (NoonConstants.DEFAULT_CULTURE_ID);
                        
                }

                
        }


    }

}
