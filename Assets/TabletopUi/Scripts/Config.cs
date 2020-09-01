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

    private Dictionary<string, string> ConfigValues;


    /// <summary>
    /// returns an empty string if the value doeesn't exist, or is '0', or 'false' or an empty string
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetConfigValue(string key)
    {
        ConfigValues.TryGetValue(key, out string value);

        if (string.IsNullOrEmpty(value) || value == "0" || value == "false")
            value = string.Empty;
        return value;
    }


    public void ReadFromIniFile()
    {
        string configLocation = Application.persistentDataPath + "/config.ini";


        if (File.Exists(configLocation))
        {

            ConfigValues = PopulateConfigValues(configLocation);


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

            File.WriteAllText(configLocation, "skiplogo=0");
        }

    }

    public void PersistSettingValue(ChangeSettingArgs args)
    {
        PlayerPrefs.SetFloat(args.Key,args.Value);
    }

    public float? GetPersistedSettingValue(string forId)
    {
        if (PlayerPrefs.HasKey(forId))
            return PlayerPrefs.GetFloat(forId);
        else
            return null;
    }

    private Dictionary<string,string> PopulateConfigValues(string hackyConfigLocation)
    {
        Dictionary<string, string> dictToPopulate=new Dictionary<string, string>();
        var lines = File.ReadLines(hackyConfigLocation);

        foreach (var line in lines)
        {
            if (line.StartsWith("//"))
                continue;

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
