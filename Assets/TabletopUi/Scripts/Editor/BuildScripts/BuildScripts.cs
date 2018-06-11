// Doesn't derive from Editor, so let's make sure it and it's DLLs are not included in any build
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;
using System.IO;
//using Ionic.Zip;

public class BuildScripts : EditorWindow
{
    ////ruthlessly adapted from Martin Nerurkar's original work.
    //public static bool clearAtlasCacheBeforeBuild;
    //public static string gameName = "CultistSimulator";

    //public static string[] scenes = new string[] {
    //        "Assets/Logo.unity",
    //        "Assets/Quote.unity",
    //        "Assets/Menu.unity",
    //        "Assets/Tabletop.unity",
    //        "Assets/GameOver.unity",
    //        "Assets/NewGame.unity"
    //    };
    //[MenuItem("Cultist Simulator/Build/Windows", priority = 50)]
    //void DoWindowBuild()
//    {
//        if (!buildOSX && !buildLINUX && !buildWIN) return;

    //        if (!deleteFiles && buildWIN && buildLINUX)
    //        {
    //            UnityEngine.Debug.LogWarning("Can Not build WIN & Linux without deleting - same DATA folder!");
    //            return;
    //        }

    //        if (uploadToItch && !uploadToBetaChannel)
    //        {
    //            if (!EditorUtility.DisplayDialog("Upload to RELEASE channel", "Are you sure you want to upload to the release channel?", "Build Release"))
    //                return;
    //        }

    //        string path;

    //        if (string.IsNullOrEmpty(buildPath))
    //            path = GetTargetPath();
    //        else
    //            path = buildPath;

    //        if (!IsPathLegal(path)) return;

    //        WriteVersionFile(path);

    //        if (buildWIN)
    //            BuildGameWIN(path, createZIP, deleteFiles, uploadToItch, uploadToBetaChannel ? butlerArgsWinBeta : butlerArgsWin);

    //        if (buildLINUX)
    //            BuildGameLINUX(path, createZIP, deleteFiles, uploadToItch, uploadToBetaChannel ? butlerArgsLinuxBeta : butlerArgsLinux);

    //        if (buildOSX)
    //            BuildGameOSX(path, createZIP, deleteFiles, uploadToItch, uploadToBetaChannel ? butlerArgsMacBeta : butlerArgsMac);

    //        if (incrementVersion)
    //            VersionIncrementor.IncreaseBuildNumber();
    //    }

    //[MenuItem("Cultist Simulator/Build/Show", priority = 50)]
    //static void ShowWindow()
    //{
    //    EditorWindow.GetWindow<BuildScripts>("Build Window");
    //    EditorWindow.FocusWindowIfItsOpen<BuildScripts>();
    //}



}


//public class BuildScripts : EditorWindow
//{
//    public static bool clearAtlasCacheBeforeBuild;
//    public static string gameName = "NowhereProphet";
//    public static string[] levels = new string[] {
//        "Assets/startup_intro.unity",
//        "Assets/MainMenu.unity",
//        "Assets/Game.unity",
//        "Assets/EndScreen.unity",
//        "Assets/LoadScreen.unity",
//        "Assets/ErrorScreen.unity",
//        "Assets/IntroWarning.unity",
//        "Assets/CreditsScreen.unity"
//    };

//    private const string butlerLocation = "C:/Users/Democritus/AppData/Roaming/itch/bin/Butler.exe";

//    private const string butlerArgsWin = "sharkbombs/nowhere-prophet:win";
//    private const string butlerArgsMac = "sharkbombs/nowhere-prophet:osx";
//    private const string butlerArgsLinux = "sharkbombs/nowhere-prophet:linux";

//    private const string butlerArgsWinBeta = "sharkbombs/nowhere-prophet-beta:win-beta";
//    private const string butlerArgsMacBeta = "sharkbombs/nowhere-prophet-beta:osx-beta";
//    private const string butlerArgsLinuxBeta = "sharkbombs/nowhere-prophet-beta:linux-beta";

