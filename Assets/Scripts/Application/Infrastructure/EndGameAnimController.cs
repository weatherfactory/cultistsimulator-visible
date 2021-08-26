using UnityEngine;
using System.Collections;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using SecretHistories.Fucine;
using SecretHistories.Services;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SecretHistories.Constants {
    public class EndGameAnimController : MonoBehaviour {
#pragma warning disable 649
        [SerializeField] Vector2 targetPosOffset = new Vector2(0f, -150f);

        [Header("Controllers")]
        [SerializeField] private TabletopManager _tabletopManager;
        [SerializeField] private UIController _uiController;
        
        [Header("Visuals")]
        [SerializeField] private Canvas tableCanvas;
        [SerializeField] private Canvas menuCanvas;

#pragma warning restore 649

        bool isEnding = false;
        public void TriggerEnd(RectTransform focusOnTransform, Ending ending) {
            if (isEnding)
                return;

            isEnding = true;
            StartCoroutine(DoEndGameAnim(focusOnTransform, ending));
        }

        IEnumerator DoEndGameAnim(RectTransform focusOnTransform, Ending ending) {
            const float zoomDuration = 5f;
            const float fadeDuration = 2f;





            // pause game
          Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel =3 , GameSpeed = GameSpeed.Paused, WithSFX =false });



         
     
            

           // cameraZoom.StartFixedZoom(0f, zoomDuration);

            var menuBarCanvasGrp = menuCanvas.GetComponent<CanvasGroup>();




            menuBarCanvasGrp.alpha = 0f;
            

            // TODO: Put the fade into the while loop so that on aborting the zoom still continues
            
            yield return new WaitForSeconds(fadeDuration);

            

        }



    }
}
