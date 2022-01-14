using System.Collections.Generic;
using NUnit.Framework;
using SecretHistories.Enums;
using SecretHistories.Services;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Editor.BuildScripts
{
    public class SwitchProjects : ScriptableObject
    {
        private static List<string> CSSceneNames;
        private static List<string> BHSceneNames;


        static SwitchProjects()
        {
            //Scene management way more finicky than I expected! coming back to this
            CSSceneNames = new List<string>
            {
                "Assets/Scenes/CS/S3Menu.unity",
                "Assets/Scenes/CS/S4Tabletop.unity"
            };

             BHSceneNames = new List<string>
            {
                "Assets/Scenes/BH/S3MenuUmber.unity",
                "Assets/Scenes/BH/S4Library.unity"
            };

        }

        [MenuItem("Tools/Switch/CS")]
        static void SwitchToCS()
        {
            PlayerSettings.productName = "Cultist Simulator";
            SetGameId(GameId.CS);
            foreach (var scenePath in CSSceneNames)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.AdditiveWithoutLoading);
            }


        }

        [MenuItem("Tools/Switch/BH")]
        static void SwitchToBH()
        {
            PlayerSettings.productName = "Book of Hours";
            SetGameId(GameId.BH);
            foreach (var scenePath in BHSceneNames)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.AdditiveWithoutLoading);
            }
        }

        private static void SetGameId(GameId gameIdAsEnum)
        {
            var glory = GameObject.Find("Glory").GetComponent<Glory>();
            glory.SetGameIdFieldInEditor(gameIdAsEnum);
     
        }
    }
}