//    private const string versionFileName = "version.txt";

//    [MenuItem("Nowhere Prophet/Build/Open Build Window", priority = 50)]
//    static void ShowWindow()
//    {
//        EditorWindow.GetWindow<BuildScripts>("Build Window");
//        EditorWindow.FocusWindowIfItsOpen<BuildScripts>();
//    }

//    static bool devBuild = false;

//    bool buildWIN = true;
//    bool buildLINUX = false;
//    bool buildOSX = false;

//    string buildPath = "";

//    bool createZIP = true;
//    bool deleteFiles = true;

//    bool uploadToItch = false;
//    bool uploadToBetaChannel = true;

//    bool incrementVersion = false;

//    protected virtual void OnGUI()
//    {
//        var defaultColor = GUI.color;

//        EditorGUILayout.LabelField("Platforms", EditorStyles.boldLabel);
//        {
//            buildWIN = EditorGUILayout.Toggle("Windows", buildWIN);
//            buildLINUX = EditorGUILayout.Toggle("Linux", buildLINUX);
//            buildOSX = EditorGUILayout.Toggle("OS X", buildOSX);
//        }

//        EditorGUILayout.LabelField("Development Build", EditorStyles.boldLabel);

//        if (devBuild)
//            GUI.color = Color.red;

//        devBuild = EditorGUILayout.Toggle("Enabled", devBuild);

//        if (devBuild)
//            GUI.color = defaultColor;

//        EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);
//        {
//            EditorGUILayout.BeginHorizontal();
//            {
//                GUI.enabled = false;
//                EditorGUILayout.TextField("Build Path", buildPath);
//                GUI.enabled = true;

//                if (GUILayout.Button("Browse", GUILayout.Width(60f)))
//                {
//                    string path = GetTargetPath();

//                    if (IsPathLegal(path))
//                        buildPath = path;
//                }
//            }
//            EditorGUILayout.EndHorizontal();
//        }


//        EditorGUILayout.LabelField("Auto Zip File", EditorStyles.boldLabel);
//        {
//            createZIP = EditorGUILayout.Toggle("Auto-Create Zip", createZIP);
//            GUI.enabled = createZIP;
//            deleteFiles = EditorGUILayout.Toggle("Delete Build Files", deleteFiles);
//            GUI.enabled = true;
//        }

//#if !UNITY_EDITOR_OSX
//        EditorGUILayout.LabelField("Itch Auto Upload", EditorStyles.boldLabel);
//        {
//            GUI.enabled = createZIP;
//            uploadToItch = EditorGUILayout.Toggle("Enabled", uploadToItch);
//            GUI.enabled = createZIP && uploadToItch;

//            if (!uploadToBetaChannel)
//                GUI.color = Color.red;

//            uploadToBetaChannel = EditorGUILayout.Toggle("Beta Channel", uploadToBetaChannel);

//            if (!uploadToBetaChannel)
//                GUI.color = defaultColor;

//            GUI.enabled = true;
//        }
//#endif

//        EditorGUILayout.LabelField("Version", EditorStyles.boldLabel);
//        {
//            GUI.enabled = false;
//            EditorGUILayout.TextField("Current", VersionIncrementor.GetBuildNumber().ToString());
//#if UNITY_EDITOR_OSX
//			GUI.enabled = true;
//#else
//            GUI.enabled = uploadToItch;
//#endif
//            incrementVersion = EditorGUILayout.Toggle("Increment Version", incrementVersion);
//            GUI.enabled = true;
//        }

//        GUILayout.Space(10f);

//        if (GUILayout.Button("Build"))
//        {
//            DoWindowBuild();
//        }

