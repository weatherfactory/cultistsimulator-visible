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
            PlayerSettings.productName = "Cultist Simulator";
            SetGameId(GameId.CS);
        }

        [MenuItem("Tools/Switch/BH")]
        static void SwitchToBH()
        {
            PlayerSettings.productName = "Book of Hours";
            SetGameId(GameId.BH);
        }

        private static void SetGameId(GameId gameIdAsEnum)
        {
            var glory = GameObject.Find("Glory").GetComponent<Glory>();
            glory.SetGameIdFieldInEditor(gameIdAsEnum);
     
        }
    }
}