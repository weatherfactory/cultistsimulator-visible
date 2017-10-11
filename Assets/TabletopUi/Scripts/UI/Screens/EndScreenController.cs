using System.Collections;
using System.Collections.Generic;
using Assets.TabletopUi.Scripts.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class EndScreenController : MonoBehaviour {

        public Image image;
        public TextMeshProUGUI header;
        public TextMeshProUGUI flavor;

        private void OnEnable() {
            var ending = CrossSceneState.GetCurrentEnding();

            if (ending == null)
                return;

            //image.sprite = ;
            header.text = ending.Title;
            flavor.text = ending.Description;
        }

        // Exposed for in-scene buttons
        public void ReturnToMenu() {
            SceneManager.LoadScene(SceneNumber.MenuScene);
        }

        public void StartNewGame() {
            SceneManager.LoadScene(SceneNumber.NewGameScene);
        }

    }
}