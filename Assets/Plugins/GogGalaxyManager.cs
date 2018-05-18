﻿#if UNITY_STANDALONE_LINUX
#else
using System;
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
            GalaxyInstance.User().SignIn();
        }
    catch (GalaxyInstance.InvalidStateError e)
    {
        Console.WriteLine("Invalid state error for GOG Galax, probably already initialised: " + e.Message);

    }
        catch (GalaxyInstance.Error error)
        {
            Console.WriteLine("Failed to initialize GOG Galaxy: Error = " + error.ToString(), this);
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
