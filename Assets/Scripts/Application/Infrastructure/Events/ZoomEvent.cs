using System;
using UnityEngine.Events;

namespace SecretHistories.Constants
{
    public enum ZoomLevel
    {
       Unspecified=0, Far=1,Mid=2,Close=3
    }

    public class ZoomLevelEventArgs
    {
     
        public float CurrentZoomInput { get; set; }

        public ZoomLevel AbsoluteTargetZoomLevel { get; set; }
    }


    [Serializable]
    public class ZoomLevelEvent : UnityEvent<ZoomLevelEventArgs>
    {
    }





}