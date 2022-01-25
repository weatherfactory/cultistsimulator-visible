using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Assets.Scripts.Application.Tokens.Elements.Manifestations
{
 [RequireComponent(typeof(RectTransform))]
    public class MortalManifestation: BasicManifestation, IManifestation
    {

        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            throw new NotImplementedException();
        }

        public bool CanAnimateIcon()
        {
            throw new NotImplementedException();
        }

        public void BeginIconAnimation()
        {
            throw new NotImplementedException();
        }

        public void Initialise(IManifestable manifestable)
        {
           //
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            //
        }

        public void OnBeginDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void OnEndDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
            throw new NotImplementedException();
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            throw new NotImplementedException();
        }

        public bool NoPush { get; }
        public void Unshroud(bool instant)
        {
           //
        }

        public void Shroud(bool instant)
        {
            //
        }

        public void Emphasise()
        {
          //
        }

        public void Understate()
        {
          //
        }

        public bool RequestingNoDrag { get; }
        public bool RequestingNoSplit { get; }
        public void DoMove(RectTransform tokenRectTransform)
        {
            throw new NotImplementedException();
        }

        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            throw new NotImplementedException();
        }

        public IGhost CreateGhost()
        {
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(VerbGhost), this.RectTransform);
            return newGhost;
        }
    }
}
