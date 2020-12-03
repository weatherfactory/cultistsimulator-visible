using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;

public class TokenTravellingToSlot : TokenTravelAnimation {


    private TokenLocation destinationLocation;
    private Sphere destinationSlot;

    protected override Vector3 EndPosition {
        get
        {
            //target token and/or slot might conceivably have been destroyed en route
            //This should really be upstream, because it doesn't stop the scale shrinking
            if (destinationSlot == null || destinationSlot.Defunct)
                return transform.localPosition;
            else
                return destinationLocation.Position;
        }
    }

    public void SetDestination(TokenLocation destination,Sphere slot)
    {
        destinationLocation = destination;
        destinationSlot = slot;
    }

 //   protected override void FireCompleteEvent() {
	//	if (onElementSlotAnimDone != null)
	//		onElementSlotAnimDone(token, destinationLocation, destinationSlot);
	//}
}
