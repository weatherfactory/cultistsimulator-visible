using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noon;
using UnityEditor;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
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
        private const string CONST_CORE_CONTENT_LOCATION = "content/core";

        public OSId OSId { get; private set; }
        private BuildTarget buildTarget;
        public string ExeName { get; private set; }



        public BuildOS(BuildTarget target)
        {

            buildTarget = target;

       if(target==BuildTarget.StandaloneWindows || target==BuildTarget.StandaloneWindows64)
       {
           OSId = OSId.Windows;
           ExeName = "cultistsimulator.exe";
       }
            else if (target == BuildTarget.StandaloneOSX)
       {
           OSId = OSId.OSX;
           ExeName =  "OSX.app";
       }
       else if (target == BuildTarget.StandaloneLinux64)
       {
           OSId = OSId.Linux;
           ExeName = "CS.x86_64";

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

            string coreContentPath=NoonUtility.JoinPaths(
                GetStreamingAssetsPath(),
                CONST_CORE_CONTENT_LOCATION + (locale != null ? "_" + locale : ""));
            return coreContentPath;
        }

     }


}
