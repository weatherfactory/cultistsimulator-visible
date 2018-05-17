using System;
using Unity.Cloud.BugReporting;
using Unity.Cloud.BugReporting.Plugin;
using UnityEngine;

public class BugReportingConfigureOnly : MonoBehaviour
{
    #region Methods

    private void Start()
    {
        if (UnityBugReporting.CurrentClient == null)
        {
            UnityBugReporting.Configure();
        }
    }

    #endregion
}