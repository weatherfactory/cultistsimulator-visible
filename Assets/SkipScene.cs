using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //with luck this is redundant if the .webm file works on multiple machines
	    SceneManager.LoadScene(SceneNumber.QuoteScene);
    }
	

}
