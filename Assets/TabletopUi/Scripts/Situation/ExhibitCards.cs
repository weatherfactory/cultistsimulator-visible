﻿#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;

public class ExhibitCards : AbstractTokenContainer {

    public override bool AllowDrag { get { return false; } }
    public override bool AllowStackMerge { get { return false; } }

    public override void Initialise()
    {
        _elementStacksManager = new ElementStacksManager(this, "exhibits");
    }

    public override string GetSaveLocationInfoForDraggable(DraggableToken draggable)
    {
        return string.Empty;
    }

    public override void DisplayHere(IElementStack stack, Context context)
    {
        base.DisplayHere(stack, context);

        // We ensure all stored cards are always face down
        stack.FlipToFaceDown(true);
    }

}
