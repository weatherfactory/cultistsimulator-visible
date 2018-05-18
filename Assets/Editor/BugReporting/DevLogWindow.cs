using System;
using Unity.Cloud.BugReporting;
using Unity.Cloud.BugReporting.Plugin;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.BugReporting
{
    public class DevLogWindow : EditorWindow
    {
        #region Static Methods

        [MenuItem("Dev Log/Dev Log Window")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLogWindow));
        }

        #endregion

        #region Constructors

        public DevLogWindow()
        {
            this.titleContent = new GUIContent("Dev Log");
            this.summary = string.Empty;
            this.description = string.Empty;
        }

        #endregion

        #region Fields

        private BugReport bugReport;

        private bool creating;

        private string description;

        private bool submitting;

        private string summary;

        private Texture thumbnailTexture;

        #endregion

        #region Methods

        private void Create()
        {
            this.creating = false;
            if (Application.isPlaying)
            {
                if (UnityBugReporting.CurrentClient != null)
                {
                    UnityBugReporting.CurrentClient.TakeScreenshot(2048, 2048, s => { });
                    UnityBugReporting.CurrentClient.TakeScreenshot(512, 512, s => { });
                    UnityBugReporting.CurrentClient.CreateBugReport((br) =>
                    {
                        this.SetThumbnail(br);
                        this.summary = string.Empty;
                        this.description = string.Empty;
                        this.bugReport = br;
                    });
                }
                else
                {
                    EditorUtility.DisplayDialog("Dev Log", "Bug Reporting is not configured. Bug Reporting is required for dev logs. Call UnityBugReporting.Configure() to configure Bug Reporting or add the BugReportingPrefab to your scene.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Dev Log", "Dev logs can only be sent in play mode.", "OK");
            }
        }

        private void CreatePropertyField(string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty stringsProperty = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(stringsProperty, true);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnGUI()
        {
            float spacing = 16;
            if (Application.isPlaying)
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.margin = new RectOffset(8, 8, 4, 4);
                buttonStyle.padding = new RectOffset(8, 8, 4, 4);
                GUIStyle textAreaStyle = new GUIStyle(GUI.skin.textArea);
                textAreaStyle.wordWrap = true;
                GUILayout.Space(spacing);
                this.creating = GUILayout.Button("New Dev Log", buttonStyle, GUILayout.ExpandWidth(false));
                if (this.bugReport != null)
                {
                    GUILayout.Space(spacing);
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    float imageHeight = Mathf.Min(this.position.width, 256);
                    EditorGUI.DrawPreviewTexture(new Rect(0, lastRect.yMax, this.position.width, imageHeight), this.thumbnailTexture, null, ScaleMode.ScaleToFit);
                    GUILayout.Space(imageHeight);
                    GUILayout.Space(spacing);
                    GUILayout.Label("Summary");
                    this.summary = EditorGUILayout.TextField(this.summary, textAreaStyle);
                    GUILayout.Label("Notes");
                    this.description = EditorGUILayout.TextArea(this.description, textAreaStyle, GUILayout.MinHeight(128));
                    GUILayout.Space(spacing);
                    EditorGUILayout.BeginHorizontal();
                    this.submitting = GUILayout.Button("Submit Dev Log", buttonStyle, GUILayout.ExpandWidth(false));
                    GUILayout.Space(spacing);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.UpperCenter;
                labelStyle.margin = new RectOffset(8, 8, 8, 8);
                labelStyle.wordWrap = true;
                GUILayout.Space(spacing);
                EditorGUILayout.HelpBox("Dev logs can only be sent in play mode.", MessageType.Info);
                GUILayout.Space(spacing);
            }
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }

        private void SetThumbnail(BugReport bugReport)
        {
            if (bugReport != null)
            {
                byte[] data = Convert.FromBase64String(bugReport.Thumbnail.DataBase64);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(data);
                this.thumbnailTexture = texture;
            }
        }

        private void Submit()
        {
            this.submitting = false;
            if (Application.isPlaying)
            {
                if (UnityBugReporting.CurrentClient != null)
                {
                    this.bugReport.Summary = this.summary;
                    this.bugReport.Fields.Add(new BugReportNamedValue("Notes", this.description));
                    this.bugReport.Dimensions.Add(new BugReportNamedValue("DevLog", "True"));
                    this.bugReport.IsHiddenWithoutDimension = true;

                    // Send
                    UnityBugReporting.CurrentClient.SendBugReport(this.bugReport, (success, br2) =>
                    {
                        this.bugReport = null;
                        if (EditorUtility.DisplayDialog("Dev Log", "Dev log submitted. Would you like to view it?", "View", "Don't View"))
                        {
                            string url = string.Format("https://developer.cloud.unity3d.com/bugreporting/direct/projects/{0}/reports/{1}", br2.ProjectIdentifier, br2.Identifier);
                            Application.OpenURL(url);
                        }
                    });
                }
                else
                {
                    EditorUtility.DisplayDialog("Dev Log", "Bug Reporting is not configured. Bug Reporting is required for dev logs. Call UnityBugReporting.Configure() to configure In-Game Bug Reporting or add the BugReportingPrefab to your scene.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Dev Log", "Dev logs can only be sent in play mode.", "OK");
            }
        }

        private void Update()
        {
            if (this.creating)
            {
                this.Create();
            }
            if (this.submitting)
            {
                this.Submit();
            }
        }

        #endregion
    }
}