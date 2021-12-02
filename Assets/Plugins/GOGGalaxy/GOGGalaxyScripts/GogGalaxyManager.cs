
#if UNITY_STANDALONE_LINUX

using UnityEngine;
using System.Collections;
using System.IO;


//
// The GogGalaxyManager provides a base implementation of GOGGalaxy C# wrapper on which you can build upon.
// It handles the basics of starting up and shutting down the GOG Galaxy for use.
//
[DisallowMultipleComponent]
public class GogGalaxyManager : MonoBehaviour
{
    public string clientID;
    public string clientSecret;

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
        return false;
    }

    
    public void TryInitialise()
    {
   
        Debug.Log("Galaxy SDK can't be initialised on Linux, sorry", this);
        
    }


    public void DoUpdate()
    {
     
    }
}
#else

using UnityEngine;
using System.Collections;
using System.IO;
using Galaxy.Api;


//
// The GogGalaxyManager provides a base implementation of GOGGalaxy C# wrapper on which you can build upon.
// It handles the basics of starting up and shutting down the GOG Galaxy for use.
//
[DisallowMultipleComponent]
public class GogGalaxyManager : MonoBehaviour
{
    public string clientID;
    public string clientSecret;

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
       // TryInitialise();

    }

    public void TryInitialise()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;

        // We want our GogGalaxyManager Instance to persist across scenes.
        DontDestroyOnLoad(gameObject);

        // Make sure clientID and clientSecret are initialized
        Debug.Assert(clientID != default(string) || clientSecret != default(string), "ClientID and/or ClientSecret are not specified");

        try
        {
            InitParams initParams = new InitParams(clientID, clientSecret);

            GalaxyInstance.Init(initParams);
        }
        catch (GalaxyInstance.Error error)
        {
            Debug.LogError("Failed to initialize GOG Galaxy: Error = " + error.ToString(), this);
            return;
        }

        Debug.Log("Galaxy SDK was initialized", this);

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

        // PS4 requires explicit loading/unloading dependency
        // this parameter is ignored for all platforms other than PS4
        GalaxyInstance.Shutdown(true);
    }

    public void DoUpdate()
    {
        if (!isInitialized)
        {
            return;
        }

        GalaxyInstance.ProcessData();
    }
}
#endif
