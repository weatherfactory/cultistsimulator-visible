using UnityEngine;
using System;

using System.Collections;
using ContentClasses;
using JetBrains.Annotations;

public class ContentManager : Singleton<ContentManager>
{

    public string Status = "";
    private string logMsgs = "";



    public Hashtable ImportVerbs()
    {
        string json = Resources.Load<TextAsset>("content/verbs").text;
        return SimpleJsonImporter.Import(json);
    }

    public void AddToLog(string msg)
    {
        Debug.Log(msg + "\n" + DateTime.Now.ToString("yyy/MM/dd hh:mm:ss.fff"));

        // for some silly reason the Editor will generate errors if the string is too long
        int lenNeeded = msg.Length + 1;
        if (logMsgs.Length + lenNeeded > 4096) logMsgs = logMsgs.Substring(0, 4096 - lenNeeded);

        logMsgs = logMsgs + "\n" + msg;
    }
}
