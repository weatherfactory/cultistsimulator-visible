using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using TMPro;
using UnityEngine;


public static class WatcherQueue
{
    public static string LastFileChange;
    public static bool NeedsRevalidation;
    
    public static void Reset()
    {
        NeedsRevalidation = false;
    }

    public static void NotifyContentFileChanged(string fileChange)
    {
        LastFileChange = fileChange;
        NeedsRevalidation = true;
        Debug.Log($" - filechange at {DateTime.Now}...");
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
    private bool validationInProgress = false;

    void Initialise()
    {
        WatcherQueue.Reset();
        StatusMessages.text="Content Watcher ready at " + DateTime.Now.ToShortTimeString();
        ValidationMessages.text = String.Empty;
        validationInProgress = false;

        FileSystemWatcher watcher = new FileSystemWatcher
        {
            Path = Application.streamingAssetsPath + "/content/core/",
            NotifyFilter = NotifyFilters.LastAccess
                           | NotifyFilters.LastWrite
                           | NotifyFilters.FileName
                           | NotifyFilters.DirectoryName,
            Filter = "*.json",
            IncludeSubdirectories = true
        };

        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;

        // Begin watching.
        watcher.EnableRaisingEvents = true;
        StatusMessages.text="Watching " + watcher.Path;
        Initialised = true;
    }

    // Define the event handlers.
    private static void OnChanged(object source, FileSystemEventArgs e) =>
        WatcherQueue.NotifyContentFileChanged($"{e.FullPath} {e.ChangeType}");


async void Update()
    {
        //if (Application.isPlaying)
        //{
        //    this.gameObject.SetActive(false);
        //    return;
        //}

        //if (!Initialised)
        //    Initialise();
        
        //if (WatcherQueue.NeedsRevalidation && !validationInProgress)
        //{
        //    Debug.Log("Validating at " + DateTime.Now.ToLongTimeString());

        //    ValidationBeginning();
        //    Task<AsyncContentImportResult> resultsTask = Validate();
        //    AsyncContentImportResult result = await resultsTask;
        //    ValidationMessages.text = result.ContentImportMessages.Aggregate(string.Empty, (current, m) => current + (m.Description + "\n"));
        //    ValidationComplete();
        //}
    }

private void ValidationBeginning()
{
    StatusMessages.text = "Began validation at " + DateTime.Now.ToLongTimeString();
     ValidationMessages.text = string.Empty;
    validationInProgress = true;
}

private void ValidationComplete()
{
//    WatcherQueue.NeedsRevalidation = false;
//    validationInProgress = false;
//    StatusMessages.text += "\n\nCompleted validation at " + DateTime.Now.ToLongTimeString();
//}

//    private async Task<AsyncContentImportResult> Validate()
//    {
//        WatcherQueue.NeedsRevalidation = false;
//        AsyncContentImportResult result;

//#if MODS
//        new Registry().Register(new ModManager(false));
//#endif
//        var contentImporter = new ContentImporter();
//        var contentImportMessages = contentImporter.PopulateCompendium(new Compendium());
//        var importantMessages = contentImportMessages.Where(i => i.MessageLevel > 0).ToList();

//        if (!importantMessages.Any())
//            result=new AsyncContentImportResult(new ContentImportMessage($"All content good at {DateTime.Now.ToLongTimeString()}"));
//        else
//            result=new AsyncContentImportResult(importantMessages);
        
//        return result;
    }

}

public class AsyncContentImportResult
{
    public List<ContentImportMessage> ContentImportMessages;

    public AsyncContentImportResult(List<ContentImportMessage> messages)
    {
        ContentImportMessages = messages.ToList();
    }

    public AsyncContentImportResult(ContentImportMessage message)
    {
        ContentImportMessages = new List<ContentImportMessage>{message};
    }

}
