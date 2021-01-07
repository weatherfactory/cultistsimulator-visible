using System;
using SecretHistories.Enums;
using UnityEngine.Events;

namespace SecretHistories.Constants
{
    public class SpeedControlEventArgs
    {
        public int ControlPriorityLevel { get; set; }
        public GameSpeed GameSpeed { get; set; }
        public bool WithSFX { get; set; }
    }
    [Serializable]
    public class SpeedControlEvent : UnityEvent<SpeedControlEventArgs>
    {
    }

}