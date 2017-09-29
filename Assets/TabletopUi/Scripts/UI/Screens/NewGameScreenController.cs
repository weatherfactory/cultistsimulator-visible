using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class NewGameScreenController : MonoBehaviour {

        // Remove and replace with proper legacy class

        [System.Serializable]
        public class Legacy {
            public Sprite artwork;
            public string title;
            public string description;
            public ElementDefinition[] elements;

            [System.Serializable]
            public class ElementDefinition {
                public string elementName;
                public int elementCount = 1;
            }
        }

        public Toggle[] legacyButtons;
        public Image[] legacyArtwork;

        [Header("Selected Legacy")]
        public CanvasGroupFader canvasFader;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public ElementStackSimple[] rewardTokens;

        [Header("Buttons")]
        public Button startGameButton;

        // public for test purposes to add dummy data
        [Header("Data")]
        public Legacy[] legacies;
        int selectedLegacy = -1;

        void Start() {
            InitData();
            SetLegacyButtons();
            canvasFader.SetAlpha(0f);
        }

#if DEBUG
        // For Debug purposes
        void OnEnable() {
            SetLegacyButtons();
        }
#endif

        // Taken directly from TabletopManager - this is copy & paste and pretty crap. 
        // Can we Init these things elsewhere to make sure they're available here?        
        void InitData() {
            var registry = new Registry();
            var compendium = new Compendium();
            registry.Register<ICompendium>(compendium);
            UpdateCompendium(compendium);
        }

        public void UpdateCompendium(ICompendium compendium) {
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);

            foreach (var p in contentImporter.GetContentImportProblems())
                Debug.Log(p.Description);
        }

        void SetLegacyButtons() {
            for (int i = 0; i < legacyArtwork.Length; i++) {
                legacyArtwork[i].sprite = legacies[i].artwork;
            }

            // No button is selected, so start game button starts deactivated
            startGameButton.interactable = false;
        }
        
        // Exposed for in-scene buttons


        public void ReturnToMenu() {
            SceneManager.LoadScene(SceneNumber.MenuScene);
        }

        public void StartGame() {
            // TODO: Somehow save selected legacy here so that game scene can use it to set up the board

            SceneManager.LoadScene(SceneNumber.GameScene);
        }


        public void SelectLegacy(int legacy) {
            if (legacy < 0 || legacy >= legacies.Length || legacies[legacy] == null)
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
            Legacy legacy = legacies[selectedLegacy];

            title.text = legacy.title;
            description.text = legacy.description;

            for (int i = 0; i < rewardTokens.Length; i++) {
                if (i >= legacy.elements.Length || legacy.elements[i] == null) {
                    rewardTokens[i].gameObject.SetActive(false);
                }
                else {
                    rewardTokens[i].Populate(legacy.elements[i].elementName, legacy.elements[i].elementCount);
                    rewardTokens[i].gameObject.SetActive(true);
                }
            }

            startGameButton.interactable = true;
        }
    }
}
