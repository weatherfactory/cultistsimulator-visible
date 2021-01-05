#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.UI.Scripts;
using SecretHistories.Services;
using SecretHistories.Infrastructure;

using TMPro;

public class ExhibitCards : Sphere {

    public override bool AllowDrag { get { return false; } }
    public override bool AllowStackMerge { get { return false; } }


    public override SphereCategory SphereCategory => SphereCategory.Meta;


    public override void DisplayAndPositionHere(Token token, Context context)
    {
        base.DisplayAndPositionHere(token, context);
        token.Understate();
    }

    public void HighlightCardWithId(string elementId)
    {
        var cards = GetElementTokens();

        foreach (var card in cards)
        {
            if (card.Element.Id == elementId)
                card.Emphasise();
            else
                card.Understate();
        }

    }
}
