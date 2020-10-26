using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;

public class TokenAnimationToSlot : TokenAnimation {

	public event System.Action<ElementStackToken, TokenLocation, TokenContainer> onElementSlotAnimDone;

    private TokenContainer destinationSlot;

    protected override Vector3 endPos {
        get
        {
            //target token and/or slot might conceivably have been destroyed en route
            //This should really be upstream, because it doesn't stop the scale shrinking
            if (targetAnchorSlotPair.Token == null || targetAnchorSlotPair.Threshold == null ||
                targetAnchorSlotPair.Token.Defunct || targetAnchorSlotPair.Threshold.Defunct)
                return transform.localPosition;
            else
                return targetAnchorSlotPair.Token.GetTargetContainerPosition();
        }
    }

    public override void StartAnim(float duration = 1) {
        base.StartAnim(duration);

        transform.SetAsLastSibling();
        targetAnchorSlotPair.Token.SituationController.NotifyGreedySlotAnim(this);
    }

    public void SetDestinationSlot(TokenContainer slot)
    {
        destinationSlot = slot;
    }

    protected override void FireCompleteEvent() {
		if (onElementSlotAnimDone != null)
			onElementSlotAnimDone(token as ElementStackToken, targetAnchorSlotPair);
	}
}
