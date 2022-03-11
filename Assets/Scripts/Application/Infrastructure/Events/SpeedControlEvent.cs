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

        public static SpeedControlEventArgs ArgsForPause()
        {
            //Always use ControlPriorityLevel 2 for code-based Pause, otherwise unpausing can get buggered up
            //REMEMBER THE MANSUS
            var args = new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused };
            return args;
        }
    }
    [Serializable]
    public class SpeedControlEvent : UnityEvent<SpeedControlEventArgs>
    {
    }

}