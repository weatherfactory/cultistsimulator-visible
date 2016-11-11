using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// I would expect that a lot of stuff that currently goes through the log panel will also appear in popup notifications.
/// </summary>
public class LogPanel : MonoBehaviour
{

    [SerializeField] private Text txtLog;

    private List<String> _messages=new List<string>();
    private const int MESSAGES_LIMIT = 8;
        public void Write(string newMessage)
        {
            try
            {
            if(_messages.Count>=MESSAGES_LIMIT)
                _messages.RemoveAt(0);
            _messages.Add(newMessage);

                txtLog.text = "";
                foreach (var existingMessage in _messages)
            
                txtLog.text += existingMessage + "\n";

            }

            catch (Exception e)
            {
                UnityEngine.Debug.Log("Tried to write to game log, but this happened: " + e.Message + " " + e.TargetSite +
                                      " " + e.StackTrace);
                UnityEngine.Debug.Log("Original message: " + newMessage);
            }
        
    }
}
