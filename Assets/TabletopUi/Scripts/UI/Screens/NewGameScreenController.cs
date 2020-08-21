using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Assets.TabletopUi.Scripts.Services;
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
        public TextMeshProUGUI availableBecause;

        [Header("Buttons")]
        public Button startGameButton;

		[Header("Fade Visuals")]
		public Image fadeOverlay;
		public float fadeDuration = 0.25f;

		bool canInteract;

        void Start() {
            var registry = new Registry();

            var modManager = new ModManager();
            modManager.CatalogueMods();
            registry.Register(modManager);

            var compendium = new Compendium();
            registry.Register<ICompendium>(compendium);
            var contentImporter = new CompendiumLoader();
            contentImporter.PopulateCompendium(compendium, Registry.Get<Concursum>().GetCurrentCultureId());

            InitLegacyButtons();
            canvasFader.SetAlpha(0f);

			FadeIn();
			SelectLegacy(0);
			canInteract = true;
        }

        #if DEBUG
        // For Debug purposes
        void OnEnable() {
            InitLegacyButtons();
        }
        #endif

		void FadeIn() {
			fadeOverlay.gameObject.SetActive(true);
			fadeOverlay.canvasRenderer.SetAlpha(1f);
			fadeOverlay.CrossFadeAlpha(0, fadeDuration, true);
		}

		void FadeOut() {
			fadeOverlay.gameObject.SetActive(true);
			fadeOverlay.canvasRenderer.SetAlpha(0f);
			fadeOverlay.CrossFadeAlpha(1, fadeDuration, true);
		}

   
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
			if (!canInteract)
				return;
			
            //save on exit, so the player will return here, not begin a new game
			FadeOut();
			canInteract = false;
			Invoke("ReturnToMenuDelayed", fadeDuration);
        }

		void ReturnToMenuDelayed() {
			var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Get<ICompendium>()), new GameDataExporter());
			saveGameManager.SaveInactiveGame(null);
			Registry.Get<StageHand>().SceneChange(SceneNumber.MenuScene);
        }

		public void StartGame() {
			if (!canInteract)
				return;
			
			FadeOut();
			canInteract = false;
			SoundManager.PlaySfx("UIStartgame");
			Invoke("StartGameDelayed", fadeDuration);
        }

		void StartGameDelayed() {
			CrossSceneState.SetChosenLegacy(CrossSceneState.GetAvailableLegacies()[selectedLegacy]);
			CrossSceneState.ClearEnding();
            Registry.Get<StageHand>().SceneChange(SceneNumber.TabletopScene);

		}

		public void SelectLegacy(int legacy) {
			if (!canInteract)
				return;
			
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
            var ending = CrossSceneState.GetCurrentEnding();
            if (legacySelected.FromEnding == ending.Id)
			{
                //availableBecause.text = "[Always available after " + ending.Title.ToUpper() + "]";
				availableBecause.text = Registry.Get<ILanguageManager>().Get("LEGACY_BECAUSE_PREFIX") + ending.Label.ToUpper() + Registry.Get<ILanguageManager>().Get("LEGACY_BECAUSE_POSTFIX");
			}
            else
			{
                availableBecause.text = "";
			}

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
