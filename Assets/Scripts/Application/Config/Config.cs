﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SettingObserverForConfig : ISettingSubscriber
{
    private readonly string _settingId;
    private readonly Config _config;

    public SettingObserverForConfig(string settingId, Config config)
    {
        _settingId = settingId;
        _config = config;
    }

    public void WhenSettingUpdated(object newValue)
    {
        _config.PersistConfigValue(_settingId,newValue.ToString());
    }
}

[Immanence(typeof(Config))]
public class Config
{
    public bool skiplogo;
    public bool knock;
    
    private Dictionary<string, string> _configValues;

    public Config()
    {
        
        
        var comparer = StringComparer.OrdinalIgnoreCase;
        _configValues = new Dictionary<string, string>(comparer);

        PopulateConfigValuesFromIniFile();
        

        if (string.IsNullOrEmpty(GetConfigValue(NoonConstants.CULTURE_SETTING_KEY)))
        PersistConfigValue(NoonConstants.CULTURE_SETTING_KEY,
            DetermineMostSuitableCultureId());


      if (string.IsNullOrEmpty(GetConfigValue(NoonConstants.IMAGES_FOLDER_NAME_KEY)))
            PersistConfigValue(NoonConstants.IMAGES_FOLDER_NAME_KEY,
                NoonConstants.DEFAULT_IMAGES_FOLDER_NAME);

      if (string.IsNullOrEmpty(GetConfigValue(NoonConstants.MUSIC_FOLDER_NAME_KEY)))
          PersistConfigValue(NoonConstants.MUSIC_FOLDER_NAME_KEY,
              NoonConstants.DEFAULT_MUSIC_FOLDER_NAME);

    }

    public GameId GetGame()
    {
        var gameIdString = GetConfigValueAsString(NoonConstants.GAME_ID_KEY);
        try
        {
            GameId gameIdAsEnum = (GameId)Enum.Parse(typeof(GameId), gameIdString);

            return gameIdAsEnum;
        }
        catch (Exception)
        {
            return GameId.XX;
        }
    }
    public void SetGame(GameId game)
    {

        PersistConfigValue(NoonConstants.GAME_ID_KEY, game.ToString());

        string gameSpecificContentFolderName = NoonConstants.DEFAULT_CONTENT_FOLDER_NAME;
        if (game != GameId.CS) //first and most honourable backwards compat hack
            gameSpecificContentFolderName = (game.ToString() + gameSpecificContentFolderName).ToLower();
        //set content folder based on game. If we want to set it arbitrarily, we'll need to widen what's allowed.
        PersistConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY,
            gameSpecificContentFolderName);

        string gameSpecificImageFolderName = NoonConstants.DEFAULT_IMAGES_FOLDER_NAME;
        if (game != GameId.CS)
            gameSpecificImageFolderName = (game.ToString() + gameSpecificImageFolderName).ToLower();
        //set content folder based on game. If we want to set it arbitrarily, we'll need to widen what's allowed.
        PersistConfigValue(NoonConstants.IMAGES_FOLDER_NAME_KEY,
            gameSpecificImageFolderName);

        string gameSpecificMusicFolderName = NoonConstants.DEFAULT_MUSIC_FOLDER_NAME;
        if (game != GameId.CS)
            gameSpecificMusicFolderName = (game.ToString() + gameSpecificMusicFolderName).ToLower();

        PersistConfigValue(NoonConstants.MUSIC_FOLDER_NAME_KEY,
            gameSpecificMusicFolderName);

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

    public void PopulateConfigValuesFromIniFile()
    {
        
        if (File.Exists(GetConfigFileLocation()))
        {

            _configValues = PopulateConfigValues(GetConfigFileLocation());

            string skipLogoValue = GetConfigValue("skiplogo");
            if (!String.IsNullOrEmpty(skipLogoValue) && skipLogoValue != "0")
            {
                skiplogo = true;
            }


            string knockValue = GetConfigValue("knock");
            if (!String.IsNullOrEmpty(knockValue) && knockValue!="0" )
            {
                knock = true;
            }
        }
        else
        {
            PersistConfigValue("skiplogo", "0");
            PersistConfigValuesToIniFile();
        }

    }

    public void MigrateAnySettingValuesInRegistry(Compendium compendium)
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
                    setting.CurrentValue = valueToSet;
   
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


    /// <summary>
    /// This only saves the value to the config. It doesn't update any associated Settings! (which communicate their changes directly to Config)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void PersistConfigValue(string key, string value)
    {
        if (string.IsNullOrEmpty(value) || value == "0" || value == "false")
            value = "0";

        _configValues[key] = value;

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

    /// <summary>
    /// Returns all entries inside the config value as a string array.
    /// </summary>
    /// <param name="forId"></param>
    /// <returns></returns>
    public string[] GetConfigValueAsArray(string forId)
    {
        string configValue = GetConfigValue(forId) ?? string.Empty;
        return configValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    private Dictionary<string,string> PopulateConfigValues(string configLocation)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        Dictionary<string, string> dictToPopulate=new Dictionary<string, string>(comparer);
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

            case SystemLanguage.Japanese:
                return "jp";

            case SystemLanguage.German:
                return "de";

            default:
                return (NoonConstants.DEFAULT_CULTURE_ID);


                
        }

    }

}
