using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
  public  class HighlightLocationsController:MonoBehaviour
  {

 private HashSet<HighlightLocation> highlightLocations;
      

      public HighlightLocationsController()
      {
          highlightLocations = new HashSet<HighlightLocation>();
      }

      public void ScanChildHighlightLocations()
      {
          var hls = GetComponentsInChildren<HighlightLocation>();
          foreach (var hl in hls)
          {
              AddHighlightLocation(hl);
              hl.Deactivate();
          }

      }

      public void AddHighlightLocation(HighlightLocation newHl)
      {
          if(highlightLocations.All(hl => hl.MatchElementId != newHl.MatchElementId))
            highlightLocations.Add(newHl);
      }

      public void DeactivateAllHighlightLocations()
      {
          foreach (var hl in highlightLocations)
                  hl.Deactivate(0f);
      }

      public void ActivateMatchingHighlightLocation(string elementId)
      {

            var hlToActivate = highlightLocations.SingleOrDefault(hl => hl.MatchElementId == elementId);
          if(hlToActivate!=null)
              hlToActivate.Activate();
      }

      public void ActivateOnlyMatchingHighlightLocation(string elementId)
      {
          DeactivateAllHighlightLocations();
          ActivateMatchingHighlightLocation(elementId);
      }
    }
}
