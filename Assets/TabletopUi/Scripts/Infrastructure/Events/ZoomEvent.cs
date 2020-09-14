using System;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    

    public class ZoomEventArgs
    {
        public static float ZOOM_CLOSE = 0.01f;
        public static float ZOOM_MID = 0.4f;
        public static float ZOOM_FAR = 1f;


        public float CurrentZoomInput { get; set; }

        public float AbsoluteTargetZoomLevel { get; set; }
    }

    [Serializable]
    public class ZoomEvent : UnityEvent<ZoomEventArgs>
    {
    }

    public class TruckEventArgs
    {

        public float CurrentTruckInput { get; set; }

    }

    [Serializable]
    public class TruckEvent : UnityEvent<TruckEventArgs>
    {
    }


    public class PedestalEventArgs
    {

        public float CurrentPedestalInput { get; set; }

    }

    [Serializable]
    public class PedestalEvent : UnityEvent<PedestalEventArgs>
    {
    }


}