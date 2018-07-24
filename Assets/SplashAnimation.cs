using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashAnimation : MonoBehaviour
{
    public VideoPlayer player;
	// Use this for initialization
	void Start ()
	{
	    player.loopPointReached += EndReached;
	}
	
	void EndReached (VideoPlayer p) {
		SceneManager.LoadScene(SceneNumber.QuoteScene);
	}
}
