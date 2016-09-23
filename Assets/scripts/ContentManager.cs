using UnityEngine;
using System;

using System.Collections;
using ContentClasses;
using JetBrains.Annotations;

public class ContentManager : Singleton<ContentManager>
{

    public string Status = "";
    private string logMsgs = "";

    //public Verb createTestVerb(string d)
    //{
    //    Verb v=new Verb();
    //    v.Id = d+ " i";
    //    v.Description = d + " d";
    //    v.Label = d + " l";
    //    return v;
    //}

    //public Verb[] createTestVerbArray()
    //{
    //    Verb v1 = createTestVerb("foo");
    //    Verb v2 = createTestVerb("bar");
    //    Verb[] va = new Verb[2];
    //    va[0] = v1;
    //    va[1] = v2;
    //    return va;
    //}


    //public string SerializeVerbArray(Verb[] va)
    //{
    //    return JsonUtility.ToJson(va);
    //}

    public Hashtable ImportVerbs()
    {
        string json = Resources.Load<TextAsset>("content/verbs").text;
        return SimpleJsonImporter.Import(json);
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
