using Assets.CS.TabletopUI;
using Noon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Services
{



    public class StageHand:MonoBehaviour
    {
        [Header("Fade Visuals")]
        public Image fadeOverlay;
        public float fadeDuration = 0.25f;


        private const int TabletopScene = 4;

        private int sceneQueuedToLoad = 0;

        public int StartingSceneNumber;

        public bool RestartingGameFlag;

        public void SceneChange(int sceneToLoad)
        {
            //if (currentSceneIndex > 0)
            //    SceneManager.UnloadSceneAsync(currentSceneIndex);

            if(SceneManager.sceneCount > 2)
                NoonUtility.Log("More than 2 scenes loaded",2);

            else if (SceneManager.sceneCount==2)
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));


            if (sceneQueuedToLoad == SceneNumber.TabletopScene)
                SoundManager.PlaySfx("UIStartGame");

            sceneQueuedToLoad = sceneToLoad;

            Invoke("SceneChangeDelayed", fadeDuration);

            FadeOut();


            //// make sure the screen is black
            //fadeOverlay.gameObject.SetActive(true);
            //fadeOverlay.canvasRenderer.SetAlpha(1f);

            //// We delay the showing to get a proper fade in
            //Invoke("UpdateAndShowMenu", 0.1f);
        }

        void SceneChangeDelayed()
        {
            SceneManager.LoadScene(sceneQueuedToLoad, LoadSceneMode.Additive);
        }

 



        public void RestartGame()
        {
            RestartingGameFlag = true;
            SceneChange(TabletopScene);
        }

        public void ClearRestartingGameFlag()
        {
            RestartingGameFlag = false;
        }


        void FadeIn()
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.canvasRenderer.SetAlpha(1f);
            fadeOverlay.CrossFadeAlpha(0, fadeDuration, true);
        }

        void FadeOut()
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.canvasRenderer.SetAlpha(0f);
            fadeOverlay.CrossFadeAlpha(1, fadeDuration, true);
        }


        public void MenuScreen()
        {
            SceneChange(SceneNumber.MenuScene);
        }


        public void TabletopScreen()
        {
            SceneChange(SceneNumber.TabletopScene);
        }


        public void EndingScreen()
        {
            SceneChange(SceneNumber.GameOverScene);
        }

        public void LegacyChoiceScreen()
        {
            SceneChange(SceneNumber.NewGameScene);
        }


        public void LoadFirstScene(bool skipLogo)
        {

            if (Application.isEditor)
            {
                if (StartingSceneNumber > 0)
                    SceneChange(StartingSceneNumber);
            }
            
            else
            {

                if (skipLogo) // This will allocate and read in config.ini
                    SceneChange(SceneNumber.QuoteScene);
                else
                    SceneChange(SceneNumber.LogoScene);
            }
        }
    }



}