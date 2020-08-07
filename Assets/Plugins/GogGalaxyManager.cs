using UnityEngine;
using System.Collections;
using System.IO;
//
// The GogGalaxyManager provides a base implementation of GOGGalaxy C# wrapper on which you can build upon.
// It handles the basics of starting up and shutting down the GOG Galaxy for use.
//
#if UNITY_STANDALONE_LINUX
public class GogGalaxyManager : MonoBehaviour
    {
  //  public string clientID = "foo";
  //  public string clientSecret = "bar";
//used to be that adding the fields here too stops the odd build error we get otherwise when we try to build just Linux after the editor knows about the fields. That doesn't seem to be the case any more.
       private void Awake()
    {
    Debug.Log("Linux build: not initialising GOG Galaxy, cos there's no Linux support for it yet.");
    }

       private bool isInitialized = false;

    public static bool IsInitialized()
       {
           return false;
       }
}
#elif UNITY_WEBGL
public class GogGalaxyManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("WebGL build: not initialising GOG Galaxy, cos there's no support for it.");
    }
}

#else
using Galaxy.Api;

[DisallowMultipleComponent]
public class GogGalaxyManager : MonoBehaviour
{
    private string clientID = "50757209545787544";
    private string clientSecret = "72e691b01ad6060c8716bb4155b305c68048585aae07d1227eecc5a6c959161c";

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
        //we're integrating with GOG again
        //if (Application.platform == RuntimePlatform.OSXPlayer)
        //{
        //    Debug.Log("Not currently integrating with Galaxy on OSX");
        //    return;
        //}
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;

        // We want our GogGalaxyManager Instance to persist across scenes.
        DontDestroyOnLoad(gameObject);

        if(!isInitialized)
        { 
        try
        {
            InitParams initParams = new InitParams(clientID, clientSecret);

            GalaxyInstance.Init(initParams);
        }
        catch (GalaxyInstance.Error error)
        {
            Debug.LogWarning("Failed to initialize GOG Galaxy: Error = " + error.ToString(), this);
            return;
        }

        //Debug.Log("Galaxy SDK was initialized", this);

        isInitialized = true;
        }
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