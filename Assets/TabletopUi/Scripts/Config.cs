using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Assets.TabletopUi.Scripts.Infrastructure;
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
    /// returns an emmpty string if the value doeesn't exist, or is '0', or 'false' or an empty string
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetConfigValue(string key)
    {
        ConfigValues.TryGetValue(key, out string value);

        if (string.IsNullOrEmpty(value) || value == "0" || value == "false" )
            value = string.Empty;
        return value;
    }


    public void ReadFromIniFile()
    {
        string configLocation = Application.persistentDataPath + "/config.ini";


        if (File.Exists(configLocation))
        {

            ConfigValues=PopulateConfigValues(configLocation);


            CultureId = GetStartingCultureId(GetConfigValue("culture"));

            if (GetConfigValue("skiplogo")!=String.Empty)
            {
                skiplogo = true;
            }

            if (GetConfigValue("verbosity") == String.Empty)
                verbosity = (int)VerbosityLevel.Significants;
            else
            {
                int.TryParse(GetConfigValue("verbosity"), out verbosity);
                NoonUtility.CurrentVerbosity = verbosity;
            }

            if (GetConfigValue("knock")!=String.Empty)
            {
                knock = true;
            }


        }
        else
        {

            File.WriteAllText(configLocation, "skiplogo=0");
        }

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



    //public class OldConfig
    //{
    //    public bool skiplogo = false;
    //    public int verbosity = 0;
    //    public bool knock = false;
    //    public string culture;

    //    // simple singleton declaration
    //    private static OldConfig _instance;

    //    public static OldConfig Instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //            {
    //                _instance = new OldConfig();
    //                _instance.Read();
    //            }

    //            return _instance;
    //        }
    //    }

    //    // Use this for initialization
    //    public void Read()
    //    {
    //        string hackyConfigLocation = Application.persistentDataPath + "/config.ini";

    //        if (File.Exists(hackyConfigLocation))
    //        {
    //            string contents = File.ReadAllText(hackyConfigLocation);
    //            if (contents.Contains("skiplogo=1"))
    //            {
    //                skiplogo = true;
    //            }

    //            if (contents.Contains("verbosity=10")) //yeah I know. Sorry future me
    //            {
    //                verbosity = 10;
    //                NoonUtility.CurrentVerbosity = 10;
    //            }

    //            if (contents.Contains("knock=1"))
    //            {
    //                knock = true;
    //            }

    //            if (contents.Contains("lang=en"))
    //            {
    //                culture = "en";
    //            }
    //            else if (contents.Contains("lang=ru"))
    //            {
    //                culture = "ru";
    //            }
    //            else if (contents.Contains("lang=zh"))
    //            {
    //                culture = "zh-hans";
    //            }
    //            else
    //            {
    //                culture = null;
    //            }
    //        }
    //        else
    //        {

    //            File.WriteAllText(hackyConfigLocation, "skiplogo=0");
    //        }
    //    }

//    }
//}