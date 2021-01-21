using UnityEngine;
using System.Collections;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;
using System;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Spheres;


public class SituationStorage : Sphere,ISituationSubscriber
{
    //because this is just a sphere, I don't think it needs to be an ISituationAttachment
    public override SphereCategory SphereCategory => SphereCategory.SituationStorage;

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
