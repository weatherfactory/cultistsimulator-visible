using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public interface IAnchorManifestation
    {
        void DisplayVisuals(IVerb verb);
        void ResetAnimations();
        bool Retire(CanvasGroup canvasGroup);
        void SetVfx(CardVFX vfxName);
        void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged);
        void BeginArtAnimation(string icon);
        bool CanAnimate();
        void OnBeginDragVisuals();
        void OnEndDragVisuals();
        void Highlight(HighlightType highlightType);
        void Unhighlight(HighlightType highlightType);
        bool NoPush { get; }
        void DoRevealEffect(bool instant);
        void DoShroudEffect(bool instant);
        bool RequestingNoDrag { get; }
    }
}
