using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI
{
  public  class HighlightLocationsController:MonoBehaviour, ISphereCatalogueEventSubscriber
    {

 private HashSet<HighlightLocation> highlightLocations;
      

      public HighlightLocationsController()
      {
          highlightLocations = new HashSet<HighlightLocation>();
      }

      public void Start()
      {
            Registry.Get<SphereCatalogue>().Subscribe(this);

          //scan for child highlight locations
          var hls = GetComponentsInChildren<HighlightLocation>();
          foreach (var hl in hls)
          {
              AddHighlightLocation(hl);
              hl.HideCompletely();
          }

      }

      public void AddHighlightLocation(HighlightLocation newHl)
      {
          if(highlightLocations.All(hl => hl.HighlightWhileElementIdInteracting != newHl.HighlightWhileElementIdInteracting))
            highlightLocations.Add(newHl);
      }

      public void DeactivateHighlightLocations()
      {
          foreach (var hl in highlightLocations)
                  hl.HideForNoInteraction();
      }

      public void ActivateMatchingHighlightLocation(string elementId)
      {
          var hlToActivate = highlightLocations.SingleOrDefault(hl =>(hl.HighlightWhileElementIdInteracting == elementId || hl.DisplayWhileElementIdPresent == elementId));
          if(hlToActivate!=null)
              hlToActivate.HighlightForInteracted();
      }

      public void DeactivateMatchingHighlightLocation(string elementId)
      {
          var hlToActivate = highlightLocations.SingleOrDefault(hl => (hl.HighlightWhileElementIdInteracting == elementId || hl.DisplayWhileElementIdPresent == elementId));
          if (hlToActivate != null)
          { 
              hlToActivate.HideForNoInteraction();
          }
      }

      public void ActivateOnlyMatchingHighlightLocation(string elementId)
      {
          DeactivateHighlightLocations();
          ActivateMatchingHighlightLocation(elementId);
      }



      public void NotifyTokensChanged(SphereContentsChangedEventArgs args)
      {
          var tc = Registry.Get<SphereCatalogue>();
          var aspectsInContext = tc.GetAspectsInContext(new AspectsDictionary());

          foreach (var hl in highlightLocations)
          {
              if (aspectsInContext.AspectsExtant.ContainsKey(hl.DisplayWhileElementIdPresent))
                  hl.DisplayForPresence(0f);
              else
              {
                  hl.HideForNoPresence();
                    hl.HideForNoInteraction(); //in case we were interacting with it and it got retired
              }
          }
      }

      public void OnTokenInteraction(TokenInteractionEventArgs args)
      {
          if(args.Interaction==Interaction.OnPointerEntered)
            if (!args.PointerEventData.dragging)
              ActivateOnlyMatchingHighlightLocation(args.Element.Id);

          if (args.Interaction == Interaction.OnPointerExited)
              if (!args.PointerEventData.dragging)
                  DeactivateMatchingHighlightLocation(args.Element.Id);
      }



    }
}
