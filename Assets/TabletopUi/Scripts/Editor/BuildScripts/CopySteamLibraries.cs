using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Facepunch.Editor
{
    public class CopySteamLibraries
    {
        //this code credit FACEPUNCH thank you sweary Garry
        public static void Copy(BuildTarget target, string pathToBuiltProject)
        {
          Console.WriteLine("BUILD: copying Steam libraries to useful locations.");
            //
            // Only steam
            //
            if (!target.ToString().StartsWith("Standalone"))
                return;

            //
            // You only need a steam_appid.txt if you're launching outside of Steam, you don't need to ship with it
            // but most games do anyway.
            //
            FileUtil.ReplaceFile("steam_appid.txt", pathToBuiltProject + "/steam_appid.txt");

            //
            // Put these dlls next to the exe
            //
            if (target == BuildTarget.StandaloneWindows)
                FileUtil.ReplaceFile("steam_api.dll", pathToBuiltProject + "/steam_api.dll");

            if (target == BuildTarget.StandaloneWindows64)
                FileUtil.ReplaceFile("steam_api64.dll", pathToBuiltProject + "/steam_api64.dll");

            //Apple dylibs need to go deeper
            if (target == BuildTarget.StandaloneOSX)
            {
                Console.WriteLine("pathToBuiltProject in CopySteamLibraries: " + pathToBuiltProject);
                FileUtil.ReplaceFile("libsteam_api.dylib", pathToBuiltProject + "/OSX.app/Contents/Frameworks/MonoEmbedRuntime/osx/libsteam_api.dylib");
            }
            if (target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinuxUniversal)
                FileUtil.ReplaceFile("libsteam_api64.so",pathToBuiltProject + "/libsteam_api64.so");

            if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinuxUniversal)
                FileUtil.ReplaceFile("libsteam_api.so", pathToBuiltProject + "/libsteam_api.so");
        }

    }
}