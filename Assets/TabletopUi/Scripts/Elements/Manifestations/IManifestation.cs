using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Enums;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
   public interface IManifestation
    {
        bool Retire(RetirementVFX retirementVfx);
        bool CanAnimate();
        void BeginArtAnimation();
        void ResetAnimations();
        void OnBeginDragVisuals();
        void OnEndDragVisuals();
        void Highlight(HighlightType highlightType);
        void Unhighlight(HighlightType highlightType);
        bool NoPush { get; }
        void DoRevealEffect(bool instant);
        void DoShroudEffect(bool instant);
        bool RequestingNoDrag { get; }
        void DoMove(RectTransform tokenRectTransform);

    }
}
