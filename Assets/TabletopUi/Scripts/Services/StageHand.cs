using Assets.CS.TabletopUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.TabletopUi.Scripts.Services
{



    public class StageHand:MonoBehaviour
    {
        public int StartingSceneNumber;


        private int currentSceneIndex;
        public void SceneChange(int sceneToLoad)
        {
            if (currentSceneIndex > 0)
                SceneManager.UnloadSceneAsync(currentSceneIndex);


            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);

            currentSceneIndex = sceneToLoad;
        }

        public void LoadFirstScene(bool skipLogo)
        {
            if(StartingSceneNumber>0 && Application.isEditor)
                SceneChange(StartingSceneNumber);
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