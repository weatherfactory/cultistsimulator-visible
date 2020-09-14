using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
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



    private Dictionary<string, string> _configValues;

    public Config()
    {
        
        _configValues = new Dictionary<string, string>();
        ReadFromIniFile();

      if(string.IsNullOrEmpty(GetConfigValue(NoonConstants.CULTURE_SETTING_KEY)))
        SetConfigValue(NoonConstants.CULTURE_SETTING_KEY,DetermineMostSuitableCultureId());

    }


    /// <summary>
    /// returns an empty string if the value doeesn't exist, or is '0', or 'false' or an empty string
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetConfigValue(string key)
    {
        _configValues.TryGetValue(key, out string value);

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
            PersistConfigValuesToIniFile();
        }

    }

    public void MigrateAnySettingValuesInRegistry(ICompendium compendium)
    {
        //We've moved storage of Options settings from PlayerPrefs to the config file.
        //To avoid users losing all their persisted settings, and to set a sensible default, for every options setting:
        

        foreach (Setting setting in compendium.GetEntitiesAsList<Setting>())
        {
            try
            {

                object valueToSet=null;
                if (setting.Id == NoonConstants.RESOLUTION)
                {
                    //stored in registry as a float, but assumed now to be an int
                    if (PlayerPrefs.HasKey(setting.Id))
                        valueToSet = (int)PlayerPrefs.GetFloat(setting.Id);

                }
                else  if (setting.DataType == nameof(Single))
                {
                    if (PlayerPrefs.HasKey(setting.Id))
                        valueToSet = PlayerPrefs.GetFloat(setting.Id);
                }
                else if (setting.DataType == nameof(Int32))
                {
                    if (PlayerPrefs.HasKey(setting.Id))
                        valueToSet = PlayerPrefs.GetInt(setting.Id);
  
                }
                else if (setting.DataType == nameof(String))
                {
                    if (PlayerPrefs.HasKey(setting.Id))
                        valueToSet = PlayerPrefs.GetString(setting.Id);
                
                }

  
                if(valueToSet!=null) //change and persist the setting to match the migrated value
                {
                    Registry.Get<Concursum>().ChangeSetting(new ChangeSettingArgs { Key = setting.Id, Value = valueToSet });
                    NoonUtility.Log($"Imported {setting.Id} from registry with value {valueToSet}");
                    PlayerPrefs.DeleteKey(setting.Id);
                    NoonUtility.Log($"Deleted {setting.Id} key from registry");
                }

            }
            catch (Exception e)
            {
                NoonUtility.Log($"Problem Importing {setting.Id} from registry: " + e.Message );
            }
        }


    }

    private void PersistConfigValuesToIniFile()
    {
        var output = _configValues.Select(kvp => kvp.Key + "=" + kvp.Value);
        File.WriteAllLines(GetConfigFileLocation(),output);
    }


    private void SetSanitisedConfigValue(string key, string value)
    {
        if (string.IsNullOrEmpty(value) || value == "0" || value == "false")
            value = "0";

        _configValues[key] = value;
    }

    public void SetConfigValue(string key, string value)
    {
        SetSanitisedConfigValue(key, value);
        PersistConfigValuesToIniFile();

    }


    public void PersistSettingValue(ChangeSettingArgs args)
    {
        SetSanitisedConfigValue(args.Key, args.Value.ToString());
        PersistConfigValuesToIniFile();

    }


    public float? GetConfigValueAsFloat (string forId)
    {
    if(Single.TryParse(GetConfigValue(forId), out Single singleValue))
        return singleValue;
    return null;
    }

    public int? GetConfigValueAsInt(string forId)
    {
        if (Int32.TryParse(GetConfigValue(forId), out int intValue))
            return intValue;
        return null;
    }

    public string GetConfigValueAsString(string forId)
    {
        return GetConfigValue(forId);
    }


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


    private string DetermineMostSuitableCultureId()
    {
        


        //check for a culture specified in the legacy format
        string legacyCultureValue = GetConfigValue("lang");
        if (!string.IsNullOrEmpty(legacyCultureValue) && legacyCultureValue == "zh")
            return "zh-hans";
        if (!string.IsNullOrEmpty(legacyCultureValue))
            return legacyCultureValue;

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
