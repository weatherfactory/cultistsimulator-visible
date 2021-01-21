using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Interfaces;
using SecretHistories.Spheres;

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
                return destinationLocation.Anchored3DPosition;
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
