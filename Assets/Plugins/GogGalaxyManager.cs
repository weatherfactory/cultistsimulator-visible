
using System;
using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_STANDALONE_LINUX
public class GogGalaxyManager : MonoBehaviour
    {
       private void Awake()
    {
    Debug.Log("Linux build: not initialising GOG Galaxy, cos there's no Linux support for it yet.");
    }
}
#elif UNITY_STANDALONE_OSX
public class GogGalaxyManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("OSX build: not initialising GOG Galaxy, because it's temporarily disabled");
    }
}
#else
using Galaxy.Api;


//
// The GogGalaxyManager provides a base implementation of GOGGalaxy C# wrapper on which you can build upon.
// It handles the basics of starting up and shutting down the GOG Galaxy for use.
//
[DisallowMultipleComponent]
public class GogGalaxyManager : MonoBehaviour
{
    
    private string clientID= "50757209545787544";
    private string clientSecret= "72e691b01ad6060c8716bb4155b305c68048585aae07d1227eecc5a6c959161c";

    private static GogGalaxyManager singleton;
    public static GogGalaxyManager Instance
    {
        get
        {
            if (singleton == null)
            {
                return new GameObject("GogGalaxyManager").AddComponent<GogGalaxyManager>();
            }
            else {
                return singleton;
            }
        }
    }

    private bool isInitialized = false;

    public static bool IsInitialized()
    {
        return singleton != null && singleton.isInitialized;
    }

    private void Awake()
    {
        
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;

        // We want our GogGalaxyManager Instance to persist across scenes.
        DontDestroyOnLoad(gameObject);
    try
        {
            InitParams initParams = new InitParams(clientID, clientSecret);

            GalaxyInstance.Init(initParams);
        }
    catch (GalaxyInstance.InvalidStateError e)
    {
        Debug.Log("Invalid state error for GOG Galax, probably already initialised: " + e.Message);

    }
        catch (GalaxyInstance.Error error)
        {
            Debug.Log("Failed to initialize GOG Galaxy: Error = " + error.ToString(), this);
            return;
        }

        

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (singleton != this)
        {
            return;
        }

        singleton = null;

        if (!isInitialized)
        {
            return;
        }

        GalaxyInstance.Shutdown(true);
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        GalaxyInstance.ProcessData();
    }

}
#endif
