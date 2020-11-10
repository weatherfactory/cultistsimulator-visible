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
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Services;
using Noon;

public class SituationStorage : Sphere
{
    public override Type ElementManifestationType => typeof(StoredManifestation);

    public override bool AllowDrag
    {
        get { return false; }
    }

    public override bool AllowStackMerge
    {
        get { return false; }
    }

    [SerializeField] private CanvasGroupFader canvasGroupFader;




    public override string GetPath()
    {
        return SphereCatalogue.STORAGE_PATH;
    }

    public override ContainerCategory ContainerCategory => ContainerCategory.SituationStorage;

    public void UpdateDisplay(SituationEventData eventData)
    {
        if(eventData.SituationState==SituationState.Ongoing)
            canvasGroupFader.Show();
        else
            canvasGroupFader.Hide();
    }

   

}
