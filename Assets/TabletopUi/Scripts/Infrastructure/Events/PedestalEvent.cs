using System;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure
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