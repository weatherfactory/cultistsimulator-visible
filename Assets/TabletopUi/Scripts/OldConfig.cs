using System.Collections;
using System.Collections.Generic;
using System.IO;
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


    public void ReadFromIniFile()
    {
        string hackyConfigLocation = Application.persistentDataPath + "/config.ini";

        CultureId = "en";

        //if (File.Exists(hackyConfigLocation))
        //{
        //    string contents = File.ReadAllText(hackyConfigLocation);
        //    if (contents.Contains("skiplogo=1"))
        //    {
        //        skiplogo = true;
        //    }

        //    if (contents.Contains("verbosity=10")) //yeah I know. Sorry future me
        //    {
        //        verbosity = 10;
        //        NoonUtility.CurrentVerbosity = 10;
        //    }

        //    if (contents.Contains("knock=1"))
        //    {
        //        knock = true;
        //    }

        //    if (contents.Contains("lang=en"))
        //    {
        //        CultureId = "en";
        //    }
        //    else if (contents.Contains("lang=ru"))
        //    {
        //        CultureId = "ru";
        //    }
        //    else if (contents.Contains("lang=zh"))
        //    {
        //        CultureId = "zh-hans";
        //    }
        //    else
        //    {
        //        CultureId = null;
        //    }
        //}
        //else
        //{

        //    File.WriteAllText(hackyConfigLocation, "skiplogo=0");
        //}


    }

    public class OldConfig
    {
        public bool skiplogo = false;
        public int verbosity = 0;
        public bool knock = false;
        public string culture;

        // simple singleton declaration
        private static OldConfig _instance;

        public static OldConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OldConfig();
                    _instance.Read();
                }

                return _instance;
            }
        }

        // Use this for initialization
        public void Read()
        {
            string hackyConfigLocation = Application.persistentDataPath + "/config.ini";

            if (File.Exists(hackyConfigLocation))
            {
                string contents = File.ReadAllText(hackyConfigLocation);
                if (contents.Contains("skiplogo=1"))
                {
                    skiplogo = true;
                }

                if (contents.Contains("verbosity=10")) //yeah I know. Sorry future me
                {
                    verbosity = 10;
                    NoonUtility.CurrentVerbosity = 10;
                }

                if (contents.Contains("knock=1"))
                {
                    knock = true;
                }

                if (contents.Contains("lang=en"))
                {
                    culture = "en";
                }
                else if (contents.Contains("lang=ru"))
                {
                    culture = "ru";
                }
                else if (contents.Contains("lang=zh"))
                {
                    culture = "zh-hans";
                }
                else
                {
                    culture = null;
                }
            }
            else
            {

                File.WriteAllText(hackyConfigLocation, "skiplogo=0");
            }
        }

    }
}