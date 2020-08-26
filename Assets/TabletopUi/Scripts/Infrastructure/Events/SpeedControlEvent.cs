using System;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    [Serializable]
    public class SpeedControlEvent : UnityEvent<GameSpeed>
    {
    }
}