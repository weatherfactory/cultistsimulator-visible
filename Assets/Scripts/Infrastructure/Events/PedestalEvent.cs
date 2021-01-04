using System;
using UnityEngine.Events;

namespace SecretHistories.Infrastructure
{
    public class PedestalEventArgs
    {

        public float CurrentPedestalInput { get; set; }

    }

    [Serializable]
    public class PedestalEvent : UnityEvent<PedestalEventArgs>
    {
    }
}