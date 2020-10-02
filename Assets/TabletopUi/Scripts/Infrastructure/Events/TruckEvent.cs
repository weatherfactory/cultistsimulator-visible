using System;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure
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