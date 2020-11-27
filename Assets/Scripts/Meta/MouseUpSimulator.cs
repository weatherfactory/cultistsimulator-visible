using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Assets.Scripts.Meta
{
    public class MouseUpSimulator : OnScreenControl
    {
        public TextMeshProUGUI OnLabel;
        public TextMeshProUGUI OffLabel;

        public void Update()
        {
            if(Keyboard.current.rightBracketKey.wasPressedThisFrame)
                SimulatedMouseUp();
        }
        public void SimulatedMouseUp()
        {
            SendValueToControl(0.0f);
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