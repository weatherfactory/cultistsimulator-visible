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
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;

public class SituationStorage : Sphere,ISituationSubscriber
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

    public void Initialise(Situation situation)
    { 
        situation.AddSubscriber(this);
        situation.AttachSphere(this);
    }


    public override SpherePath GetPath()
    {
        if (!string.IsNullOrEmpty(PathIdentifier))
            NoonUtility.Log($"We're trying to specify a spherepath ({PathIdentifier}) in a storage sphere)");

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


    public void SituationStateChanged(Situation situation)
    {
        UpdateDisplay(situation);
    }

    public void TimerValuesChanged(Situation s)
    {
        //
    }

    public void SituationSphereContentsUpdated(Situation s)
    {
       //
    }

    public void ReceiveNotification(INotification n)
    {
        //
    }
}
