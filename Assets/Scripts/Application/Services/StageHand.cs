using System.Collections;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.UI;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SecretHistories.Services
{

    public class StageHand:MonoBehaviour
    {
        [Header("Fade Visuals")]
        public Image fadeOverlay;
        public float fadeDuration = 0.25f;

        //we don't want to load it more than once, and checking if it's in loaded scenes doesn't seem to work - because they're not loaded when we do the pass again?
        private bool loadedInfoScene = false;


        public int StartingSceneNumber;

        public GamePersistenceProvider GamePersistenceProvider { get; private set; }

        public void UseProvider(GamePersistenceProvider game)
        {
            GamePersistenceProvider = game;
        }

        async Task FadeOut()
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.canvasRenderer.SetAlpha(0f);
            fadeOverlay.CrossFadeAlpha(1, fadeDuration, true);
            await Task.Delay((int)(fadeDuration * 1000));

        }


        async Task FadeIn()
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.canvasRenderer.SetAlpha(1f);
            fadeOverlay.CrossFadeAlpha(0, fadeDuration, true);
            //fadeOverlay.gameObject.SetActive(false);
            await Task.Delay((int) (fadeDuration*1000));
        }


        private async void SceneChange(string sceneToLoad,bool withFadeEffect)
        {
            var sphereCatalogue = Watchman.Get<HornedAxe>();

            if(sphereCatalogue != null)
                Watchman.Get<HornedAxe>().Reset();

            var situationsCatalogue = Watchman.Get<HornedAxe>();
            if (situationsCatalogue != null)
                situationsCatalogue.Reset();


            if(SceneManager.sceneCount > 2)
                NoonUtility.Log("More than 2 scenes loaded",2);

            else if (SceneManager.sceneCount==2)
            {
                var sceneToUnload = SceneManager.GetSceneAt(1);
                AsyncOperation unloadOperation=SceneManager.UnloadSceneAsync(sceneToUnload);

            }

            if (withFadeEffect)
            {
                var fadeOutTask = FadeOut();
                await fadeOutTask;

                SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
                var fadeInTask=FadeIn();
                await fadeInTask;
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
            }

        }


        public void LoadInfoScene()
        {
            if(!loadedInfoScene)
            {
                SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().UhOScene, false);
                loadedInfoScene = true;
            }
        }

        public void LoadGameOnTabletop(GamePersistenceProvider source)
        {
            SoundManager.PlaySfx("UIStartGame");
            GamePersistenceProvider = source;
            SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().PlayfieldScene, true);
        }


        public void ClearRestartingGameFlag()
        {
            GamePersistenceProvider=new DefaultGamePersistenceProvider();
        }



        public void LogoScreen()
        {
            SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().LogoScene,false);
        }


        public void QuoteScreen()
        {
            SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().QuoteScene, false);
        }

        public void MenuScreen()
        {
            SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().MenuScene, false);
        }


        public bool SceneIsActive(string sceneToCheck)
        {
            return SceneManager.GetSceneByName(sceneToCheck).IsValid() &&
                   SceneManager.GetSceneByName(sceneToCheck).isLoaded;
        }


        public void EndingScreen()
        {
            SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().GameOverScene, false);
        }

        public void LegacyChoiceScreen()
        {
            SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().NewGameScene, true);
        }


        public void LoadFirstScene(bool skipLogo)
        {

            if (Application.isEditor)
            {
                if (StartingSceneNumber > 0)
                    SceneChange(StartingSceneNumber.ToString(),true);
              //  NewGameOnTabletop();
            }
            
            else
            {

                if (skipLogo)
                    SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().QuoteScene, false);
                else
                    SceneChange(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().LogoScene, false);
            }
        }
    }



}