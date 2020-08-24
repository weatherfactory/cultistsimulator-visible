using Assets.CS.TabletopUI;
using Noon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.TabletopUi.Scripts.Services
{



    public class StageHand:MonoBehaviour
    {
        private const int TabletopScene = 4;

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
            


            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);

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


        public void MenuScreen()
        {
            SceneChange(SceneNumber.MenuScene);
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