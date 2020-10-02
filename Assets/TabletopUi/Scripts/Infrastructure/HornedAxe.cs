using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
   public class HornedAxe
    {
        public static event System.Action<bool> onChangeDragState;
        public static Camera dragCamera;
        public static AbstractToken itemBeingDragged;
        public static bool draggingEnabled = true;
        public static bool resetToStartPos = false;
        private static string resetToStartPosReason = null;	// For debug purposes only - CP

        public static void SetReturn(bool value, string reason = "") {
            resetToStartPos = value;
            resetToStartPosReason = reason;	// So that we can see why this variable was last changed... - CP
            NoonUtility.Log( "AbstractToken::SetReturn( " + value + ", " + reason + " )",0,VerbosityLevel.Trivia );
            //log here if necessary
        }

        public static void ChangeDragState(bool newValue)
        {
            onChangeDragState?.Invoke(newValue);
        }

        public static void CancelDrag() {
            if (itemBeingDragged == null)
                return;

            if (itemBeingDragged.gameObject.activeInHierarchy) itemBeingDragged.DelayedEndDrag();
        }
    }
}
