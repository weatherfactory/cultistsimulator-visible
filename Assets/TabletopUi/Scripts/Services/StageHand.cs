using Assets.CS.TabletopUI;
using Noon;
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
            //if (currentSceneIndex > 0)
            //    SceneManager.UnloadSceneAsync(currentSceneIndex);

            if(SceneManager.sceneCount > 2)
                NoonUtility.Log("More than 2 scenes loaded",2);

            else if (SceneManager.sceneCount==2)
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
            


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