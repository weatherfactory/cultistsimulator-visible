using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Noon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashAnimation : MonoBehaviour
{
    public VideoPlayer player;
	// Use this for initialization
	void Start ()
	{
	    string hackyConfigLocation = Application.persistentDataPath + "/config.ini";

	    if (File.Exists(hackyConfigLocation))
	    {
	        string contents = File.ReadAllText(hackyConfigLocation);
            if(contents.Contains("skiplogo=1"))
            { 
	            SceneManager.LoadScene(SceneNumber.QuoteScene);
	            return;
            }

	        if (contents.Contains("verbosity=10")) //yeah I know. Sorry future me
	        {
	            NoonUtility.CurrentVerbosity = 10;
	        }
	    }
	    else
	    {
	        
            File.WriteAllText(hackyConfigLocation,"skiplogo=0");
	    }
	    

        player.loopPointReached += EndReached;
	    try
	    {
	        player.Play();

        }
	    
    catch (Exception e)
    {
        NoonUtility.Log(e.Message,11);
        SceneManager.LoadScene(SceneNumber.QuoteScene);
        
    }

	    
    }
	
	void EndReached (VideoPlayer p) {
		SceneManager.LoadScene(SceneNumber.QuoteScene);
	}
}
