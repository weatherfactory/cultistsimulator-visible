using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEditor;

namespace SecretHistories.Editor.BuildScripts
{

    public enum OSId
    {
        Windows,
        OSX,
        Linux
    }
    public class BuildOS
    {
        private const string CONST_DATA_FOLDER_SUFFIX = "_Data";
        private const string CONST_STREAMING_ASSETS_FOLDER = "StreamingAssets";


        public OSId OSId { get; private set; }
        private BuildTarget buildTarget;
        public string ExeName { get; private set; }


        public BuildOS(GameId game, BuildTarget target)
        {
            buildTarget = target;

            if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                OSId = OSId.Windows;
                if (game == GameId.CS)
                    ExeName = "cultistsimulator.exe";
                else
                    ExeName = $"{game.ToString().ToLower()}.exe";
            }

            else if (target == BuildTarget.StandaloneOSX)
            {
                OSId = OSId.OSX;
                ExeName = "OSX.app";
            }
            else if (target == BuildTarget.StandaloneLinux64)
            {
                OSId = OSId.Linux;
                if (game == GameId.CS)
                    ExeName = "CS.x86_64";
                else
                    ExeName = $"{game.ToString().ToLower()}.x86_64";
            }
            else
                throw new ApplicationException("Unrecognised build target " + target);
        }
       

        public string GetRelativePath()
        {
            return OSId.ToString();
        }


        public string GetStreamingAssetsPath()
        {
            string streamingAssetsPath=NoonUtility.JoinPaths(GetDataFolderPath(), CONST_STREAMING_ASSETS_FOLDER);
            return streamingAssetsPath;

        }

        
        public string GetDataFolderPath()
        {
            string dataFolderPath;
            if (ExeName.Contains("OSX")) // OSX is cray
                dataFolderPath= "OSX.app/Contents/Resources/Data";
            else
                dataFolderPath= ExeName.Split('.')[0] + CONST_DATA_FOLDER_SUFFIX;
            return dataFolderPath;
        }

        public string GetCoreContentPath( string locale)
        {
            string coreContentPath;
            if (string.IsNullOrEmpty(locale))
                coreContentPath = NoonUtility.JoinPaths(
                    GetStreamingAssetsPath(),
                    Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY),
                    NoonConstants.CORE_FOLDER_NAME);
            else
                coreContentPath = NoonUtility.JoinPaths(
                    GetStreamingAssetsPath(),
                    Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY),
                    NoonConstants.LOC_FOLDER_TEMPLATE.Replace(NoonConstants.LOC_TOKEN, locale));
                
            return coreContentPath;
        }

     }


}
