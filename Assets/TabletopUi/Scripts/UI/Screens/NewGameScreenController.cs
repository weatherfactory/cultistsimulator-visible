using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class NewGameScreenController : MonoBehaviour {


        public Toggle[] legacyButtons;
        public Image[] legacyArtwork;
        public RectTransform elementsHolder;
        int selectedLegacy = -1;

        [Header("Prefabs")]
        public ElementStackSimple elementStackSimplePrefab;
      

        [Header("Selected Legacy")]
        public CanvasGroupFader canvasFader;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public ElementStackSimple[] rewardTokens;

        [Header("Buttons")]
        public Button startGameButton;




        void Start() {
            var registry = new Registry();
            var compendium = new Compendium();
            registry.Register<ICompendium>(compendium);
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);

            InitLegacyButtons();
            canvasFader.SetAlpha(0f);
        }

        #if DEBUG
        // For Debug purposes
        void OnEnable() {
            InitLegacyButtons();
        }
        #endif
   


        void InitLegacyButtons() {
            for (int i = 0; i < CrossSceneState.GetAvailableLegacies().Count; i++)
            {
                var legacySprite= ResourcesManager.GetSpriteForLegacy(CrossSceneState.GetAvailableLegacies()[i].Image);
                legacyArtwork[i].sprite = legacySprite;
            }


            // No button is selected, so start game button starts deactivated
            startGameButton.interactable = false;
        }
        
        // Exposed for in-scene buttons


        public void ReturnToMenu() {
            //save on exit, so the player will return here, not begin a new game

            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
            saveGameManager.SaveInactiveGame();
            SceneManager.LoadScene(SceneNumber.MenuScene);
        }

        public void StartGame() {
            
            CrossSceneState.SetChosenLegacy(CrossSceneState.GetAvailableLegacies()[selectedLegacy]);
            CrossSceneState.ClearEndingAndLegacies();

            SceneManager.LoadScene(SceneNumber.GameScene);
        }


        public void SelectLegacy(int legacy) {
            if (legacy < 0)
                return;

            if (selectedLegacy == legacy)
                return;

            if (legacyButtons[legacy].isOn == false)
                return;

            StopAllCoroutines();
            StartCoroutine(DoShowLegacy(legacy));
        }

        void HideLegacyInfo() {
            canvasFader.Hide();
            selectedLegacy = -1;
            startGameButton.interactable = false;
        }

        IEnumerator DoShowLegacy(int legacy) {
            if (selectedLegacy >= 0) {
                canvasFader.Hide();
                yield return new WaitForSeconds(canvasFader.durationTurnOff);
            }

            selectedLegacy = legacy;
            UpdateSelectedLegacyInfo();
            canvasFader.Show();

            yield return new WaitForSeconds(canvasFader.durationTurnOn);
        }

        void UpdateSelectedLegacyInfo() {
           
            Legacy legacySelected = CrossSceneState.GetAvailableLegacies()[selectedLegacy];

            title.text = legacySelected.Label;
            description.text = legacySelected.Description;

            //display effects for legacy:
            //clear out any existing effect stacks
            var l = elementsHolder.GetComponentsInChildren<ElementStackSimple>();

            foreach (var effectStack in l)
            	Destroy(effectStack.gameObject);

            //and add effects for this legacy

            foreach (var e in legacySelected.Effects)
            {
                var effectStack = Object.Instantiate(elementStackSimplePrefab, elementsHolder, false);
                effectStack.Populate(e.Key, e.Value);

            }

            startGameButton.interactable = true;
        }
    }
}
