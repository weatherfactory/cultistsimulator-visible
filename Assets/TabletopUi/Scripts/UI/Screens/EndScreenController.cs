using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class EndScreenController : MonoBehaviour
	{
		// Convenient way to list all the endings for the debug menu
		enum eEndings
		{
			workvictory,
			deathofthebody,

			maxEndings
		};

        public Image image;
        public TextMeshProUGUI header;
        public TextMeshProUGUI flavor;

        public Image blackOverlay;

        bool hasSelected;

        const float durationFadeIn = 1f;
        const float durationFadeOut = 2f;
#pragma warning disable 649
        [SerializeField] private AudioSource audioSource;
#pragma warning restore 649
        protected AudioClip endingMusic;
        

        private void OnEnable()
		{
            FadeIn(durationFadeIn);

            var ending = CrossSceneState.GetCurrentEnding();

            if (ending == null)
                ending = Ending.DefaultEnding();
			
			InitEnding( ending );
		}

		private void InitEnding( Ending ending )
		{
			PlayEndingMusic(ending);

            header.text = ending.Label;
            flavor.text = ending.Description;
            image.sprite = ResourcesManager.GetSpriteForEnding(ending.Image);
        }

        private void PlayEndingMusic(Ending ending)
        {
            endingMusic = ResourcesManager.GetEndingMusic(ending.EndingFlavour).First();
            audioSource.Stop();
            audioSource.PlayOneShot(endingMusic);
        }

        void FadeIn(float duration)
		{
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.canvasRenderer.SetAlpha(1f);
            blackOverlay.CrossFadeAlpha(0f, duration, false);
        }

        void FadeOut(float duration)
		{
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.canvasRenderer.SetAlpha(0f);
            blackOverlay.CrossFadeAlpha(1f, duration, false);
        }

        public void ReturnToMenu()
		{
            if (hasSelected)
                return;

            hasSelected = true;
            // TODO: PLAY Button SFX
            FadeOut(durationFadeOut);
            Invoke("ReturnToMenuInternal", durationFadeOut);
        }

        private void ReturnToMenuInternal()
		{
            //save on exit, so the player will return here, not begin a new game
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
            saveGameManager.SaveInactiveGame(null);
            SceneManager.LoadScene(SceneNumber.MenuScene);
        }

        public void StartNewGame()
		{
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
#if UNITY_EDITOR
		private void OnGUI()
		{
			Rect buttonRect = new Rect(5,5,200,20);
			var compendium = Registry.Retrieve<ICompendium>();
			List<Ending> endings = compendium.GetAllEndings();

			foreach (Ending ending in endings)
			{
				if (GUI.Button(buttonRect, ending.Id))
				{
					InitEnding( ending );
				}
				buttonRect.y += 20.0f;
				if (buttonRect.y > Screen.height-20)
				{
					buttonRect.y = 5;
					buttonRect.x = Screen.width-205;
				}
			}
		}
#endif
	}
}