using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public class VerbManifestation: MonoBehaviour, IAnchorManifestation
    {
        public void DisplayVisuals(IVerb verb)
        {
            throw new NotImplementedException();
        }

        public void ResetAnimations()
        {
            throw new NotImplementedException();
        }

        public bool Retire(CanvasGroup canvasGroup)
        {
            throw new NotImplementedException();
        }

        public void SetVfx(CardVFX vfxName)
        {
            throw new NotImplementedException();
        }

        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            throw new NotImplementedException();
        }

        public void BeginArtAnimation(string icon)
        {
            throw new NotImplementedException();
        }

        public bool CanAnimate()
        {
            throw new NotImplementedException();
        }

        public void OnBeginDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void OnEndDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void Highlight(HighlightType highlightType)
        {
            throw new NotImplementedException();
        }

        public void Unhighlight(HighlightType highlightType)
        {
            throw new NotImplementedException();
        }

        public bool NoPush { get; }
        public void DoRevealEffect(bool instant)
        {
            throw new NotImplementedException();
        }

        public void DoShroudEffect(bool instant)
        {
            throw new NotImplementedException();
        }

        public bool RequestingNoDrag { get; }
    }
}
