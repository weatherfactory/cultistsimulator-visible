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

	        NoonUtility.Log("Steamworks integration startup.", 1);
            Facepunch.Steamworks.Config.ForUnity(Application.platform.ToString());
	        if (Client.Instance == null)
	            new Facepunch.Steamworks.Client(SteamAppId);

	        NoonUtility.Log("Initialised Facepunch Steamworks client", 1);

        }
        catch (Exception e)
	    {
            NoonUtility.Log("Failed Steamworks integration startup with error " + e.Message + " which is fine if you're not on Steam.",1);

            NoonUtility.Log(e.StackTrace);
	    }
    }
	
	
	void OnDestroy ()
	{
	    if (Client.Instance != null)
	    {
	        Client.Instance.Dispose();
	    }
    }
}
