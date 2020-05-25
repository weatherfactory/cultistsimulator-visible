using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
  public  class HighlightLocationsController:MonoBehaviour, IStacksChangeSubscriber
    {

 private HashSet<HighlightLocation> highlightLocations;
      

      public HighlightLocationsController()
      {
          highlightLocations = new HashSet<HighlightLocation>();
      }

      public void Initialise(StackManagersCatalogue smc)
      {
            smc.Subscribe(this);

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

      public void NotifyStacksChanged()
      {
          var ttm = Registry.Retrieve<ITabletopManager>();
          var aspectsInContext = ttm.GetAspectsInContext(new AspectsDictionary());

          foreach(var hl in highlightLocations)
          {
              if (aspectsInContext.AspectsExtant.ContainsKey(hl.DisplayWhileElementIdPresent))
                  hl.DisplayForPresence(0f);
              else
                  hl.HideForNoPresence();
          }
      }
    }
}
