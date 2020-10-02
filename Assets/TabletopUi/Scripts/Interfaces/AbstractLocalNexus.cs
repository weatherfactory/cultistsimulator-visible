using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Noon;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Assets.TabletopUi.Scripts.Interfaces
{

    

   public abstract class LocalNexus : MonoBehaviour
   {
       [SerializeField] public UnityEvent ViewFilesEvent;
       [SerializeField] public UnityEvent ToggleOptionsEvent;
       [SerializeField] public UnityEvent SaveAndExitEvent;
       [SerializeField] public UnityEvent ToggleDebugEvent;
       [SerializeField] public UnityEvent StackCardsEvent;
       [SerializeField] public SpeedControlEvent SpeedControlEvent;
       [SerializeField] public UILookAtMeEvent UILookAtMeEvent;
       [SerializeField] public ZoomEvent ZoomEvent;
       [SerializeField] public TruckEvent TruckEvent;
       [SerializeField] public PedestalEvent PedestalEvent;
       public UnityEvent<bool> OnChangeDragStateEvent;

       public event System.Action<bool> onChangeDragState;
       public Camera dragCamera;
       public AbstractToken itemBeingDragged;
       public bool draggingEnabled = true;
       public bool resetToStartPos = false;
       private string resetToStartPosReason = null;	// For debug purposes only - CP

        public void Awake()
        {
       var registry = new Registry();
       registry.Register(this);

        }

        
        public void SetReturn(bool value, string reason = "")
        {
            resetToStartPos = value;
            resetToStartPosReason = reason;	// So that we can see why this variable was last changed... - CP
            NoonUtility.Log("AbstractToken::SetReturn( " + value + ", " + reason + " )", 0, VerbosityLevel.Trivia);
            //log here if necessary
        }

        public void ChangeDragState(bool newValue)
        {
            onChangeDragState?.Invoke(newValue);
        }

        public void CancelDrag()
        {
            if (itemBeingDragged == null)
                return;

            if (itemBeingDragged.gameObject.activeInHierarchy) itemBeingDragged.DelayedEndDrag();
        }
    }
}
