using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
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
        Watchman.Get<SphereCatalogue>().Subscribe(this);
    }



    public void NotifyTokensChanged(SphereContentsChangedEventArgs args)
    {
     
    }

    public void OnTokenInteraction(TokenInteractionEventArgs args)
    {
      if(args.Interaction==Interaction.OnClicked || args.Interaction==Interaction.OnDrag || args.Interaction==Interaction.OnDragBegin || args.Interaction == Interaction.OnDragEnd)
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
