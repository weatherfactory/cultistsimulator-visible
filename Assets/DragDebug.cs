using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using TMPro;
using UnityEngine;

public class DragDebug : MonoBehaviour,ISphereEventSubscriber
{
    public TextMeshProUGUI currentlyDragging;
    public TextMeshProUGUI currentSphere;
    public TextMeshProUGUI currentAnchorPoint3D;


    // Start is called before the first frame update
    void Start()
    {
        Registry.Get<SphereCatalogue>().Subscribe(this);
    }



    public void NotifyTokensChangedForSphere(TokenEventArgs args)
    {
     
    }

    public void OnTokenClicked(TokenEventArgs args)
    {
        DisplayDetails(args);

    }

    public void OnTokenReceivedADrop(TokenEventArgs args)
    {
       
    }

    public void OnTokenPointerEntered(TokenEventArgs args)
    {
    }

    public void OnTokenPointerExited(TokenEventArgs args)
    {
    }

    public void OnTokenDoubleClicked(TokenEventArgs args)
    {
    }

    public void OnTokenDragged(TokenEventArgs args)
    {

        DisplayDetails(args);

    }

    private void DisplayDetails(TokenEventArgs args)
    {
        currentlyDragging.text = args.Token?.name;
        currentSphere.text = args.Sphere?.name;
        if (args.Token != null && args.Token.ManifestationRectTransform != null)
        {
            var position = args.Token.TokenRectTransform.anchoredPosition3D;

            currentAnchorPoint3D.text = $"{Math.Round(position.x, 0)}, {Math.Round(position.y, 0)}, {Math.Round(position.z, 0)}";

        }

    }
}
