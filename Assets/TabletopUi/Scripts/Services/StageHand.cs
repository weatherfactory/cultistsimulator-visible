using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.TabletopUi.Scripts.Services
{
    public class StageHand:MonoBehaviour
    {
        private int currentSceneIndex;
        public void SceneChange(int sceneToLoad)
        {
            if (currentSceneIndex > 0)
                SceneManager.UnloadSceneAsync(currentSceneIndex);


            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);

            currentSceneIndex = sceneToLoad;
        }
    }
}