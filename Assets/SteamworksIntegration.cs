using System.Collections;
using System.Collections.Generic;
using Noon;
using UnityEngine;

public class SteamworksIntegration : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    Facepunch.Steamworks.Config.ForUnity(Application.platform.ToString());
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
