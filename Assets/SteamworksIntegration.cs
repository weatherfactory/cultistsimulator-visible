using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Facepunch.Steamworks;
using Noon;


public class SteamworksIntegration: MonoBehaviour
    {
    //thanks Garry for the API wrapper, Kyle Kukshtel for the guidance!
    void Start()
    {
        // Don't destroy this when loading new scenes
        DontDestroyOnLoad(gameObject);

        // Configure for Unity
        // This is VERY important - call this before doing anything
        Facepunch.Steamworks.Config.ForUnity(Application.platform.ToString());

        if (Client.Instance != null)
            return;

        

        // Create the steam client using the test AppID (or your own AppID eventually)
       var client=new Facepunch.Steamworks.Client(NoonUtility.CultistSimulatorSteamAppId);

        // Make sure we started up okay
        if (Client.Instance == null)
        {
            NoonUtility.Log("Couldn't start Steam. Which is fine, if this is a non-Steam build, sorry to bug you even",1);
            return;
        }

     //   Client.Instance.Achievements.Refresh();
    }

    private void OnDestroy()
    {
        if (Client.Instance != null)
        {
            // Properly get rid of the client if this object is destroyed
            Client.Instance.Dispose();
        }

    }


    void Update()
    {
        if (Client.Instance != null)
        {
            // This needs to be called in Update for the library to properly function
            Client.Instance.Update();
        }

    }

}



