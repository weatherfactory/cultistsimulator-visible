using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using System;
using Noon;

public class SituationStorage : AbstractTokenContainer {

    public override bool AllowDrag { get { return false; } }
    public override bool AllowStackMerge { get { return false; } }


    public override string GetSaveLocationForToken(AbstractToken token) {
        return "slot_storage";
    }

    public override void DisplayHere(ElementStackToken stack, Context context) {
        base.DisplayHere(stack, context);

        // We ensure all stored cards are always face down
        stack.Shroud(true);
    }

}
