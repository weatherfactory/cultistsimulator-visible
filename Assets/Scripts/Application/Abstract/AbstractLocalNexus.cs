using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SecretHistories.Fucine
{

    

   public abstract class LocalNexus : MonoBehaviour
   {
       [SerializeField] public UnityEvent ViewFilesEvent;
       [SerializeField] public UnityEvent ToggleOptionsEvent;
       [SerializeField] public UnityEvent SaveAndExitEvent;
       [SerializeField] public UnityEvent AbortEvent;
        [SerializeField] public UnityEvent ToggleDebugEvent;
       [SerializeField] public UnityEvent StackCardsEvent;
       [SerializeField] public UnityEvent LockInteractionEvent;
       [SerializeField] public UnityEvent UnlockInteractionEvent;
       [SerializeField] public UnityEvent ShowHudEvent;
       [SerializeField] public UnityEvent HideHudEvent;
        [SerializeField] public SpeedControlEvent SpeedControlEvent;
       [SerializeField] public ZoomLevelEvent ZoomLevelEvent;
       [SerializeField] public TruckEvent TruckEvent;
       [SerializeField] public PedestalEvent PedestalEvent;


       protected bool playerInputDisabled = false;
       protected Coroutine _enablePlayerInputAfterDelayCoroutine;

        public void Awake()
        {
       var registry = new Watchman();
       registry.Register(this);

        }

        //0 seconds means 'until further notice'
        public void DisablePlayerInput(float forSeconds)
        {
            if (_enablePlayerInputAfterDelayCoroutine != null)
                StopCoroutine(_enablePlayerInputAfterDelayCoroutine);


            if (forSeconds > 0)
                _enablePlayerInputAfterDelayCoroutine = StartCoroutine(EnablePlayerInputAfterDelay(forSeconds));

            playerInputDisabled = true;
            LockInteractionEvent.Invoke();
        }

        protected IEnumerator EnablePlayerInputAfterDelay(float afterDelay)
        {
            yield return new WaitForSeconds(afterDelay);
            playerInputDisabled = false; //this may re-enable things we don't want
            UnlockInteractionEvent.Invoke();
        }

        public void EnablePlayerInput()
        {
            if (_enablePlayerInputAfterDelayCoroutine != null)
                StopCoroutine(_enablePlayerInputAfterDelayCoroutine);

            playerInputDisabled = false; //this may re-enable things we don't want
            UnlockInteractionEvent.Invoke();
        }

        public void EnablePlayerInputAndUnmetapause()
        {
            EnablePlayerInput();
            Watchman.Get<Heart>().Unmetapause();
        }

        public void DoShowHud()
        {
            ShowHudEvent.Invoke();
        }

        public void DoHideHud()
        {
            HideHudEvent.Invoke();
        }

    }
}
