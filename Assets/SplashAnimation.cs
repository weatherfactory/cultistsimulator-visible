using System;
using System.Collections;
using System.Collections.Generic;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
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
            Registry.Get<StageHand>().SceneChange(SceneNumber.QuoteScene);
            
        
        }

	    
    }
	
	void EndReached (VideoPlayer p)
    {
        Registry.Get<StageHand>().SceneChange(SceneNumber.QuoteScene);
    }
}
