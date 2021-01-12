﻿using System.Collections;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
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

        public SourceForGameState SourceForGameState { get; private set; }

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
            var sphereCatalogue = Registry.Get<SphereCatalogue>();

            if(sphereCatalogue != null)
                Registry.Get<SphereCatalogue>().Reset();

            var situationsCatalogue = Registry.Get<SituationsCatalogue>();
            if (situationsCatalogue != null)
                situationsCatalogue.Reset();


            if(SceneManager.sceneCount > 2)
                NoonUtility.Log("More than 2 scenes loaded",2);

            else if (SceneManager.sceneCount==2)
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));


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
                SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().UhOScene, false);
                loadedInfoScene = true;
            }
        }

        public void LoadGameOnTabletop(SourceForGameState source)
        {
            SoundManager.PlaySfx("UIStartGame");
            SourceForGameState = source;
            SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().PlayfieldScene, true);
        }


        public void NewGameOnTabletop()
        {
            SourceForGameState = SourceForGameState.NewGame;
            SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().NewGameScene, true);
        }

        public void ClearRestartingGameFlag()
        {
            SourceForGameState = SourceForGameState.DefaultSave;
        }



        public void LogoScreen()
        {
            SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().LogoScene,false);
        }


        public void QuoteScreen()
        {
            SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().QuoteScene, false);
        }

        public void MenuScreen()
        {
            SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().MenuScene, false);
        }


        public bool SceneIsActive(string sceneToCheck)
        {
            return SceneManager.GetSceneByName(sceneToCheck).IsValid() &&
                   SceneManager.GetSceneByName(sceneToCheck).isLoaded;
        }


        public void EndingScreen()
        {
            SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().GameOverScene, false);
        }

        public void LegacyChoiceScreen()
        {
            SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().NewGameScene, true);
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
                    SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().QuoteScene, false);
                else
                    SceneChange(Registry.Get<Compendium>().GetSingleEntity<Dictum>().LogoScene, false);
            }
        }
    }



}