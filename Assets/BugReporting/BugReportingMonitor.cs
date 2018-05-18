using System;
using Unity.Cloud.BugReporting;
using Unity.Cloud.BugReporting.Plugin;
using UnityEngine;

public class BugReportingMonitor : MonoBehaviour
{
    #region Constructors

    public BugReportingMonitor()
    {
        this.IsEnabled = true;
        this.IsHiddenWithoutDimension = true;
        Type type = this.GetType();
        this.MonitorName = type.Name;
    }

    #endregion

    #region Fields

    public bool IsEnabled;

    public bool IsEnabledAfterTrigger;

    public bool IsHiddenWithoutDimension;

    public string MonitorName;

    public string Summary;

    #endregion

    #region Methods

    private void Start()
    {
        if (UnityBugReporting.CurrentClient == null)
        {
            UnityBugReporting.Configure();
        }
    }

    public void Trigger()
    {
        if (!this.IsEnabledAfterTrigger)
        {
            this.IsEnabled = false;
        }
        UnityBugReporting.CurrentClient.TakeScreenshot(2048, 2048, s => { });
        UnityBugReporting.CurrentClient.TakeScreenshot(512, 512, s => { });
        UnityBugReporting.CurrentClient.CreateBugReport((br) =>
        {
            if (string.IsNullOrEmpty(br.ProjectIdentifier))
            {
                Debug.LogWarning("The bug report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityBugReporting.Configure().");
            }
            br.Summary = this.Summary;
            br.DeviceMetadata.Add(new BugReportNamedValue("Monitor", this.MonitorName));
            string platform = "Unknown";
            string version = "0.0";
            foreach (BugReportNamedValue deviceMetadata in br.DeviceMetadata)
            {
                if (deviceMetadata.Name == "Platform")
                {
                    platform = deviceMetadata.Value;
                }
                if (deviceMetadata.Name == "Version")
                {
                    version = deviceMetadata.Value;
                }
            }
            br.Dimensions.Add(new BugReportNamedValue("Monitor.Platform.Version", string.Format("{0}.{1}.{2}", this.MonitorName, platform, version)));
            br.Dimensions.Add(new BugReportNamedValue("Monitor", this.MonitorName));
            br.IsHiddenWithoutDimension = this.IsHiddenWithoutDimension;
            UnityBugReporting.CurrentClient.SendBugReport(br, (success, br2) => { this.Triggered(); });
        });
    }

    #endregion

    #region Virtual Methods

    protected virtual void Triggered()
    {
        // Empty
    }

    #endregion
}