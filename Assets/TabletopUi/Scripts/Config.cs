using System.Collections;
using System.Collections.Generic;
using System.IO;
using Noon;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Config
{
	public bool skiplogo = false;
	public int verbosity = 0;
    public bool knock = false;

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
	            return;
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
        }
	    else
	    {
	        
            File.WriteAllText(hackyConfigLocation,"skiplogo=0");
	    }
	}

}
