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
        private static List<string> LGScenes;


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

             LGScenes = new List<string>
             {
                 //       "Assets/Scenes/BH/S3MenuUmber.unity",
                 "Assets/Scenes/LG/S4World.unity" //doesn't yet exist, prototyping in CS
             };

        }

        [MenuItem("Tools/Switch/CS")]
        static void SwitchToCS()
        {
            PlayerSettings.productName = NoonConstants.CSPRODUCTNAME;
            OpenScenes(CSScenes);
         //   CloseScenes(LGScenes);
            CloseScenes(BHScenes);

        }

        [MenuItem("Tools/Switch/BH")]
        static void SwitchToBH()
        {
            PlayerSettings.productName = NoonConstants.BHPRODUCTNAME;
            OpenScenes(BHScenes);
         //   CloseScenes(LGScenes);
            CloseScenes(CSScenes);
        }
        [MenuItem("Tools/Switch/LG")]
        static void SwitchToLG()
        {
            PlayerSettings.productName = NoonConstants.LGPRODUCTNAME;
            OpenScenes(CSScenes);
            //   OpenScenes(LGScenes);
            //CloseScenes(CSScenes);

            CloseScenes(BHScenes);
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