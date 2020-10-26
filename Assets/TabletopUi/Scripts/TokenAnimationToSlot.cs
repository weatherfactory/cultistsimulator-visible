using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;

public class TokenAnimationToSlot : TokenAnimation {

	public event System.Action<ElementStackToken, TokenLocation, TokenContainer> onElementSlotAnimDone;

    private TokenLocation destinationLocation;
    private TokenContainer destinationSlot;

    protected override Vector3 endPos {
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

    public override void StartAnim(float duration = 1) {
        base.StartAnim(duration);

        transform.SetAsLastSibling();
    }

    public void SetDestination(TokenLocation destination,TokenContainer slot)
    {
        destinationLocation = destination;
        destinationSlot = slot;
    }

    protected override void FireCompleteEvent() {
		if (onElementSlotAnimDone != null)
			onElementSlotAnimDone(token as ElementStackToken, destinationLocation, destinationSlot);
	}
}
