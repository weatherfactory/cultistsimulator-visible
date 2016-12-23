using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;

public class TokenAnimationToSlot : TokenAnimation {

	public event System.Action<DraggableToken, IRecipeSlot> onAnimSlotDone;

	IRecipeSlot targetSlot;

	public void SetTargetSlot(IRecipeSlot targetSlot) {
		this.targetSlot = targetSlot;
	}

	protected override void FireCompleteEvent() {
		if (onAnimSlotDone != null)
			onAnimSlotDone(token, targetSlot);
	}
}
