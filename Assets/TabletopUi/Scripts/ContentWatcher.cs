using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using TMPro;
using UnityEngine;


public static class WatcherQueue
{
    public static List<string> StatusMessages=new List<string>();
    public static List<string> ValidationMessages= new List<string>();

    public static bool NeedsRevalidation;
    public static bool InProgress;

    public static void Reset()
    {
        StatusMessages = new List<string>();
        ValidationMessages = new List<string>();

        InProgress = false;
        NeedsRevalidation = false;
    }

    public static void AddStatusMessage(string message)
    {
        StatusMessages.Add(message);
    }

    public static void AddValidationMessage(string message)
    {
        ValidationMessages.Add(message);
    }
    public static void ClearValidations()
    { ValidationMessages.Clear();}

    public static string GetStatus()
    {
        string status = string.Empty;
        foreach (var s in StatusMessages)
        {
            status += (s + "\n");
        }

        return status;

        //return DateTime.Now.ToLongTimeString();
    }

    public static string GetValidations()
    {
        string validations = string.Empty;
        foreach (var v in ValidationMessages)
        {
            validations += (v + "\n");
        }

        return validations;
        //   return DateTime.Now.ToLongTimeString(); ;
    }


    public static void NotifyContentFileChanged(string message)
    {
      //  AddStatusMessage(message + $" - revalidating at { DateTime.Now}...");
      AddStatusMessage($" - revalidating at {DateTime.Now}...");
        NeedsRevalidation = true;
        Debug.Log($" - revalidating at {DateTime.Now}...");

    }

}

[ExecuteAlways]
public class ContentWatcher : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] TMP_Text StatusMessages;
    [SerializeField] TMP_Text ValidationMessages;
#pragma warning restore 649
    // Start is called before the first frame update

    public bool Initialised = false;

    void Initialise()
    {
        WatcherQueue.Reset();

        FileSystemWatcher watcher = new FileSystemWatcher
        {
            Path = Application.streamingAssetsPath + "/content/core/recipes/",
            NotifyFilter = NotifyFilters.LastAccess
                           | NotifyFilters.LastWrite
                           | NotifyFilters.FileName
                           | NotifyFilters.DirectoryName,
            Filter = "*.json"
         //   IncludeSubdirectories = true
        };

        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;

        // Begin watching.
        watcher.EnableRaisingEvents = true;
        WatcherQueue.AddStatusMessage("Watching " + watcher.Path);
        Initialised = true;
    }

    // Define the event handlers.
    private static void OnChanged(object source, FileSystemEventArgs e) =>

        WatcherQueue.NotifyContentFileChanged($"{e.FullPath} {e.ChangeType}");



void Update()
    {
        if (Application.isPlaying)
        {
            this.gameObject.SetActive(false);
            return;
        }


        if (!Initialised)
            Initialise();

        StatusMessages.text = WatcherQueue.GetStatus();
        ValidationMessages.text = WatcherQueue.GetValidations();


        if (WatcherQueue.NeedsRevalidation && !WatcherQueue.InProgress)
        {
            Validate();
        }


    }

    private void Validate()
    {
        WatcherQueue.InProgress = true;
        WatcherQueue.ClearValidations();
#if MODS
        new Registry().Register(new ModManager(false));
#endif
        var contentImporter = new ContentImporter();
        var validationInfoMessages = contentImporter.PopulateCompendium(new Compendium());

        var importantMessages = validationInfoMessages.Where(i => i.MessageLevel > 0);

        var contentImportMessages = importantMessages.ToList();
        if (!contentImportMessages.Any())
            WatcherQueue.AddValidationMessage($"All content good at {DateTime.Now.ToShortTimeString()}");
        else
        {
            foreach (var p in contentImportMessages)
                WatcherQueue.AddValidationMessage(p.Description);
        }


        WatcherQueue.InProgress = false;
        WatcherQueue.NeedsRevalidation = false;

    }

}
