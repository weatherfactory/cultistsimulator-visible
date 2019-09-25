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
    public string culture;
    public bool showAllLanguages;

    // simple singleton declaration
    private static Config _instance;
    public static Config Instance
    {
        get
        {
            if (_instance == null)
			{
                _instance = new Config();
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
            if(contents.Contains("skiplogo=1"))
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

	        if (contents.Contains("showAllLanguages=1"))
	        {
		        showAllLanguages = true;
	        }
        }
	    else
	    {
	        
            File.WriteAllText(hackyConfigLocation,"skiplogo=0");
	    }
	}

}
