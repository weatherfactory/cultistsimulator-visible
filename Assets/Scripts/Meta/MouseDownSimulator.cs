using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Assets.Scripts.Meta
{
   public class MouseDownSimulator: OnScreenControl
   {
       public TextMeshProUGUI OnLabel;
       public TextMeshProUGUI OffLabel;

       public void Update()
       {
           if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
               SimulateMouseDown();
       }

        public void SimulateMouseDown()
            {
                SendValueToControl(1.0f);
                OnLabel.fontStyle = FontStyles.Bold;
                OffLabel.fontStyle = FontStyles.Normal;
            }

            [InputControl]
            [SerializeField]
            private string MouseDownControlPath;

            protected override string controlPathInternal
            {
                get => MouseDownControlPath;
                set => MouseDownControlPath = value;
            }
    }
}
