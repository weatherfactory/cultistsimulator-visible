using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using System;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Services;
using Noon;

public class SituationStorage : Sphere
{

    public override bool AllowDrag
    {
        get { return false; }
    }

    public override bool AllowStackMerge
    {
        get { return false; }
    }

    [SerializeField] private CanvasGroupFader canvasGroupFader;




    public override SpherePath GetPath()
    {
        if (!string.IsNullOrEmpty(pathIdentifier))
            NoonUtility.Log($"We're trying to specify a spherepath ({pathIdentifier}) in a storage sphere)");

        return new SpherePath("storage");
    }

    public override SphereCategory SphereCategory => SphereCategory.SituationStorage;

    public void UpdateDisplay(Situation situation)
    {
        if(situation.CurrentState.IsActiveInThisState(this))
            canvasGroupFader.Show();
        else
            canvasGroupFader.Hide();
    }

   

}
