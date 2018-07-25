using System;
using System.Collections;
using System.Collections.Generic;
using Facepunch.Steamworks;
using Noon;
using UnityEngine;

public class SteamworksIntegration : MonoBehaviour
{

    private uint SteamAppId=718670;
	// Use this for initialization
	void Start () {

	    try
	    {
	        Facepunch.Steamworks.Config.ForUnity(Application.platform.ToString());
	        if (Client.Instance == null)
	            new Facepunch.Steamworks.Client(SteamAppId);
	    }
	    catch
	    {
            NoonUtility.Log("Failed Steam integration startup, which is fine if you're not on Steam.",1);
	    }
    }
	
	// Update is called once per frame
	void Update ()
	{

	}
}
