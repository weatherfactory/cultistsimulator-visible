using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using TMPro;
using UnityEngine;

public class DragDebug : MonoBehaviour,ISphereCatalogueEventSubscriber
{
    public TextMeshProUGUI currentlyDragging;
    public TextMeshProUGUI currentSphere;
    public TextMeshProUGUI currentAnchorPoint3D;


    // Start is called before the first frame update
    void Start()
    {
        Registry.Get<SphereCatalogue>().Subscribe(this);
    }



    public void NotifyTokensChanged(TokenInteractionEventArgs args)
    {
     
    }

    public void OnTokenInteraction(TokenInteractionEventArgs args)
    {
      if(args.Interaction==Interaction.OnClicked || args.Interaction==Interaction.OnDrag)
          DisplayDetails(args);
    }




    private void DisplayDetails(TokenInteractionEventArgs args)
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