//        EditorGUILayout.LabelField("Push Beta", EditorStyles.boldLabel);
//        EditorGUILayout.BeginHorizontal();
//        {
//            if (GUILayout.Button("WIN"))
//                UploadWinToItch(true);
//            if (GUILayout.Button("LINUX"))
//                UploadLinuxToItch(true);
//            if (GUILayout.Button("OS X"))
//                UploadMacToItch(true);
//        }
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.LabelField("Push Final", EditorStyles.boldLabel);
//        EditorGUILayout.BeginHorizontal();
//        {
//            if (GUILayout.Button("WIN"))
//                UploadWinToItch(false);
//            if (GUILayout.Button("LINUX"))
//                UploadLinuxToItch(false);
//            if (GUILayout.Button("OS X"))
//                UploadMacToItch(false);
//        }
//        EditorGUILayout.EndHorizontal();

//        if (GUILayout.Button("Increment Version"))
//            VersionIncrementor.IncreaseBuildNumber();
//    }

//    void DoWindowBuild()
//    {
//        if (!buildOSX && !buildLINUX && !buildWIN) return;

//        if (!deleteFiles && buildWIN && buildLINUX)
//        {
//            UnityEngine.Debug.LogWarning("Can Not build WIN & Linux without deleting - same DATA folder!");
//            return;
//        }

//        if (uploadToItch && !uploadToBetaChannel)
//        {
//            if (!EditorUtility.DisplayDialog("Upload to RELEASE channel", "Are you sure you want to upload to the release channel?", "Build Release"))
//                return;
//        }

//        string path;

//        if (string.IsNullOrEmpty(buildPath))
//            path = GetTargetPath();
//        else
//            path = buildPath;

//        if (!IsPathLegal(path)) return;

//        WriteVersionFile(path);

//        if (buildWIN)
//            BuildGameWIN(path, createZIP, deleteFiles, uploadToItch, uploadToBetaChannel ? butlerArgsWinBeta : butlerArgsWin);

//        if (buildLINUX)
//            BuildGameLINUX(path, createZIP, deleteFiles, uploadToItch, uploadToBetaChannel ? butlerArgsLinuxBeta : butlerArgsLinux);

//        if (buildOSX)
//            BuildGameOSX(path, createZIP, deleteFiles, uploadToItch, uploadToBetaChannel ? butlerArgsMacBeta : butlerArgsMac);

//        if (incrementVersion)
//            VersionIncrementor.IncreaseBuildNumber();
//    }

//    [MenuItem("Nowhere Prophet/Build/Build All + Increment", priority = 60)]
//    public static void BuildGameAll()
//    {
//        BuildGameAll(false);
//    }

//    [MenuItem("Nowhere Prophet/Build/Build All + Increment + Push", priority = 60)]
//    public static void BuildGameAllWithPush()
//    {
//        BuildGameAll(true);
//    }

//    static void BuildGameAll(bool push)
//    {
//        string path = GetTargetPath();

//        if (!IsPathLegal(path)) return;

//        WriteVersionFile(path);

//        BuildGameWIN(path, true, true, push, butlerArgsWinBeta);
//        BuildGameLINUX(path, true, true, push, butlerArgsLinuxBeta);
//        BuildGameOSX(path, true, true, push, butlerArgsMacBeta);

//        VersionIncrementor.IncreaseBuildNumber();
//    }

//    public static string GetTargetPath()
//    {
//        return EditorUtility.SaveFolderPanel("Choose File Desination", "", "") + Path.DirectorySeparatorChar;
//    }

//    static bool IsPathLegal(string path)
//    {
//        if (path == "/")
//            return false;
//        if (path == "\\")
//            return false;

//        return true;
//    }

//    [MenuItem("Nowhere Prophet/Build/Build WIN", priority = 70)]
//    public static void BuildGameWIN()
//    {
//        BuildGameWIN(GetTargetPath(), true, true, false);
//    }

//    public static void BuildGameWIN(string path, bool zip, bool delete, bool push, string butlerArgs = null)
//    {
//        if (!IsPathLegal(path)) return;

