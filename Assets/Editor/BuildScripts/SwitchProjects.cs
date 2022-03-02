using System.Collections.Generic;
using NUnit.Framework;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Services;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Editor.BuildScripts
{
    public class SwitchProjects : ScriptableObject
    {
        private static List<string> CSScenes;
        private static List<string> BHScenes;


        static SwitchProjects()
        {
            //Scene management way more finicky than I expected! coming back to this
            CSScenes = new List<string>
            {
           //     "Assets/Scenes/CS/S3Menu.unity",
                "Assets/Scenes/CS/S4Tabletop.unity"
            };

             BHScenes = new List<string>
            {
         //       "Assets/Scenes/BH/S3MenuUmber.unity",
                "Assets/Scenes/BH/S4Library.unity"
            };

        }

        [MenuItem("Tools/Switch/CS")]
        static void SwitchToCS()
        {
            PlayerSettings.productName = NoonConstants.CSPRODUCTNAME;
            OpenScenes(CSScenes);
            CloseScenes(BHScenes);

        }

        [MenuItem("Tools/Switch/BH")]
        static void SwitchToBH()
        {
            PlayerSettings.productName = NoonConstants.BHPRODUCTNAME;
            OpenScenes(BHScenes);
            CloseScenes(CSScenes);
        }



        private static void OpenScenes(List<string> scenePaths)
        {
            foreach (var scenePath in scenePaths)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
        }

        private static void CloseScenes(List<string> scenePaths)
        {
            foreach(var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.GetSceneByPath(scenePath);
                EditorSceneManager.CloseScene(scene, false);
            }
        }
    }
}