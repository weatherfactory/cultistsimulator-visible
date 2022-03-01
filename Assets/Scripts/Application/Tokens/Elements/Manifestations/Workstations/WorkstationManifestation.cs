using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public class WorkstationManifestation: BasicManifestation, IManifestation
    {
        [SerializeField] protected List<ShelfDominion> _dominions;

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
            foreach (var d in _dominions)
                d.RegisterFor(manifestable);
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
        }

        public void OnBeginDragVisuals(Token token)
        {
        }

        public void OnEndDragVisuals(Token token)
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
        public void DoMove(RectTransform tokenRectTransform)
        {
        }

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
    }
}
