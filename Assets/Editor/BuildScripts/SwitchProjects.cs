using SecretHistories.Enums;
using SecretHistories.Services;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.BuildScripts
{
    public class SwitchProjects : ScriptableObject
    {
        [MenuItem("Tools/Switch/CS")]
        static void SwitchToCS()
        {
            PlayerSettings.bundleVersion = "2022.1.a.2";
            SetGameId(GameId.CS);
        }

        [MenuItem("Tools/Switch/BH")]
        static void SwitchToBH()
        {
            PlayerSettings.bundleVersion = "2022.1.a.1";
            SetGameId(GameId.BH);
        }

        private static void SetGameId(GameId gameIdAsEnum)
        {
            var glory = GameObject.Find("Glory").GetComponent<Glory>();
            glory.SetGameIdFieldInEditor(gameIdAsEnum);
     
        }
    }
}