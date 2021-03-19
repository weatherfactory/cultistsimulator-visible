using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using TMPro;
using UnityEngine;

public class DragDebug : MonoBehaviour,ISphereCatalogueEventSubscriber
{
    public TextMeshProUGUI currentlyDragging;
    public TextMeshProUGUI currentSphere;
    public TextMeshProUGUI positioningText;


    // Start is called before the first frame update
    void Start()
    {
        Watchman.Get<HornedAxe>().Subscribe(this);
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
        if (args.Token != null && args.Token.ManifestationRectTransform != null && args.Sphere!=null)
        {
            var localPosition = args.Token.TokenRectTransform.localPosition;
            
            string lpstring = $"{Math.Round(localPosition.x, 0)}, {Math.Round(localPosition.y, 0)}, {Math.Round(localPosition.z, 0)}";

            var anchoredPosition3D = args.Token.TokenRectTransform.anchoredPosition3D;

            string apstring= $"{Math.Round(anchoredPosition3D.x, 0)}, {Math.Round(anchoredPosition3D.y, 0)}, {Math.Round(anchoredPosition3D.z, 0)}";


            var position = args.Token.TokenRectTransform.position;

            string pstring = $"{Math.Round(position.x, 0)}, {Math.Round(position.y, 0)}, {Math.Round(position.z, 0)}";


            var defaultSphere = Watchman.Get<HornedAxe>().GetDefaultSphere();

            
            var projectionPosition = defaultSphere.GetRectTransform().InverseTransformPoint(position);
            string projectedPositionInTabletop = $"{Math.Round(projectionPosition.x, 0)}, {Math.Round(projectionPosition.y, 0)}, {Math.Round(projectionPosition.z, 0)}";



            string hoveringOver = string.Empty;
            var hovered = args.PointerEventData.hovered;

            if(hovered.Any())
            {
                hoveringOver = string.Empty;
                foreach (var h in hovered)
                    hoveringOver = $"{hoveringOver}\n{h.name}";
            }


            positioningText.text = $"Local: {lpstring}\n Anchored: {apstring}\n Global: {pstring} \n Projected Position on Tabletop: {projectedPositionInTabletop} \nHovering Over: {hoveringOver}";

        }

    }
}
