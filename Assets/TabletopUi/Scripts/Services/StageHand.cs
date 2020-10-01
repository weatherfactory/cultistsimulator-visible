using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Noon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Services
{

    public enum SourceForGameState
    {
        NewGame=-1,
        DefaultSave=0,
        DevSlot1=1,
        DevSlot2=2,
        DevSlot3=3,
        DevSlot4=5,
        DevSlot5=5,
        DevSlot6=6,
        DevSlot7=7,
        DevSlot8=8,
        DevSlot9=9,
        DevSlot10=10
    }

    public class StageHand:MonoBehaviour
    {
        [Header("Fade Visuals")]
        public Image fadeOverlay;
        public float fadeDuration = 0.25f;

        

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



        private async void SceneChange(int sceneToLoad,bool withFadeEffect)
        {
            Registry.Get<StackManagersCatalogue>().Reset();

            //if (currentSceneIndex > 0)
            //    SceneManager.UnloadSceneAsync(currentSceneIndex);

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
           SceneChange(SceneNumber.InfoScene,false);
        }

        public void LoadGameOnTabletop(SourceForGameState source)
        {
            SoundManager.PlaySfx("UIStartGame");
            SourceForGameState = source;
            SceneChange(SceneNumber.TabletopScene, true);
        }


        public void NewGameOnTabletop()
        {
            SourceForGameState = SourceForGameState.NewGame;
            SceneChange(SceneNumber.TabletopScene, true);
        }

        public void ClearRestartingGameFlag()
        {
            SourceForGameState = SourceForGameState.DefaultSave;
        }



        public void LogoScreen()
        {
            SceneChange(SceneNumber.LogoScene,false);
        }


        public void QuoteScreen()
        {
            SceneChange(SceneNumber.QuoteScene,false);
        }

        public void MenuScreen()
        {
            SceneChange(SceneNumber.MenuScene,false);
        }


        public bool SceneIsActive(int sceneToCheck)
        {
            return SceneManager.GetSceneByBuildIndex(sceneToCheck).isLoaded;
        }


        public void EndingScreen()
        {
            SceneChange(SceneNumber.GameOverScene,false);
        }

        public void LegacyChoiceScreen()
        {
            SceneChange(SceneNumber.NewGameScene,true);
        }


        public void LoadFirstScene(bool skipLogo)
        {

            if (Application.isEditor)
            {
                if (StartingSceneNumber > 0)
                    SceneChange(StartingSceneNumber,true);
            }
            
            else
            {

                if (skipLogo) // This will allocate and read in config.ini
                    SceneChange(SceneNumber.QuoteScene,false);
                else
                    SceneChange(SceneNumber.LogoScene,false);
            }
        }
    }



}