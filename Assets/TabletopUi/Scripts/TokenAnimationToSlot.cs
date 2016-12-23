using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

public class TokenAnimationToSlot : TokenAnimation {

	public event System.Action<ElementStackToken, TokenAndSlot> onElementSlotAnimDone;

	TokenAndSlot targetTokenSlotPair;

	public void SetTargetSlot(TokenAndSlot targetTokenSlotPair) {
		this.targetTokenSlotPair = targetTokenSlotPair;
	}

	protected override void FireCompleteEvent() {
		if (onElementSlotAnimDone != null)
			onElementSlotAnimDone(token as ElementStackToken, targetTokenSlotPair);
	}
}