//        string executable = gameName + ".exe";
//        string dataFolder = gameName + "_Data";
//        string zipArchive = gameName + "WIN.zip";

//        DeleteAtlasCache();
//        BuildPipeline.BuildPlayer(levels, path + executable, BuildTarget.StandaloneWindows, GetBuildOptions());

//        if (!zip)
//            return;

//        string[] files = new string[] { executable };
//        string[] directories = new string[] { dataFolder };

//        string zipPath = ZipFiles(path, zipArchive, files, directories);

//        if (delete)
//            DeleteFiles(path, files, directories);

//        if (push && butlerArgs != null)
//            UploadToItch(zipPath, butlerArgs);
//    }

//    private static BuildOptions GetBuildOptions()
//    {
//        if (devBuild)
//            return BuildOptions.Development;
//        else
//            return BuildOptions.None;
//    }

//    [MenuItem("Nowhere Prophet/Build/Build LINUX", priority = 70)]
//    public static void BuildGameLINUX()
//    {
//        BuildGameLINUX(GetTargetPath(), true, true, false);
//    }

//    public static void BuildGameLINUX(string path, bool zip, bool delete, bool push, string butlerArgs = null)
//    {
//        if (!IsPathLegal(path)) return;

//        string executable = gameName + ".x86";
//        string dataFolder = gameName + "_Data";
//        string zipArchive = gameName + "LINUX.zip";

//        DeleteAtlasCache();
//        BuildPipeline.BuildPlayer(levels, path + executable, BuildTarget.StandaloneLinuxUniversal, GetBuildOptions());

//        if (!zip)
//            return;

//        string[] files = new string[] { executable, executable + "_64" };
//        string[] directories = new string[] { dataFolder };

//        string zipPath = ZipFiles(path, zipArchive, files, directories);

//        if (delete)
//            DeleteFiles(path, files, directories);

//        if (push && butlerArgs != null)
//            UploadToItch(zipPath, butlerArgs);
//    }

//    [MenuItem("Nowhere Prophet/Build/Build OSX", priority = 70)]
//    public static void BuildGameOSX()
//    {
//        BuildGameOSX(GetTargetPath(), true, true, false);
//    }

//    public static void BuildGameOSX(string path, bool zip, bool delete, bool push, string butlerArgs = null)
//    {
//        if (!IsPathLegal(path)) return;

//        string appFolder = gameName + ".app";
//        string zipArchive = gameName + "OSX.zip";

//        DeleteAtlasCache();
//        BuildPipeline.BuildPlayer(levels, path + appFolder, BuildTarget.StandaloneOSXUniversal, GetBuildOptions());

//        if (!zip)
//            return;

//        string[] files = new string[0];
//        string[] directories = new string[] { appFolder };

//        string zipPath = ZipFiles(path, zipArchive, files, directories);

//        if (delete)
//            DeleteFiles(path, files, directories);

//        if (push && butlerArgs != null)
//            UploadToItch(zipPath, butlerArgs);
//    }

//    // ---------------------------

//    public static string ZipFiles(string path, string zipFileName, string[] files, string[] directories)
//    {
//        Directory.CreateDirectory(path);

//        using (ZipFile zip = new ZipFile())
//        {
//            foreach (string file in files)
//            {
//                zip.AddFile(path + file, "");
//            }
//            foreach (string directory in directories)
//            {
//                zip.AddDirectory(path + directory, directory);
//            }
//            zip.Save(path + zipFileName);
//        }

//        return path + zipFileName;
//    }

//    public static void DeleteFiles(string path, string[] files, string[] directories)
//    {
//        if (files != null)
//            foreach (string f in files)
//                FileUtil.DeleteFileOrDirectory(path + f);

//        if (directories != null)
//            foreach (string d in directories)
//                FileUtil.DeleteFileOrDirectory(path + d);
//    }

