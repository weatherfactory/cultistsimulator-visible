using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class EndScreenController : MonoBehaviour {

        private static Sprite endImage;
        private static string endHeader;
        private static string endFlavor;

        public Image image;
        public TextMeshProUGUI header;
        public TextMeshProUGUI flavor;

        private void OnEnable() {
            if (endImage != null)
                image.sprite = endImage;

            if (endHeader != null)
                header.text = endHeader;

            if (endFlavor != null)
                flavor.text = endFlavor;
        }

        private void OnDisable() {
            endImage = null;
            endHeader = null;
            endFlavor = null;
        }

        // Trigger Method for convenience
        public static void LoadEndScreen(Sprite sprite, string header, string flavor) {
            endImage = sprite;
            endHeader = header;
            endFlavor = flavor;

            SceneManager.LoadScene(SceneNumber.EndScene);
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