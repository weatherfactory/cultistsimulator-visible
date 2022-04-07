using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public class WorkstationManifestation: BasicManifestation, IManifestation
    {


        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
            
        }

        public void Initialise(IManifestable manifestable)
        {
            name = GetType().Name + manifestable.Id;

        }

        public void UpdateVisuals(IManifestable manifestable, Sphere sphere)
        {
        }


        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public bool NoPush { get; }
        public void Unshroud(bool instant)
        {
        }

        public void Shroud(bool instant)
        {
        }

        public void Emphasise()
        {
        }

        public void Understate()
        {
        }

        public bool RequestingNoDrag => false;
        public bool RequestingNoSplit => true;
 
        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public IGhost CreateGhost()
        {
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(WorkstationGhost), this.RectTransform);
            return newGhost;
        }

        public OccupiesSpaceAs OccupiesSpaceAs() => Enums.OccupiesSpaceAs.LargePhysicalObject;
    }
}
