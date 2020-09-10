using System;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    

    public class ZoomEventArgs
    {
        public static float ZOOM_CLOSE = 0.01f;
        public static float ZOOM_MID = 0.4f;
        public static float ZOOM_FAR = 1f;
        public static float ZOOM_INCREMENT = 0.025f;


        public float OngoingZoomIncrement { get; set; }

        public float AbsoluteTargetZoomLevel { get; set; }
    }

    [Serializable]
    public class ZoomEvent : UnityEvent<ZoomEventArgs>
    {
    }
}