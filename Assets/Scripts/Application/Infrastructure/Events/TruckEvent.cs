using System;
using UnityEngine.Events;

namespace SecretHistories.Constants
{

    public class TruckEventArgs
    {

        public float CurrentTruckInput { get; set; }
        
    }

    [Serializable]
    public class TruckEvent : UnityEvent<TruckEventArgs>
    {
    }
}