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
            image.sprite = ResourcesManager.GetSpriteForEnding(ending.ImageId);
        }

       
        public void ReturnToMenu() {
            //save on exit, so the player will return here, not begin a new game
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
            saveGameManager.SaveInactiveGame();
            SceneManager.LoadScene(SceneNumber.MenuScene);
        }

        public void StartNewGame() {
            SceneManager.LoadScene(SceneNumber.NewGameScene);
        }

    }
}