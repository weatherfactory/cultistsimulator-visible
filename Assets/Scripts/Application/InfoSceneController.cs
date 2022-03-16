using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Constants.Modding;
using SecretHistories.UI;
using UnityEngine;

public class InfoSceneController : MonoBehaviour
{
    [SerializeField] private GameObject modWarning;
    public void Start()
    {
        var modManager = Watchman.Get<ModManager>();
        if (modManager == null)
            return;
        try
        {
         
            var enabledMods = modManager.GetEnabledModsLoadOrderList();
        
            if(enabledMods.Count>0)

                modWarning.SetActive(true);
            else
                modWarning.SetActive(false);


        }
        catch (Exception e)
        {
            NoonUtility.Log($"Error when trying to establish modfulness in error scene: {e.ToString()}");
        }

    }
    public void BrowseToFiles()
    {
        OpenInFileBrowser.Open(Application.persistentDataPath);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
