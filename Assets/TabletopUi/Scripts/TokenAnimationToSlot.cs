using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

public class TokenAnimationToSlot : TokenAnimation {

	public event System.Action<ElementStackToken, TokenAndSlot> onElementSlotAnimDone;

	TokenAndSlot targetTokenSlotPair;

    protected override Vector3 endPos {
        get
        {
            //target token and/or slot might conceivably have been destroyed en route
            //This should really be upstream, because it doesn't stop the scale shrinking
            if (targetTokenSlotPair.Token == null || targetTokenSlotPair.RecipeSlot == null || targetTokenSlotPair.Token.Defunct || targetTokenSlotPair.RecipeSlot.Defunct)
                return transform.localPosition;
            else
                return targetTokenSlotPair.Token.GetOngoingSlotPosition();
        }
    }

    public override void StartAnim(float duration = 1) {
        base.StartAnim(duration);

        transform.SetAsLastSibling();
        targetTokenSlotPair.Token.SituationController.NotifyGreedySlotAnim(this);
    }

    public void SetTargetSlot(TokenAndSlot targetTokenSlotPair) {
		this.targetTokenSlotPair = targetTokenSlotPair;
    }

    protected override void FireCompleteEvent() {
		if (onElementSlotAnimDone != null)
			onElementSlotAnimDone(token as ElementStackToken, targetTokenSlotPair);
	}
}
