using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using System;
using Assets.Core.Enums;
using Noon;

public class SituationStorage : TokenContainer
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


    public override string GetPath()
    {
        return "storage";
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
