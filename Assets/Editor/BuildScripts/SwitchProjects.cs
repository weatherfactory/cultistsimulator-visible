using System.Collections.Generic;
using NUnit.Framework;
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
                "Assets/Scenes/CS/S3Menu.unity",
                "Assets/Scenes/CS/S4Tabletop.unity"
            };

             BHScenes = new List<string>
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
            OpenScenes(CSScenes);
            CloseScenes(BHScenes);

        }

        [MenuItem("Tools/Switch/BH")]
        static void SwitchToBH()
        {
            PlayerSettings.productName = "Book of Hours";
            SetGameId(GameId.BH);
            OpenScenes(BHScenes);
            CloseScenes(CSScenes);
        }

        private static void SetGameId(GameId gameIdAsEnum)
        {
            var glory = GameObject.Find("Glory").GetComponent<Glory>();
            glory.SetGameIdFieldInEditor(gameIdAsEnum);
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