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

        public Image blackOverlay;

        bool hasSelected;

        const float durationFadeIn = 1f;
        const float durationFadeOut = 2f;

        private void OnEnable() {
            FadeIn(durationFadeIn);

            var ending = CrossSceneState.GetCurrentEnding();

            if (ending == null)
                return;

            header.text = ending.Title;
            flavor.text = ending.Description;
            image.sprite = ResourcesManager.GetSpriteForEnding(ending.ImageId);
        }

        void FadeIn(float duration) {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.canvasRenderer.SetAlpha(1f);
            blackOverlay.CrossFadeAlpha(0f, duration, false);
        }

        void FadeOut(float duration) {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.canvasRenderer.SetAlpha(0f);
            blackOverlay.CrossFadeAlpha(1f, duration, false);
        }

        public void ReturnToMenu() {
            if (hasSelected)
                return;

            hasSelected = true;
            // TODO: PLAY Button SFX
            FadeOut(durationFadeOut);
            Invoke("ReturnToMenuInternal", durationFadeOut);
        }

        private void ReturnToMenuInternal() {
            //save on exit, so the player will return here, not begin a new game
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
            saveGameManager.SaveInactiveGame();
            SceneManager.LoadScene(SceneNumber.MenuScene);
        }

        public void StartNewGame() {
            if (hasSelected)
                return;

            hasSelected = true;
            // TODO: PLAY Button SFX
            FadeOut(durationFadeOut);
            Invoke("StartNewGameInternal", durationFadeOut);
        }

        private void StartNewGameInternal() {
            SceneManager.LoadScene(SceneNumber.NewGameScene);
        }

    }
}