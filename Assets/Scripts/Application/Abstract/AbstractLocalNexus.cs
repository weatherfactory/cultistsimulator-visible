using System;
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

namespace SecretHistories.Interfaces
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



        public void Awake()
        {
       var registry = new Registry();
       registry.Register(this);

        }



    }
}
