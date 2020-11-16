#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;

public class ExhibitCards : Sphere {

    public override bool AllowDrag { get { return false; } }
    public override bool AllowStackMerge { get { return false; } }
    public override bool AlwaysShowHoverGlow { get { return true; } }

    
    public override SphereCategory SphereCategory => SphereCategory.Meta;


    public override void DisplayHere(Token token, Context context)
    {
        base.DisplayHere(token, context);
        token.Understate();
    }

    public void HighlightCardWithId(string elementId)
    {
        var cards = GetStackTokens();

        foreach (var card in cards)
        {
            if (card.Element.Id == elementId)
                card.Emphasise();
            else
                card.Understate();
        }

    }
}