//    private static void DeleteAtlasCache()
//    {
//        if (!clearAtlasCacheBeforeBuild)
//            return;

//        string projectPath = Application.dataPath; //Asset path
//        string atlasCachePath = Path.GetFullPath(Path.Combine(projectPath, Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Library" + Path.DirectorySeparatorChar + "AtlasCache"));
//        Directory.Delete(atlasCachePath, true);
//        UnityEngine.Debug.Log("Deleted atlas cache folder.");
//    }

//    private static void WriteVersionFile(string path)
//    {
//        FileUtil.ReplaceFile(VersionIncrementor.versionFilePath, path + versionFileName);
//    }


//    [MenuItem("Nowhere Prophet/Build/Push Win To Itch", priority = 80)]
//    public static void UploadWinToItch(bool beta = true)
//    {
//        string windowName = "Choose zipped Windows Build" + (!beta ? " FINAL" : "");
//        string path = EditorUtility.OpenFilePanel(windowName, "", "zip");
//        UploadToItch(path, beta ? butlerArgsWinBeta : butlerArgsWin);
//    }

//    [MenuItem("Nowhere Prophet/Build/Push Linux To Itch", priority = 80)]
//    public static void UploadLinuxToItch(bool beta = true)
//    {
//        string windowName = "Choose zipped Linux Build" + (!beta ? " FINAL" : "");
//        string path = EditorUtility.OpenFilePanel(windowName, "", "zip");
//        UploadToItch(path, beta ? butlerArgsLinuxBeta : butlerArgsLinux);
//    }

//    [MenuItem("Nowhere Prophet/Build/Push Mac To Itch", priority = 80)]
//    public static void UploadMacToItch(bool beta = true)
//    {
//        string windowName = "Choose zipped Mac OSX Build" + (!beta ? " FINAL" : "");
//        string path = EditorUtility.OpenFilePanel(windowName, "", "zip");
//        UploadToItch(path, beta ? butlerArgsMacBeta : butlerArgsMac);
//    }

//    private static void UploadToItch(string path, string args)
//    {
//        if (System.String.IsNullOrEmpty(path))
//            return;

//        var versionPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + versionFileName;

//        if (File.Exists(versionPath))
//        {
//            var versionText = System.IO.File.ReadAllText(versionPath);
//            versionText = versionText.Trim();
//            versionText = versionText.Trim(System.Environment.NewLine.ToCharArray());

//            if (versionText != null)
//                args += " --userversion-file \"" + versionPath + "\"";
//            else
//                UnityEngine.Debug.LogWarning("Upload to Itch: Version File found but nothing within it");
//        }
//        else
//        {
//            UnityEngine.Debug.LogWarning("Upload to Itch: No Version File found!");
//        }

//        ProcessStartInfo startInfo = new ProcessStartInfo();
//        startInfo.CreateNoWindow = false;
//        startInfo.UseShellExecute = false;
//        startInfo.FileName = butlerLocation;
//        startInfo.WindowStyle = ProcessWindowStyle.Normal;
//        startInfo.RedirectStandardOutput = false;
//        startInfo.RedirectStandardError = true;
//        startInfo.Arguments = "push \"" + path + "\" " + args;

//        UnityEngine.Debug.Log("Starting " + startInfo.FileName + " " + startInfo.Arguments);

//        var process = Process.Start(startInfo);

//        if (startInfo.RedirectStandardOutput)
//        {
//            string output = process.StandardOutput.ReadToEnd();
//            if (!System.String.IsNullOrEmpty(output))
//                UnityEngine.Debug.Log(output);
//        }

//        if (startInfo.RedirectStandardError)
//        {
//            string err = process.StandardError.ReadToEnd();
//            if (!System.String.IsNullOrEmpty(err))
//                UnityEngine.Debug.LogError(err);
//        }

//        process.WaitForExit();
//    }
//}