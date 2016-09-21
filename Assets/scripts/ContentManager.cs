using UnityEngine;
using System;

using System.Collections;

public class ContentManager : Singleton<ContentManager>
{

    public string Status = "aleph";
    private string logMsgs = "";


    public Hashtable ImportActionsFromJSON()
    {
        TextAsset actionsJson = Resources.Load<TextAsset>("content/actions");
       AddToLog("Importing Json string");
        return SimpleJsonImporter.Import(actionsJson.text, false);
    }

    

    private void AddToLog(string msg)
    {
        Debug.Log(msg + "\n" + DateTime.Now.ToString("yyy/MM/dd hh:mm:ss.fff"));

        // for some silly reason the Editor will generate errors if the string is too long
        int lenNeeded = msg.Length + 1;
        if (logMsgs.Length + lenNeeded > 4096) logMsgs = logMsgs.Substring(0, 4096 - lenNeeded);

        logMsgs = logMsgs + "\n" + msg;
    }
}
