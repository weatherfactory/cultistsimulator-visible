using System;
using System.Collections;
using System.Collections.Generic;
using Facepunch.Steamworks;
using Noon;
using UnityEngine;

public class SteamworksIntegration : MonoBehaviour
{

    private uint SteamAppId=718670;

    private Facepunch.Steamworks.Client client;

    void Start () {

	    try
	    {

	        NoonUtility.Log("Steamworks integration startup.", 8);
            Facepunch.Steamworks.Config.ForUnity(Application.platform.ToString());
	        if (Client.Instance == null || !Client.Instance.IsValid)
	          client= new Facepunch.Steamworks.Client(SteamAppId);

	        if (!client.IsValid)
	        {
	           Debug.Log("Couldn't initialise Steam client");

            }

            NoonUtility.Log("Initialised Facepunch Steamworks client", VerbosityLevel.SystemChatter);

        }
        catch (Exception e)
	    {
            NoonUtility.Log("Failed Steamworks integration startup with error " + e.Message + " which is fine if you're not on Steam.", VerbosityLevel.SystemChatter);

            NoonUtility.Log(e.StackTrace);
	    }
    }


    public static void Release()
    {
        if (Client.Instance != null)
        {
            NoonUtility.Log("Shutting down Steamworks client",VerbosityLevel.SystemChatter);
            Client.Instance.Dispose();
        }
    }



}
