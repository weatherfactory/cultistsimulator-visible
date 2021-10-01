using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using Assets.Logic;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Modding;
using SecretHistories.Enums;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SecretHistories.UI {
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
        private List<Legacy> AvailableLegaciesForEnding;

        void Start() {
            var registry = new Watchman();


            var compendium = new Compendium();
            registry.Register<Compendium>(compendium);
            var contentImporter = new CompendiumLoader(Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY));
            contentImporter.PopulateCompendium(compendium, Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));

            var ls = new LegacySelector(Watchman.Get<Compendium>());
            AvailableLegaciesForEnding = ls.DetermineLegacies(Watchman.Get<Stable>().Protag().EndingTriggered);

            InitLegacyButtons();
            canvasFader.SetFinalAlpha(0f);

			FadeIn();
			SelectLegacy(0);
			canInteract = true;

    
        }


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

   
        void InitLegacyButtons()
        {


            for (int i = 0; i < AvailableLegaciesForEnding.Count; i++)
            {
                var legacySprite= ResourcesManager.GetSpriteForLegacy(AvailableLegaciesForEnding[i].Image);
                legacyArtwork[i].sprite = legacySprite;
            }

            // No button is selected, so start game button starts deactivated
            startGameButton.interactable = false;
        }


        
        // Exposed for in-scene buttons

        public  void ReturnToMenu() {
			if (!canInteract)
				return;


            Watchman.Get<StageHand>().MenuScreen();

            //save on exit, so the player will return here, not begin a new game
//            FadeOut();
	//		canInteract = false;
		//	Invoke("ReturnToMenuDelayed", fadeDuration);
        }

      //  async void  ReturnToMenuDelayed() { //unused?
			
      //throw new NotImplementedException("save here?");

      //      Watchman.Get<StageHand>().MenuScreen();
      //  }

		public void StartGame() {
			if (!canInteract)
				return;

            var chosenLegacy = AvailableLegaciesForEnding[selectedLegacy];

            var freshGamePersistenceProvider = new FreshGameProvider(chosenLegacy);

            Watchman.Get<StageHand>().LoadGameOnTabletop(freshGamePersistenceProvider);


            //         FadeOut();
            //canInteract = false;
            //SoundManager.PlaySfx("UIStartGame");
            //Invoke("StartGameDelayed", fadeDuration);
        }

		void StartGameDelayed()
        {
            var chosenLegacy = AvailableLegaciesForEnding[selectedLegacy];
            //Watchman.Get<Stable>().Protag().Reincarnate(chosenLegacy,NullEnding.Create());

            //Watchman.Get<StageHand>().NewGameOnTabletop();

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
            Legacy legacySelected = AvailableLegaciesForEnding[selectedLegacy];

            title.text = legacySelected.Label;
            description.text = legacySelected.Description;
            var ending = Watchman.Get<Stable>().Protag().EndingTriggered;
            if (legacySelected.FromEnding == ending.Id)
			{
                //availableBecause.text = "[Always available after " + ending.Title.ToUpper() + "]";
				availableBecause.text = Watchman.Get<ILocStringProvider>().Get("LEGACY_BECAUSE_PREFIX") + ending.Label.ToUpper() + Watchman.Get<ILocStringProvider>().Get("LEGACY_BECAUSE_POSTFIX");
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
