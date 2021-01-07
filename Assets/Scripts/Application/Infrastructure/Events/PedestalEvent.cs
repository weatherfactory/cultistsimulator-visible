using System;
using UnityEngine.Events;

namespace SecretHistories.Constants
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