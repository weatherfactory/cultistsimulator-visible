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
	    Facepunch.Steamworks.Config.ForUnity(Application.platform.ToString());
        if(Client.Instance==null)
            new Facepunch.Steamworks.Client(SteamAppId);
    }
	
	// Update is called once per frame
	void Update ()
	{

	}
}
