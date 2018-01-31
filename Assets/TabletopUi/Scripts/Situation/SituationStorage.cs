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

    public override void Initialise() {
        _elementStacksManager = new ElementStacksManager(this, "storage");
    }

    public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return "slot_storage";
    }

}
