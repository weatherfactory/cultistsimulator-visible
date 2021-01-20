using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.UI;
using SecretHistories.Services;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashAnimation : MonoBehaviour
{
    public VideoPlayer player;
	void Awake()
	{

	}
	// Use this for initialization
	void Start ()
	{

        player.loopPointReached += EndReached;
	    try
	    {
	        player.Play();

        }
	    
        catch (Exception e)
        {
            NoonUtility.Log(e.Message,0);
            Watchman.Get<StageHand>().QuoteScreen();
            
        
        }

	    
    }
	
	void EndReached (VideoPlayer p)
    {
        Watchman.Get<StageHand>().QuoteScreen();
    }
}
