using System;
using Unity.Cloud.BugReporting;
using Unity.Cloud.BugReporting.Client;
using Unity.Cloud.BugReporting.Plugin;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BugReportingScript : MonoBehaviour
{
    #region Constructors

    public BugReportingScript()
    {
        this.BugReportSubmitting = new UnityEvent();
        this.unityBugReportingUpdater = new UnityBugReportingUpdater();
    }

    #endregion

    #region Fields

    public Button BugReportButton;

    public Canvas BugReportForm;

    public UnityEvent BugReportSubmitting;

    public InputField DescriptionInput;

    private bool isCreatingBugReport;

    public bool IsHotkeyEnabled;

    public bool IsInSilentMode;

    public bool IsSelfReporting;

    private bool isSubmitting;

    public bool SendEventsToAnalytics;

    public Canvas SubmittingPopup;

    public InputField SummaryInput;

    public Image ThumbnailViewer;

    private UnityBugReportingUpdater unityBugReportingUpdater;

    #endregion

    #region Properties

    public BugReport CurrentBugReport { get; private set; }

    public BugReportingState State
    {
        get
        {
            if (this.CurrentBugReport != null)
            {
                if (this.IsInSilentMode)
                {
                    return BugReportingState.Idle;
                }
                else if (this.isSubmitting)
                {
                    return BugReportingState.SubmittingForm;
                }
                else
                {
                    return BugReportingState.ShowingForm;
                }
            }
            else
            {
                if (this.isCreatingBugReport)
                {
                    return BugReportingState.CreatingBugReport;
                }
                else
                {
                    return BugReportingState.Idle;
                }
            }
        }
    }

    #endregion

    #region Methods

    public void CancelBugReport()
    {
        this.CurrentBugReport = null;
        this.ClearForm();
    }

    private void ClearForm()
    {
        this.SummaryInput.text = null;
        this.DescriptionInput.text = null;
    }

    public void CreateBugReport()
    {
        if (this.isCreatingBugReport)
        {
            return;
        }
        this.isCreatingBugReport = true;
        UnityBugReporting.CurrentClient.TakeScreenshot(2048, 2048, s => { });
        UnityBugReporting.CurrentClient.TakeScreenshot(512, 512, s => { });
        UnityBugReporting.CurrentClient.CreateBugReport((br) =>
        {
            if (string.IsNullOrEmpty(br.ProjectIdentifier))
            {
                Debug.LogWarning("The bug report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityBugReporting.Configure().");
            }
            br.Attachments.Add(new BugReportAttachment("Sample Attachment.txt", "SampleAttachment.txt", "text/plain", System.Text.Encoding.UTF8.GetBytes("This is a sample attachment.")));
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
            br.Dimensions.Add(new BugReportNamedValue("Platform.Version", string.Format("{0}.{1}", platform, version)));
            this.CurrentBugReport = br;
            this.isCreatingBugReport = false;
            this.SetThumbnail(br);
            if (this.IsInSilentMode)
            {
                this.SubmitBugReport();
            }
        });
    }

    public bool IsSubmitting()
    {
        return this.isSubmitting;
    }

    private void SetThumbnail(BugReport bugReport)
    {
        if (bugReport != null && this.ThumbnailViewer != null)
        {
            byte[] data = Convert.FromBase64String(bugReport.Thumbnail.DataBase64);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(data);
            this.ThumbnailViewer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5F, 0.5F));
            this.ThumbnailViewer.preserveAspect = true;
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            EventSystem sceneEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (sceneEventSystem == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }
        if (UnityBugReporting.CurrentClient == null)
        {
            UnityBugReporting.Configure();
        }
    }

    public void SubmitBugReport()
    {
        if (this.isSubmitting || this.CurrentBugReport == null)
        {
            return;
        }
        this.isSubmitting = true;
        if (this.SummaryInput != null)
        {
            this.CurrentBugReport.Summary = this.SummaryInput.text;
        }
        if (this.DescriptionInput != null)
        {
            BugReportNamedValue bugReportField = new BugReportNamedValue();
            bugReportField.Name = "Description";
            bugReportField.Value = this.DescriptionInput.text;
            this.CurrentBugReport.Fields.Add(bugReportField);
        }
        this.ClearForm();
        this.RaiseBugReportSubmitting();
        UnityBugReporting.CurrentClient.SendBugReport(this.CurrentBugReport, (success, br2) =>
        {
            this.CurrentBugReport = null;
            this.isSubmitting = false;
        });
    }

    private void Update()
    {
        if (this.IsHotkeyEnabled)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.B))
                {
                    this.CreateBugReport();
                }
            }
        }
        UnityBugReporting.CurrentClient.IsSelfReporting = this.IsSelfReporting;
        UnityBugReporting.CurrentClient.SendEventsToAnalytics = this.SendEventsToAnalytics;
        if (this.BugReportButton != null)
        {
            this.BugReportButton.interactable = this.State == BugReportingState.Idle;
        }
        if (this.BugReportForm != null)
        {
            this.BugReportForm.enabled = this.State == BugReportingState.ShowingForm;
        }
        if (this.SubmittingPopup != null)
        {
            this.SubmittingPopup.enabled = this.State == BugReportingState.SubmittingForm;
        }
        this.unityBugReportingUpdater.Reset();
        this.StartCoroutine(this.unityBugReportingUpdater);
    }

    #endregion

    #region Virtual Methods

    protected virtual void RaiseBugReportSubmitting()
    {
        if (this.BugReportSubmitting != null)
        {
            this.BugReportSubmitting.Invoke();
        }
    }

    #endregion
}